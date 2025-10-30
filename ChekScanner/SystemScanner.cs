using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace ChekScanner;

public sealed class SystemScanner
{
    private static readonly string[] SuspiciousProcessKeywords =
    {
        "cheat", "inject", "trainer", "hack", "engine", "aim", "esp"
    };

    private static readonly string[] SuspiciousFileNames =
    {
        "injector.exe", "cheat.dll", "hook.dll", "loader.exe", "trainer.exe"
    };

    private static readonly string[] SuspiciousRegistryPaths =
    {
        @"Software\\CheatEngine",
        @"Software\\Valve\\Steam\\Apps\\730\\CustomSettings",
        @"SYSTEM\\CurrentControlSet\\Services\\EasyAntiCheat"
    };

    public async Task<SystemScanResult> ScanAsync()
    {
        var result = new SystemScanResult();
        var watch = Stopwatch.StartNew();

        await Task.Run(() =>
        {
            result.MachineName = Environment.MachineName;
            result.SystemInfo = $"{RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})";
            result.Uptime = FormatDuration(TimeSpan.FromMilliseconds(Environment.TickCount64));
            result.VirtualizationStatus = DetectVirtualization();
            result.HardwareSummary = BuildHardwareSummary();
            result.VpnStatus = DetectVpnStatus();
            result.LocalUserCount = CountLocalUsers();
            result.SuspiciousRegistryKeys.AddRange(CheckRegistry());
            result.Processes.AddRange(GetProcessSnapshot(out var suspiciousProcesses));
            result.SuspiciousProcesses.AddRange(suspiciousProcesses);
            result.SuspiciousFiles.AddRange(FindSuspiciousFiles());
            result.NvidiaDrsEntries.AddRange(ReadNvidiaDrs());
        });

        watch.Stop();
        result.ScanDuration = FormatDuration(watch.Elapsed);
        result.Judgement = result.SuspiciousProcesses.Any() || result.SuspiciousFiles.Any() || result.SuspiciousRegistryKeys.Any()
            ? "Подозрения ⚠️"
            : "Clean ✅";

        return result;
    }

    private static string FormatDuration(TimeSpan span)
    {
        if (span.TotalHours >= 1)
        {
            return $"{(int)span.TotalHours}h {span.Minutes}m";
        }

        if (span.TotalMinutes >= 1)
        {
            return $"{span.Minutes}m {span.Seconds}s";
        }

        return $"{span.Seconds}s";
    }

    private static string DetectVirtualization()
    {
        if (!OperatingSystem.IsWindows())
        {
            return "Недоступно";
        }

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Model FROM Win32_ComputerSystem");
            foreach (var obj in searcher.Get().Cast<ManagementObject>())
            {
                var manufacturer = (obj["Manufacturer"]?.ToString() ?? string.Empty).ToLowerInvariant();
                var model = (obj["Model"]?.ToString() ?? string.Empty).ToLowerInvariant();
                if (manufacturer.Contains("vmware") || model.Contains("vmware"))
                {
                    return "VMware";
                }

                if (manufacturer.Contains("microsoft") && model.Contains("virtual"))
                {
                    return "Hyper-V";
                }

                if (manufacturer.Contains("xen") || model.Contains("xen"))
                {
                    return "Xen";
                }

                if (model.Contains("virtualbox"))
                {
                    return "VirtualBox";
                }
            }
        }
        catch
        {
            // ignored
        }

        return "Нет";
    }

    private static string BuildHardwareSummary()
    {
        var cpu = "CPU: неизвестно";
        var gpu = "GPU: неизвестно";
        var ram = "RAM: неизвестно";
        var disk = "Disk: неизвестно";

        if (!OperatingSystem.IsWindows())
        {
            return string.Join(" • ", new[] { cpu, gpu, ram, disk });
        }

        try
        {
            using var cpuSearcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
            cpu = cpuSearcher.Get().Cast<ManagementObject>().Select(m => m["Name"]?.ToString() ?? "").FirstOrDefault()
                  ?? cpu;
            cpu = $"CPU: {cpu.Trim()}";
        }
        catch
        {
            // ignore
        }

        try
        {
            using var gpuSearcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController");
            gpu = gpuSearcher.Get().Cast<ManagementObject>().Select(m => m["Name"]?.ToString() ?? "").FirstOrDefault()
                  ?? gpu;
            gpu = $"GPU: {gpu.Trim()}";
        }
        catch
        {
            // ignore
        }

        try
        {
            using var ramSearcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
            var bytes = ramSearcher.Get().Cast<ManagementObject>().Select(m => Convert.ToDouble(m["TotalPhysicalMemory"])).FirstOrDefault();
            if (bytes > 0)
            {
                ram = $"RAM: {bytes / (1024 * 1024 * 1024):F1} GB";
            }
        }
        catch
        {
            // ignore
        }

        try
        {
            var drives = DriveInfo.GetDrives()
                .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                .Select(d => $"{d.Name.TrimEnd('\\')} {d.TotalSize / (1024 * 1024 * 1024):F0} GB")
                .ToArray();
            if (drives.Length > 0)
            {
                disk = "Disk: " + string.Join(", ", drives);
            }
        }
        catch
        {
            // ignore
        }

        return string.Join(" • ", new[] { cpu, gpu, ram, disk }.Where(s => !string.IsNullOrWhiteSpace(s)));
    }

    private static string DetectVpnStatus()
    {
        try
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var adapter in interfaces)
            {
                if (adapter.OperationalStatus != OperationalStatus.Up)
                {
                    continue;
                }

                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                {
                    return "Да";
                }

                var description = adapter.Description.ToLowerInvariant();
                if (description.Contains("vpn") || description.Contains("tunnel"))
                {
                    return "Да";
                }
            }
        }
        catch
        {
            // ignore
        }

        return "Нет";
    }

    private static int CountLocalUsers()
    {
        if (!OperatingSystem.IsWindows())
        {
            return 0;
        }

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_UserAccount WHERE LocalAccount = TRUE");
            return searcher.Get().Cast<ManagementObject>().Count();
        }
        catch
        {
            return 0;
        }
    }

    private static IEnumerable<string> CheckRegistry()
    {
        var results = new List<string>();
        if (!OperatingSystem.IsWindows())
        {
            return results;
        }

        foreach (var path in SuspiciousRegistryPaths)
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(path) ?? Registry.CurrentUser.OpenSubKey(path);
                if (key != null)
                {
                    results.Add(path);
                }
            }
            catch
            {
                // ignore
            }
        }

        return results;
    }

    private static IEnumerable<SuspiciousFile> FindSuspiciousFiles()
    {
        var results = new List<SuspiciousFile>();
        if (!OperatingSystem.IsWindows())
        {
            return results;
        }

        var basePaths = new List<string>
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")
        };

        foreach (var directory in basePaths.Where(p => !string.IsNullOrWhiteSpace(p) && Directory.Exists(p)))
        {
            try
            {
                foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
                {
                    if (SuspiciousFileNames.Any(name => file.EndsWith(name, StringComparison.OrdinalIgnoreCase)))
                    {
                        var info = new FileInfo(file);
                        results.Add(new SuspiciousFile(file, FormatFileSize(info.Length)));
                    }
                }
            }
            catch
            {
                // ignore inaccessible directories
            }
        }

        return results;
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes > 1024 * 1024)
        {
            return $"{bytes / (1024d * 1024d):F1} MB";
        }

        if (bytes > 1024)
        {
            return $"{bytes / 1024d:F1} KB";
        }

        return $"{bytes} B";
    }

    private static List<ProcessInfo> GetProcessSnapshot(out List<string> suspicious)
    {
        var processes = new List<ProcessInfo>();
        var suspiciousList = new List<string>();

        foreach (var process in Process.GetProcesses())
        {
            try
            {
                var name = process.ProcessName;
                var pid = process.Id;
                var path = string.Empty;
                try
                {
                    path = process.MainModule?.FileName ?? "—";
                }
                catch
                {
                    path = "—";
                }

                var memory = process.WorkingSet64 / (1024d * 1024d);
                processes.Add(new ProcessInfo(name, pid, path, memory));

                var lower = name.ToLowerInvariant();
                if (SuspiciousProcessKeywords.Any(keyword => lower.Contains(keyword)))
                {
                    suspiciousList.Add(name);
                }
            }
            catch
            {
                // ignore processes we cannot access
            }
            finally
            {
                process.Dispose();
            }
        }

        suspicious = suspiciousList.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(s => s).ToList();
        return processes.OrderByDescending(p => p.MemoryMegabytes).ToList();
    }

    private static IEnumerable<NvidiaDrsEntry> ReadNvidiaDrs()
    {
        // Placeholder stub for future integration with NVIDIA DRS configuration.
        return Array.Empty<NvidiaDrsEntry>();
    }
}
