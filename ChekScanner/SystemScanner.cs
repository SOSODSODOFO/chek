using Microsoft.VisualBasic.Devices;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace ChekScanner;

public sealed class SystemScanner
{
    private static readonly string[] SuspiciousProcessKeywords =
    {
        "cheat", "loader", "inject", "hack", "dllhost", "trainer", "engine", "bypass", "spoof", "hook", "xenos", "xeno",
        "crack", "wh", "aim", "radar", "trigger", "overlay", "macro", "mod", "script"
    };

    private static readonly string[] SuspiciousProcessNames =
    {
        "cs2", "steamwebhelper", "steam.exe", "steamservice.exe", "plaguecheat.exe", "boosterx.exe", "xeno.exe",
        "loader.exe", "injector.exe", "cheat_checker.exe", "deltaloader.exe", "mzfu9zwbjnjk0vtknhqv.exe"
    };

    private static readonly string[] SuspiciousFileKeywords =
    {
        "cheat", "hack", "inject", "loader", "bypass", "macro", "aim", "radar", "spoof", "wall", "xeno", "script"
    };

    private static readonly string[] SuspiciousExtensions =
    {
        ".dll", ".zip", ".rar", ".7z", ".exe", ".bat", ".cmd"
    };

    private static readonly string[] RegistryLocationsToCheck =
    {
        @"Software\\Cheat Engine",
        @"Software\\CheatEngine",
        @"Software\\Game Cheats",
        @"Software\\Microsoft\\Windows\\CurrentVersion\\Run",
        @"SYSTEM\\CurrentControlSet\\Services"
    };

    private static readonly string[] SuspiciousRegistryKeywords =
    {
        "cheat", "hack", "loader", "inject", "bypass", "wall", "aim", "radar", "spoof", "macro"
    };

    private static readonly string NvidiaDrsPath = @"C:\\ProgramData\\NVIDIA Corporation\\Drs\\nvAppTimestamps";

    public ScanResult PerformScan()
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Сканирование доступно только на Windows.");
        }

        var result = new ScanResult
        {
            SystemInfo = ComposeSystemInfo(),
            MachineName = Environment.MachineName,
            VmStatus = DetectVirtualMachine() ? "Обнаружена виртуализация" : "Нет",
            HardwareSummary = BuildHardwareSummary(),
            VpnStatus = DetectVpn() ? "VPN активен" : "VPN не обнаружен",
            UserCount = CountLocalUsers(),
            BootTime = FormatUptime()
        };

        var processes = GatherProcesses();
        result.Processes = processes;
        result.SuspiciousProcesses = IdentifySuspiciousProcesses(processes);
        result.SuspiciousFiles = FindSuspiciousFiles();
        result.SuspiciousRegistry = InspectRegistry();
        result.NvidiaDrsEntries = ParseNvidiaDrs();
        result.Judgement = BuildJudgement(result);

        return result;
    }

    private static string ComposeSystemInfo()
    {
        var architecture = RuntimeInformation.OSArchitecture;
        var os = RuntimeInformation.OSDescription;
        return $"{os} ({architecture})";
    }

    private static bool DetectVirtualMachine()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Model FROM Win32_ComputerSystem");
            foreach (var obj in searcher.Get())
            {
                var manufacturer = obj["Manufacturer"]?.ToString()?.ToLowerInvariant() ?? string.Empty;
                var model = obj["Model"]?.ToString()?.ToLowerInvariant() ?? string.Empty;

                if (manufacturer.Contains("microsoft") && model.Contains("virtual"))
                {
                    return true;
                }

                if (manufacturer.Contains("vmware") || manufacturer.Contains("xen") || manufacturer.Contains("qemu") || manufacturer.Contains("innotek"))
                {
                    return true;
                }
            }
        }
        catch (ManagementException)
        {
            // ignore
        }

        try
        {
            using var biosSearcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS");
            foreach (var obj in biosSearcher.Get())
            {
                var serial = obj["SerialNumber"]?.ToString()?.ToLowerInvariant() ?? string.Empty;
                if (serial.Contains("vmware") || serial.Contains("virtualbox") || serial.Contains("xen"))
                {
                    return true;
                }
            }
        }
        catch (ManagementException)
        {
            // ignore
        }

        return false;
    }

    private static string BuildHardwareSummary()
    {
        var cpu = "—";
        var gpu = "—";
        var ram = "—";
        var disk = "—";

        try
        {
            using var cpuSearcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
            cpu = cpuSearcher.Get().Cast<ManagementObject>().Select(mo => mo["Name"]?.ToString()).FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)) ?? cpu;
        }
        catch (ManagementException)
        {
            // ignore
        }

        try
        {
            using var gpuSearcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController WHERE AdapterDACType IS NOT NULL");
            gpu = gpuSearcher.Get().Cast<ManagementObject>().Select(mo => mo["Name"]?.ToString()).FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)) ?? gpu;
        }
        catch (ManagementException)
        {
            // ignore
        }

        try
        {
            var computerInfo = new ComputerInfo();
            var totalRam = computerInfo.TotalPhysicalMemory;
            ram = $"{Math.Round(totalRam / 1024d / 1024d / 1024d, 2)} GB";
        }
        catch
        {
            // ignore
        }

        try
        {
            var systemDrive = DriveInfo.GetDrives().FirstOrDefault(d => d.Name.Equals(Path.GetPathRoot(Environment.SystemDirectory), StringComparison.OrdinalIgnoreCase));
            if (systemDrive != null && systemDrive.IsReady)
            {
                disk = $"{Math.Round(systemDrive.TotalSize / 1024d / 1024d / 1024d, 2)} GB";
            }
        }
        catch
        {
            // ignore
        }

        return $"CPU: {cpu} • GPU: {gpu} • RAM: {ram} • Disk: {disk}";
    }

    private static bool DetectVpn()
    {
        try
        {
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up)
                {
                    continue;
                }

                var description = nic.Description.ToLowerInvariant();
                var name = nic.Name.ToLowerInvariant();

                if (description.Contains("vpn") || description.Contains("virtual") || description.Contains("tunnel") || description.Contains("wireguard") ||
                    name.Contains("vpn") || name.Contains("tap") || name.Contains("tun") || name.Contains("wg"))
                {
                    return true;
                }
            }
        }
        catch
        {
            // ignore
        }

        return false;
    }

    private static int CountLocalUsers()
    {
        try
        {
            var usersDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "..", "..", "Users");
            var normalized = Path.GetFullPath(usersDir);
            if (Directory.Exists(normalized))
            {
                return Directory.GetDirectories(normalized).Length;
            }
        }
        catch
        {
            // ignore
        }

        return 0;
    }

    private static string FormatUptime()
    {
        var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
        if (uptime.TotalDays >= 1)
        {
            return $"{(int)uptime.TotalDays}d {uptime.Hours}h {uptime.Minutes}m";
        }

        return $"{uptime.Hours}h {uptime.Minutes}m";
    }

    private static List<ProcessInfo> GatherProcesses()
    {
        var list = new List<ProcessInfo>();
        foreach (var process in Process.GetProcesses())
        {
            string path = "Неизвестно";
            try
            {
                path = process.MainModule?.FileName ?? path;
            }
            catch
            {
                // ignored
            }

            double memoryMb = 0;
            try
            {
                memoryMb = Math.Round(process.WorkingSet64 / 1024d / 1024d, 2);
            }
            catch
            {
                // ignore
            }

            list.Add(new ProcessInfo
            {
                Name = process.ProcessName,
                Pid = process.Id,
                Path = path,
                MemoryMb = memoryMb
            });
        }

        return list.OrderByDescending(p => p.MemoryMb).ToList();
    }

    private static List<ProcessInfo> IdentifySuspiciousProcesses(IEnumerable<ProcessInfo> processes)
    {
        var suspicious = new List<ProcessInfo>();
        foreach (var proc in processes)
        {
            var name = proc.Name.ToLowerInvariant();
            var path = proc.Path.ToLowerInvariant();

            if (SuspiciousProcessNames.Any(sp => name.Contains(sp.Replace(".exe", string.Empty))))
            {
                suspicious.Add(proc);
                continue;
            }

            if (SuspiciousProcessKeywords.Any(keyword => name.Contains(keyword) || path.Contains(keyword)))
            {
                suspicious.Add(proc);
                continue;
            }

            if (path.Contains("downloads") || path.Contains("temp") || path.Contains("appdata"))
            {
                if (SuspiciousExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                {
                    suspicious.Add(proc);
                }
            }
        }

        return suspicious.Distinct().ToList();
    }

    private static List<FileFinding> FindSuspiciousFiles()
    {
        var findings = new List<FileFinding>();
        var rootFolders = new List<string>
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))
        };

        const int maxFindings = 200;

        foreach (var folder in rootFolders.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            {
                continue;
            }

            try
            {
                foreach (var file in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
                {
                    var lower = file.ToLowerInvariant();
                    if (!SuspiciousExtensions.Any(ext => lower.EndsWith(ext)))
                    {
                        continue;
                    }

                    if (SuspiciousFileKeywords.Any(keyword => lower.Contains(keyword)))
                    {
                        try
                        {
                            var info = new FileInfo(file);
                            findings.Add(new FileFinding
                            {
                                Name = file,
                                Size = FormatFileSize(info.Length)
                            });

                            if (findings.Count >= maxFindings)
                            {
                                return findings;
                            }
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                }

                if (findings.Count >= maxFindings)
                {
                    break;
                }
            }
            catch
            {
                // ignore directories that cannot be accessed
            }
        }

        return findings;
    }

    private static string FormatFileSize(long bytes)
    {
        double size = bytes;
        string[] suffixes = { "B", "KB", "MB", "GB" };
        int order = 0;
        while (size >= 1024 && order < suffixes.Length - 1)
        {
            order += 1;
            size /= 1024;
        }

        return $"{size:0.##} {suffixes[order]}";
    }

    private static List<string> InspectRegistry()
    {
        var findings = new List<string>();

        foreach (var location in RegistryLocationsToCheck)
        {
            foreach (var hive in new[] { RegistryHive.CurrentUser, RegistryHive.LocalMachine })
            {
                try
                {
                    using var key = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64).OpenSubKey(location);
                    if (key != null)
                    {
                        var values = key.GetValueNames();
                        foreach (var valueName in values)
                        {
                            var value = key.GetValue(valueName)?.ToString() ?? string.Empty;
                            if (SuspiciousRegistryKeywords.Any(keyword => valueName.ToLowerInvariant().Contains(keyword) || value.ToLowerInvariant().Contains(keyword)))
                            {
                                findings.Add($"{hive}\\{location} → {valueName}: {value}");
                            }
                        }
                    }
                }
                catch
                {
                    // ignore access exceptions
                }
            }
        }

        return findings;
    }

    private static List<KeyValuePair<string, string>> ParseNvidiaDrs()
    {
        var list = new List<KeyValuePair<string, string>>();
        try
        {
            if (!File.Exists(NvidiaDrsPath))
            {
                return list;
            }

            var bytes = File.ReadAllBytes(NvidiaDrsPath);
            var builder = new StringBuilder();
            foreach (var b in bytes)
            {
                if (b >= 32 && b <= 126)
                {
                    builder.Append((char)b);
                }
                else
                {
                    builder.Append('\n');
                }
            }

            var tokens = builder.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            int counter = 1;
            foreach (var token in tokens)
            {
                if (token.Length < 4)
                {
                    continue;
                }

                var cleaned = token.Trim();
                if (!Regex.IsMatch(cleaned, "[A-Za-z]"))
                {
                    continue;
                }

                string profile = $"Запись {counter}";
                string value = cleaned;

                var match = Regex.Match(cleaned, "^(?<prefix>[^A-Za-z0-9]{0,2})(?<content>.+)$");
                if (match.Success)
                {
                    var prefix = match.Groups["prefix"].Value.Trim();
                    if (!string.IsNullOrWhiteSpace(prefix))
                    {
                        profile = prefix;
                    }

                    value = match.Groups["content"].Value.Trim();
                }

                list.Add(new KeyValuePair<string, string>(profile, value));
                counter += 1;

                if (counter > 200)
                {
                    break;
                }
            }
        }
        catch
        {
            // ignore parsing issues
        }

        return list;
    }

    private static string BuildJudgement(ScanResult result)
    {
        var issues = new List<string>();
        if (result.SuspiciousProcesses.Any())
        {
            issues.Add("подозрительные процессы");
        }

        if (result.SuspiciousFiles.Any())
        {
            issues.Add("сомнительные файлы");
        }

        if (result.SuspiciousRegistry.Any())
        {
            issues.Add("ключи реестра");
        }

        return issues.Count == 0
            ? "Clean ✅"
            : $"Обнаружены {string.Join(", ", issues)}";
    }
}

public sealed class ScanResult
{
    public TimeSpan ScanDuration { get; set; }
    public string SystemInfo { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string VmStatus { get; set; } = string.Empty;
    public string HardwareSummary { get; set; } = string.Empty;
    public string VpnStatus { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public string BootTime { get; set; } = string.Empty;
    public List<ProcessInfo> Processes { get; set; } = new();
    public List<ProcessInfo> SuspiciousProcesses { get; set; } = new();
    public List<FileFinding> SuspiciousFiles { get; set; } = new();
    public List<string> SuspiciousRegistry { get; set; } = new();
    public List<KeyValuePair<string, string>> NvidiaDrsEntries { get; set; } = new();
    public string Judgement { get; set; } = string.Empty;
}

public sealed class ProcessInfo : IEquatable<ProcessInfo>
{
    public string Name { get; set; } = string.Empty;
    public int Pid { get; set; }
    public string Path { get; set; } = string.Empty;
    public double MemoryMb { get; set; }

    public bool Equals(ProcessInfo? other)
    {
        if (other is null)
        {
            return false;
        }

        return Pid == other.Pid;
    }

    public override bool Equals(object? obj) => Equals(obj as ProcessInfo);

    public override int GetHashCode() => Pid.GetHashCode();
}

public sealed class FileFinding
{
    public string Name { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
}
