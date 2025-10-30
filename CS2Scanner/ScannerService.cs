using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using CS2Scanner.Models;
using Microsoft.Win32;

namespace CS2Scanner;

internal sealed class ScannerService
{
    private static readonly string[] SuspiciousProcessKeywords =
    {
        "cheat", "inject", "trainer", "hack", "aim", "bypass", "spoof"
    };

    private static readonly string[] SuspiciousRegistryLocations =
    {
        @"Software\\CheatEngine",
        @"Software\\WPE Pro",
        @"Software\\Microsoft\\Windows\\CurrentVersion\\Run",
        @"System\\CurrentControlSet\\Services\\EasyAntiCheat"
    };

    private static readonly string[] ScanDirectories =
    {
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp"),
        Path.GetTempPath()
    };

    private static readonly string[] FilePatterns = { "*.dll", "*.exe", "*.zip" };

    public async Task<SystemScanResult> ScanAsync(IProgress<string>? progress = null)
    {
        return await Task.Run(() => PerformScan(progress));
    }

    private SystemScanResult PerformScan(IProgress<string>? progress)
    {
        var stopwatch = Stopwatch.StartNew();
        progress?.Report("Сбор информации о системе...");

        var systemInfo = GetSystemInfo();
        var machineName = Environment.MachineName;
        var vmStatus = DetectVirtualMachine();
        var hardwareSpec = BuildHardwareSpec();
        var vpnStatus = DetectVpn();
        var userCount = CountLocalUsers();
        var bootTime = GetLastBootTime();

        progress?.Report("Анализ активных процессов...");
        var processes = CollectProcesses(out var cheatProcesses);

        progress?.Report("Поиск подозрительных файлов...");
        var files = ScanSuspiciousFiles();

        progress?.Report("Проверка реестра...");
        var registry = ScanRegistry();

        progress?.Report("Анализ NVIDIA DRS...");
        var drsEntries = AnalyzeNvidiaDrs();

        stopwatch.Stop();
        var scanDuration = FormatDuration(stopwatch.Elapsed);

        return new SystemScanResult
        {
            SystemInfo = systemInfo,
            ScanDuration = scanDuration,
            MachineName = machineName,
            VmStatus = vmStatus,
            HardwareSpec = hardwareSpec,
            VpnStatus = vpnStatus,
            UserCount = userCount,
            BootTime = bootTime,
            CheatProcesses = cheatProcesses,
            SuspiciousFiles = files,
            SuspiciousRegistryKeys = registry,
            Processes = processes,
            DrsEntries = drsEntries
        };
    }

    private static string GetSystemInfo()
    {
        var version = Environment.OSVersion;
        var architecture = Environment.Is64BitOperatingSystem ? "x64" : "x86";
        return $"{version.VersionString} ({architecture})";
    }

    private static string DetectVirtualMachine()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Model FROM Win32_ComputerSystem");
            foreach (var obj in searcher.Get().OfType<ManagementObject>())
            {
                var manufacturer = obj["Manufacturer"]?.ToString() ?? string.Empty;
                var model = obj["Model"]?.ToString() ?? string.Empty;
                var combined = (manufacturer + " " + model).ToLowerInvariant();
                if (combined.Contains("vmware") || combined.Contains("virtual") || combined.Contains("hyper-v") || combined.Contains("qemu"))
                {
                    return "Да";
                }
            }
        }
        catch
        {
            // ignored
        }

        return "Нет";
    }

    private static string BuildHardwareSpec()
    {
        var cpu = "Не удалось определить";
        var gpu = "Не удалось определить";
        var ram = "?";
        var disk = "?";

        try
        {
            using var cpuSearcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
            cpu = string.Join(", ", cpuSearcher.Get().OfType<ManagementObject>().Select(mo => mo["Name"]?.ToString()?.Trim() ?? string.Empty).Where(s => !string.IsNullOrWhiteSpace(s)));
        }
        catch
        {
            cpu = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? cpu;
        }

        try
        {
            using var gpuSearcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController");
            gpu = string.Join(", ", gpuSearcher.Get().OfType<ManagementObject>().Select(mo => mo["Name"]?.ToString()?.Trim() ?? string.Empty).Where(s => !string.IsNullOrWhiteSpace(s)));
        }
        catch
        {
            // ignored
        }

        try
        {
            using var ramSearcher = new ManagementObjectSearcher("SELECT Capacity FROM Win32_PhysicalMemory");
            ulong totalBytes = 0;
            foreach (var module in ramSearcher.Get().OfType<ManagementObject>())
            {
                totalBytes += Convert.ToUInt64(module["Capacity"] ?? 0);
            }

            ram = FormatBytes(totalBytes);
        }
        catch
        {
            try
            {
                ram = FormatBytes(new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory);
            }
            catch
            {
                // ignored
            }
        }

        try
        {
            var drives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed && d.IsReady).ToList();
            var builder = new StringBuilder();
            foreach (var drive in drives)
            {
                builder.Append($"{drive.Name} {FormatBytes((ulong)drive.TotalSize)} ({FormatBytes((ulong)drive.AvailableFreeSpace)} свободно);");
            }

            if (builder.Length > 0)
            {
                disk = builder.ToString();
            }
        }
        catch
        {
            // ignored
        }

        return $"CPU: {cpu} • GPU: {gpu} • RAM: {ram} • Disk: {disk}";
    }

    private static string DetectVpn()
    {
        try
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var @interface in interfaces)
            {
                var description = @interface.Description.ToLowerInvariant();
                if (description.Contains("vpn") || description.Contains("tap") || description.Contains("tun"))
                {
                    return "Возможно";
                }
            }
        }
        catch
        {
            // ignored
        }

        return "Нет";
    }

    private static int CountLocalUsers()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_UserAccount WHERE LocalAccount = TRUE");
            return searcher.Get().Count;
        }
        catch
        {
            return 0;
        }
    }

    private static string GetLastBootTime()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT LastBootUpTime FROM Win32_OperatingSystem");
            foreach (var obj in searcher.Get().OfType<ManagementObject>())
            {
                var value = obj["LastBootUpTime"]?.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    var date = ManagementDateTimeConverter.ToDateTime(value);
                    var uptime = DateTime.Now - date;
                    return FormatDuration(uptime);
                }
            }
        }
        catch
        {
            // ignored
        }

        return "Не удалось определить";
    }

    private static IReadOnlyList<ProcessDetail> CollectProcesses(out IReadOnlyList<string> cheatProcesses)
    {
        var results = new List<ProcessDetail>();
        var suspicious = new List<string>();

        foreach (var process in Process.GetProcesses())
        {
            try
            {
                var name = process.ProcessName;
                var path = "Недоступно";

                try
                {
                    path = process.MainModule?.FileName ?? path;
                }
                catch
                {
                    // access denied
                }

                double memoryMb = Math.Round(process.WorkingSet64 / 1024d / 1024d, 2);
                results.Add(new ProcessDetail(name, process.Id, path, memoryMb));

                var lower = name.ToLowerInvariant();
                if (SuspiciousProcessKeywords.Any(keyword => lower.Contains(keyword)))
                {
                    suspicious.Add(name);
                }
            }
            catch
            {
                // ignore inaccessible processes
            }
        }

        results.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
        suspicious = suspicious.Distinct().ToList();
        suspicious.Sort(StringComparer.OrdinalIgnoreCase);

        cheatProcesses = suspicious;
        return results;
    }

    private static IReadOnlyList<FileFinding> ScanSuspiciousFiles()
    {
        var findings = new List<FileFinding>();
        foreach (var directory in ScanDirectories.Distinct().Where(Directory.Exists))
        {
            foreach (var pattern in FilePatterns)
            {
                try
                {
                    foreach (var file in Directory.EnumerateFiles(directory, pattern, SearchOption.AllDirectories))
                    {
                        var info = new FileInfo(file);
                        var size = FormatBytes((ulong)info.Length);
                        findings.Add(new FileFinding(file, size));
                    }
                }
                catch
                {
                    // skip directories without permission
                }
            }
        }

        return findings;
    }

    private static IReadOnlyList<string> ScanRegistry()
    {
        var findings = new List<string>();
        foreach (var path in SuspiciousRegistryLocations)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(path) ?? Registry.LocalMachine.OpenSubKey(path);
                if (key != null)
                {
                    findings.Add(path);
                }
            }
            catch
            {
                // ignored
            }
        }

        return findings;
    }

    private static IReadOnlyList<DrsEntry> AnalyzeNvidiaDrs()
    {
        var results = new List<DrsEntry>();
        try
        {
            var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "NVIDIA Corporation", "Drs");
            if (Directory.Exists(basePath))
            {
                foreach (var file in Directory.EnumerateFiles(basePath, "*.profile", SearchOption.TopDirectoryOnly))
                {
                    var info = new FileInfo(file);
                    results.Add(new DrsEntry(info.Name, $"Изменён {info.LastWriteTime:G}"));
                }
            }
        }
        catch
        {
            // ignored
        }

        return results;
    }

    private static string FormatDuration(TimeSpan time)
    {
        if (time.TotalHours >= 1)
        {
            return $"{(int)time.TotalHours}h {time.Minutes}m";
        }

        if (time.TotalMinutes >= 1)
        {
            return $"{time.Minutes}m {time.Seconds}s";
        }

        return $"{time.Seconds}s";
    }

    private static string FormatBytes(ulong bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return string.Format(CultureInfo.InvariantCulture, "{0:0.##} {1}", len, sizes[order]);
    }
}
