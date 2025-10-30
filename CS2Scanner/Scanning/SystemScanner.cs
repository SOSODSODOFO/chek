using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace CS2Scanner.Scanning;

internal sealed class SystemScanner
{
    private static readonly string[] SuspiciousProcessKeywords =
    {
        "cheat", "injector", "aimbot", "hook", "loader", "esp", "radar", "wall", "bypass"
    };

    private static readonly string[] SuspiciousFileTargets =
    {
        @"C:\Users\Public\Downloads\injector.exe",
        @"C:\Program Files\x64\hook.dll",
        @"C:\ProgramData\CheatEngine\config.json"
    };

    private static readonly string[] SuspiciousRegistryTargets =
    {
        @"Software\CheatEngine",
        @"Software\Classes\CLSID\{BDA0234F-82A6-4F6E-B618-7DA9E54C4C1E}",
        @"SYSTEM\CurrentControlSet\Services\EasyAntiCheat"
    };

    public SystemReport Scan()
    {
        var stopwatch = Stopwatch.StartNew();

        var processes = GetProcesses();
        var suspiciousProcesses = processes
            .Where(p => SuspiciousProcessKeywords.Any(keyword =>
                p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            .Select(p => p.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => p)
            .ToList();

        var suspiciousFiles = GetSuspiciousFiles();
        var suspiciousRegistryKeys = GetSuspiciousRegistryKeys();

        stopwatch.Stop();

        var judgement = suspiciousProcesses.Count > 0 || suspiciousFiles.Any(f => f.Exists) || suspiciousRegistryKeys.Count > 0
            ? "Cheating suspected ❌"
            : "Clean ✅";

        return new SystemReport
        {
            SystemInfo = $"{RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})",
            ScanDuration = stopwatch.Elapsed.ToString("mm':'ss'.'fff"),
            MachineName = Environment.MachineName,
            VmStatus = DetectVirtualMachineStatus(),
            HardwareSpec = BuildHardwareSpec(),
            VpnStatus = DetectVpnStatus(),
            UserCount = CountLocalUsers(),
            BootTime = FormatUptime(TimeSpan.FromMilliseconds(Environment.TickCount64)),
            Judgement = judgement,
            SuspiciousProcesses = suspiciousProcesses,
            SuspiciousFiles = suspiciousFiles,
            SuspiciousRegistryKeys = suspiciousRegistryKeys,
            Processes = processes
        };
    }

    private static List<ProcessEntry> GetProcesses()
    {
        var list = new List<ProcessEntry>();
        foreach (var process in Process.GetProcesses())
        {
            using (process)
            {
                string path = string.Empty;
                try
                {
                    path = process.MainModule?.FileName ?? string.Empty;
                }
                catch
                {
                    // ignored
                }

                double memoryMb = 0;
                try
                {
                    memoryMb = process.WorkingSet64 / 1024d / 1024d;
                }
                catch
                {
                    // ignored
                }

                list.Add(new ProcessEntry(process.ProcessName, process.Id, path, Math.Round(memoryMb, 1)));
            }
        }

        list.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
        return list;
    }

    private static List<FileEntry> GetSuspiciousFiles()
    {
        var files = new List<FileEntry>();
        foreach (var target in SuspiciousFileTargets)
        {
            try
            {
                if (File.Exists(target))
                {
                    var info = new FileInfo(target);
                    files.Add(new FileEntry(target, FormatBytes(info.Length), true));
                }
                else
                {
                    files.Add(new FileEntry(target, "—", false));
                }
            }
            catch
            {
                files.Add(new FileEntry(target, "Ошибка чтения", false));
            }
        }

        return files;
    }

    private static List<string> GetSuspiciousRegistryKeys()
    {
        var keys = new List<string>();
        try
        {
            foreach (var target in SuspiciousRegistryTargets)
            {
                using var hive = target.StartsWith("SYSTEM", StringComparison.OrdinalIgnoreCase)
                    ? Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, Microsoft.Win32.RegistryView.Registry64)
                    : Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.CurrentUser, Microsoft.Win32.RegistryView.Registry64);

                var relativePath = target;

                using var key = hive.OpenSubKey(relativePath, false);
                if (key != null)
                {
                    keys.Add(target);
                }
            }
        }
        catch
        {
            // ignored on non-Windows or restricted environments
        }

        return keys;
    }

    private static string DetectVirtualMachineStatus()
    {
        try
        {
            var indicators = new[] { "VBOX", "VIRTUAL", "VMWARE", "HYPER", "QEMU", "KVM" };
            var processor = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? string.Empty;
            if (indicators.Any(indicator => processor.Contains(indicator, StringComparison.OrdinalIgnoreCase)))
            {
                return "Возможно (CPU сигнатура)";
            }

            var manufacturer = GetWmiValue("Win32_ComputerSystem", "Manufacturer");
            var model = GetWmiValue("Win32_ComputerSystem", "Model");
            if (indicators.Any(indicator =>
                    (manufacturer?.IndexOf(indicator, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0 ||
                    (model?.IndexOf(indicator, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0))
            {
                return "Возможно (по данным системы)";
            }
        }
        catch
        {
            // ignore missing permissions
        }

        return "Нет";
    }

    private static string BuildHardwareSpec()
    {
        var cpu = GetWmiValue("Win32_Processor", "Name") ??
                  (Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "Неизвестно");
        var gpu = GetWmiValue("Win32_VideoController", "Name") ?? "Недоступно";
        var ram = GetTotalRam();
        var disk = GetSystemDriveSize();

        return $"CPU: {cpu.Trim()} • GPU: {gpu.Trim()} • RAM: {ram} • Disk: {disk}";
    }

    private static string DetectVpnStatus()
    {
        try
        {
            var activeVpn = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up)
                .Any(nic =>
                    nic.Description.Contains("VPN", StringComparison.OrdinalIgnoreCase) ||
                    nic.Name.Contains("VPN", StringComparison.OrdinalIgnoreCase) ||
                    nic.Description.Contains("TAP", StringComparison.OrdinalIgnoreCase) ||
                    nic.Description.Contains("TUN", StringComparison.OrdinalIgnoreCase));

            return activeVpn ? "Возможно подключён" : "Нет";
        }
        catch
        {
            return "Не удалось определить";
        }
    }

    private static int CountLocalUsers()
    {
        try
        {
            var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var usersRoot = Directory.GetParent(profile)?.FullName;
            if (!string.IsNullOrEmpty(usersRoot) && Directory.Exists(usersRoot))
            {
                return Directory.EnumerateDirectories(usersRoot)
                    .Count(dir => !dir.EndsWith("Default", StringComparison.OrdinalIgnoreCase) &&
                                  !dir.EndsWith("Default User", StringComparison.OrdinalIgnoreCase) &&
                                  !dir.EndsWith("Public", StringComparison.OrdinalIgnoreCase));
            }
        }
        catch
        {
            // ignore access issues
        }

        return 1;
    }

    private static string GetTotalRam()
    {
        try
        {
            var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
            foreach (var obj in searcher.Get())
            {
                var bytes = Convert.ToDouble(obj["TotalPhysicalMemory"]);
                return FormatBytes(bytes);
            }
        }
        catch
        {
            // ignored
        }

        return "—";
    }

    private static string GetSystemDriveSize()
    {
        try
        {
            var systemDrive = Path.GetPathRoot(Environment.SystemDirectory) ?? string.Empty;
            if (!string.IsNullOrEmpty(systemDrive))
            {
                var driveInfo = new DriveInfo(systemDrive);
                if (driveInfo.IsReady)
                {
                    return FormatBytes(driveInfo.TotalSize);
                }
            }
        }
        catch
        {
            // ignored
        }

        return "—";
    }

    private static string? GetWmiValue(string scope, string property)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher($"SELECT {property} FROM {scope}");
            foreach (var obj in searcher.Get())
            {
                return obj[property]?.ToString();
            }
        }
        catch
        {
            // ignore if WMI unavailable
        }

        return null;
    }

    private static string FormatUptime(TimeSpan span)
    {
        var parts = new List<string>();
        if (span.TotalDays >= 1)
        {
            parts.Add($"{(int)span.TotalDays}d");
        }
        parts.Add($"{span.Hours}h");
        parts.Add($"{span.Minutes}m");
        return string.Join(" ", parts);
    }

    private static string FormatBytes(double bytes)
    {
        const double scale = 1024;
        var units = new[] { "B", "KB", "MB", "GB", "TB" };
        var order = 0;
        while (bytes >= scale && order < units.Length - 1)
        {
            bytes /= scale;
            order++;
        }

        return $"{bytes:0.##} {units[order]}";
    }
}
