using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;

namespace ChekApp
{
    public sealed class ScanReport
    {
        public string ScanDuration { get; init; } = string.Empty;
        public string SystemInfo { get; init; } = string.Empty;
        public string MachineName { get; init; } = string.Empty;
        public string VirtualMachineStatus { get; init; } = string.Empty;
        public string HardwareSpec { get; init; } = string.Empty;
        public string VpnStatus { get; init; } = string.Empty;
        public int UserCount { get; init; }
        public string Uptime { get; init; } = string.Empty;
        public string Judgement { get; init; } = string.Empty;
        public IReadOnlyList<string> CheatProcesses { get; init; } = Array.Empty<string>();
        public IReadOnlyList<(string Path, string Size)> SuspiciousFiles { get; init; } = Array.Empty<(string, string)>();
        public IReadOnlyList<string> SuspiciousRegistry { get; init; } = Array.Empty<string>();
        public IReadOnlyList<ProcessDetails> Processes { get; init; } = Array.Empty<ProcessDetails>();
    }

    public sealed class ProcessDetails
    {
        public string Name { get; init; } = string.Empty;
        public int Id { get; init; }
        public string FilePath { get; init; } = string.Empty;
        public double MemoryMb { get; init; }
    }

    public static class SystemScanner
    {
        private static readonly string[] SuspiciousProcessKeywords =
        {
            "cheat", "inject", "trainer", "engine", "h4x", "hack"
        };

        private static readonly string[] SuspiciousFilePaths =
        {
            "C:/Users/Public/injector.exe",
            "C:/Users/Public/hook.dll",
            "C:/Program Files/cheatengine64.exe"
        };

        private static readonly string[] SuspiciousRegistryKeys =
        {
            "Software\\CheatEngine",
            "SYSTEM\\CurrentControlSet\\Services\\kprocesshacker"
        };

        public static ScanReport GenerateReport()
        {
            var stopwatch = Stopwatch.StartNew();

            var processes = GetProcessDetails();
            var suspected = processes
                .Where(p => SuspiciousProcessKeywords.Any(keyword => p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                .Select(p => p.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .DefaultIfEmpty("Нет подозрительных процессов")
                .ToArray();

            var suspiciousFiles = SuspiciousFilePaths
                .Where(File.Exists)
                .Select(path =>
                {
                    try
                    {
                        var info = new FileInfo(path);
                        return (path, SizeFormatter(info.Length));
                    }
                    catch
                    {
                        return (path, "—");
                    }
                })
                .DefaultIfEmpty(("Нет обнаруженных файлов", "—"))
                .ToArray();

            var registryKeys = GetSuspiciousRegistryKeys();

            var report = new ScanReport
            {
                ScanDuration = $"{stopwatch.Elapsed:mm\:ss}",
                SystemInfo = GetSystemInfo(),
                MachineName = Environment.MachineName,
                VirtualMachineStatus = DetectVirtualMachine() ? "Да" : "Нет",
                HardwareSpec = GetHardwareSpec(),
                VpnStatus = DetectVpnStatus(),
                UserCount = GetLocalUserCount(),
                Uptime = GetUptime(),
                Judgement = suspected.Any(s => !s.StartsWith("Нет", StringComparison.OrdinalIgnoreCase)) ? "Cheating ❌" : "Clean ✅",
                CheatProcesses = suspected,
                SuspiciousFiles = suspiciousFiles,
                SuspiciousRegistry = registryKeys,
                Processes = processes
            };

            stopwatch.Stop();
            report = report with { ScanDuration = $"{stopwatch.Elapsed:mm\:ss}" };

            return report;
        }

        private static IReadOnlyList<ProcessDetails> GetProcessDetails()
        {
            var result = new List<ProcessDetails>();
            foreach (var process in Process.GetProcesses().OrderByDescending(p => p.WorkingSet64).Take(120))
            {
                string path;
                try
                {
                    path = string.IsNullOrEmpty(process.MainModule?.FileName)
                        ? "—"
                        : process.MainModule!.FileName;
                }
                catch
                {
                    path = "Недоступно";
                }

                result.Add(new ProcessDetails
                {
                    Name = process.ProcessName,
                    Id = process.Id,
                    FilePath = path,
                    MemoryMb = Math.Round(process.WorkingSet64 / 1024d / 1024d, 2)
                });
            }

            return result;
        }

        private static string GetSystemInfo()
        {
            var os = RuntimeInformation.OSDescription;
            var architecture = RuntimeInformation.OSArchitecture;
            return $"{os} ({architecture})";
        }

        private static string GetHardwareSpec()
        {
            try
            {
                using var cpuSearcher = new ManagementObjectSearcher("select Name from Win32_Processor");
                var cpuName = cpuSearcher.Get().Cast<ManagementObject>().FirstOrDefault()?.GetPropertyValue("Name")?.ToString() ?? "Неизвестный CPU";

                using var gpuSearcher = new ManagementObjectSearcher("select Name from Win32_VideoController");
                var gpuName = gpuSearcher.Get().Cast<ManagementObject>().FirstOrDefault()?.GetPropertyValue("Name")?.ToString() ?? "Неизвестный GPU";

                using var ramSearcher = new ManagementObjectSearcher("select Capacity from Win32_PhysicalMemory");
                var ramBytes = ramSearcher.Get().Cast<ManagementObject>().Sum(m => Convert.ToInt64(m.GetPropertyValue("Capacity")));
                var ramGb = Math.Round(ramBytes / 1024d / 1024d / 1024d, 1);

                using var diskSearcher = new ManagementObjectSearcher("select Size from Win32_DiskDrive");
                var diskBytes = diskSearcher.Get().Cast<ManagementObject>().Sum(m => Convert.ToInt64(m.GetPropertyValue("Size")));
                var diskGb = Math.Round(diskBytes / 1024d / 1024d / 1024d, 1);

                return $"CPU: {cpuName} • GPU: {gpuName} • RAM: {ramGb} GB • Disk: {diskGb} GB";
            }
            catch
            {
                return "Аппаратные данные недоступны";
            }
        }

        private static string DetectVpnStatus()
        {
            try
            {
                var networkInterfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
                var vpnActive = networkInterfaces.Any(nic => nic.Description.Contains("VPN", StringComparison.OrdinalIgnoreCase) && nic.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up);
                return vpnActive ? "Да" : "Нет";
            }
            catch
            {
                return "Не удалось определить";
            }
        }

        private static bool DetectVirtualMachine()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("Select * from Win32_ComputerSystem");
                var info = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
                if (info == null)
                {
                    return false;
                }

                var manufacturer = info["Manufacturer"]?.ToString() ?? string.Empty;
                var model = info["Model"]?.ToString() ?? string.Empty;
                return manufacturer.Contains("Microsoft Corporation", StringComparison.OrdinalIgnoreCase) && model.Contains("Virtual", StringComparison.OrdinalIgnoreCase)
                       || manufacturer.Contains("VMware", StringComparison.OrdinalIgnoreCase)
                       || model.Contains("VirtualBox", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static string GetUptime()
        {
            var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
            return $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m";
        }

        private static int GetLocalUserCount()
        {
            try
            {
                var windowsPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                var root = Path.GetPathRoot(string.IsNullOrEmpty(windowsPath) ? Environment.CurrentDirectory : windowsPath);
                if (string.IsNullOrEmpty(root))
                {
                    root = Path.GetPathRoot(Environment.CurrentDirectory) ?? "C:\\";
                }

                var usersPath = Path.Combine(root!, "Users");
                if (Directory.Exists(usersPath))
                {
                    return Directory.GetDirectories(usersPath).Length;
                }
            }
            catch
            {
                // ignored
            }

            return 0;
        }

        private static IReadOnlyList<string> GetSuspiciousRegistryKeys()
        {
            var availableKeys = new List<string>();
            foreach (var key in SuspiciousRegistryKeys)
            {
                if (RegistryHelper.KeyExists(key))
                {
                    availableKeys.Add(key);
                }
            }

            if (availableKeys.Count == 0)
            {
                availableKeys.Add("Не обнаружено");
            }

            return availableKeys;
        }

        private static string SizeFormatter(long bytes)
        {
            if (bytes < 1024)
            {
                return $"{bytes} B";
            }

            double value = bytes;
            var suffixes = new[] { "KB", "MB", "GB", "TB" };
            var suffixIndex = 0;

            while (value >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                value /= 1024;
                suffixIndex++;
            }

            return $"{value:F1} {suffixes[suffixIndex]}";
        }
    }
}
