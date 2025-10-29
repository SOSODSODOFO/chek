using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using CS2Scanner.Models;

namespace CS2Scanner.Scanning;

internal static class SystemInfoCollector
{
    private static readonly string[] SuspiciousProcessKeywords =
    {
        "cheat", "inject", "loader", "hack", "bypass", "crack", "xeno", "x64dbg", "trainer", "engine",
        "dllhost", "booster", "aimbot", "modmenu", "hook", "injector", "script", "vencord", "bloxstrap"
    };

    private static readonly string[] SuspiciousFileExtensions =
    {
        ".dll", ".asi", ".zip", ".rar", ".7z", ".sys"
    };

    private static readonly string[] SuspiciousFileKeywords =
    {
        "cheat", "hack", "inject", "loader", "trainer", "mod", "bypass", "xeno", "booster", "exploit",
        "crack", "rage", "blox", "cs2", "rage", "raze", "aim", "trigger", "macro", "script"
    };

    private static readonly string[] VpnProcessHints =
    {
        "vpn", "openvpn", "wireguard", "windscribe", "protonvpn", "nordvpn", "expressvpn", "tailscale",
        "zerotier", "hamachi"
    };

    public static async Task<ScanReport> RunFullScanAsync(IProgress<string>? progress, CancellationToken cancellationToken)
    {
        progress?.Report("Сбор информации о системе...");
        var stopwatch = Stopwatch.StartNew();

        var systemInfo = GetSystemInformation();
        var machineName = Environment.MachineName;
        var vmStatus = DetectVirtualMachine();
        var hardwareSpec = GetHardwareSpecification();
        var vpnStatus = DetectVpnPresence();
        var userCount = GetUserCount();
        var bootTime = GetUptime();

        progress?.Report("Анализ процессов...");
        var processes = GetProcessDetails();
        var suspiciousProcesses = IdentifySuspiciousProcesses(processes);

        progress?.Report("Поиск подозрительных файлов...");
        var suspiciousFiles = await Task.Run(() => LocateSuspiciousFiles(), cancellationToken).ConfigureAwait(false);

        progress?.Report("Проверка реестра...");
        var registry = CheckRegistry();

        progress?.Report("Чтение NVIDIA DRS...");
        var drsEntries = ReadNvidiaDrs();

        stopwatch.Stop();
        var scanTime = stopwatch.Elapsed;

        var judgement = (suspiciousProcesses.Count > 0 || suspiciousFiles.Count > 0 || registry.Count > 0)
            ? "Cheating suspected ❌"
            : "Clean ✅";

        return new ScanReport
        {
            ScanTime = string.Format("{0:m\'m\' ss\'s\'}", scanTime),
            SystemInfo = systemInfo,
            MachineName = machineName,
            VmStatus = vmStatus,
            HardwareSpec = hardwareSpec,
            VpnStatus = vpnStatus,
            UserCount = userCount,
            BootTime = bootTime,
            Judgement = judgement,
            SuspiciousProcesses = suspiciousProcesses,
            SuspiciousFiles = suspiciousFiles,
            SuspiciousRegistry = registry,
            Processes = processes,
            NvidiaDrs = drsEntries
        };
    }

    private static string GetSystemInformation()
    {
        var version = Environment.OSVersion.VersionString;
        var architecture = Environment.Is64BitOperatingSystem ? "x64" : "x86";
        return $"{version} ({architecture})";
    }

    private static string DetectVirtualMachine()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Model FROM Win32_ComputerSystem");
            foreach (var managementBaseObject in searcher.Get())
            {
                var manufacturer = managementBaseObject["Manufacturer"]?.ToString() ?? string.Empty;
                var model = managementBaseObject["Model"]?.ToString() ?? string.Empty;
                if (manufacturer.Contains("Virtual", StringComparison.OrdinalIgnoreCase) ||
                    manufacturer.Contains("VMware", StringComparison.OrdinalIgnoreCase) ||
                    model.Contains("Virtual", StringComparison.OrdinalIgnoreCase) ||
                    model.Contains("VirtualBox", StringComparison.OrdinalIgnoreCase))
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

    private static string GetHardwareSpecification()
    {
        var cpu = "Unknown CPU";
        var gpu = "Unknown GPU";
        var ram = "Unknown RAM";
        var disk = "Unknown Disk";

        try
        {
            using var searcher = new ManagementObjectSearcher("select Name from Win32_Processor");
            cpu = string.Join(", ", searcher.Get().Cast<ManagementBaseObject>().Select(o => o["Name"]?.ToString() ?? ""));
        }
        catch
        {
        }

        try
        {
            using var searcher = new ManagementObjectSearcher("select Name from Win32_VideoController");
            gpu = string.Join(", ", searcher.Get().Cast<ManagementBaseObject>().Select(o => o["Name"]?.ToString() ?? ""));
        }
        catch
        {
        }

        try
        {
            using var searcher = new ManagementObjectSearcher("select TotalPhysicalMemory from Win32_ComputerSystem");
            foreach (var obj in searcher.Get())
            {
                if (obj["TotalPhysicalMemory"] is ulong bytes)
                {
                    ram = FormatSize(bytes);
                }
            }
        }
        catch
        {
        }

        try
        {
            using var searcher = new ManagementObjectSearcher("select Size from Win32_DiskDrive");
            var diskSizes = new List<string>();
            foreach (var obj in searcher.Get())
            {
                if (obj["Size"] is ulong bytes)
                {
                    diskSizes.Add(FormatSize(bytes));
                }
            }
            if (diskSizes.Count > 0)
            {
                disk = string.Join(", ", diskSizes);
            }
        }
        catch
        {
        }

        return $"CPU: {cpu} • GPU: {gpu} • RAM: {ram} • Disk: {disk}";
    }

    private static string DetectVpnPresence()
    {
        foreach (var process in Process.GetProcesses())
        {
            var name = process.ProcessName.ToLowerInvariant();
            if (VpnProcessHints.Any(hint => name.Contains(hint)))
            {
                return "Возможно";
            }
        }

        return "Нет";
    }

    private static int GetUserCount()
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

    private static string GetUptime()
    {
        var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
        return $"{(int)uptime.TotalHours}h {(int)uptime.Minutes}m";
    }

    private static List<ProcessDetail> GetProcessDetails()
    {
        var list = new List<ProcessDetail>();
        foreach (var process in Process.GetProcesses().OrderBy(p => p.ProcessName))
        {
            string path = string.Empty;
            try
            {
                path = process.MainModule?.FileName ?? string.Empty;
            }
            catch
            {
            }

            double memMb = 0;
            try
            {
                memMb = process.WorkingSet64 / 1024d / 1024d;
            }
            catch
            {
            }

            list.Add(new ProcessDetail
            {
                Name = process.ProcessName,
                Id = process.Id,
                Path = path,
                MemoryMb = Math.Round(memMb, 1)
            });
        }

        return list;
    }

    private static List<string> IdentifySuspiciousProcesses(List<ProcessDetail> processes)
    {
        var result = new List<string>();
        foreach (var process in processes)
        {
            var name = process.Name.ToLowerInvariant();
            var path = process.Path.ToLowerInvariant();
            if (SuspiciousProcessKeywords.Any(keyword => name.Contains(keyword) || path.Contains(keyword)))
            {
                result.Add($"{process.Name} (PID {process.Id})");
            }
        }

        return result.Distinct().ToList();
    }

    private static List<FileFinding> LocateSuspiciousFiles()
    {
        var suspiciousFiles = new List<FileFinding>();
        var roots = new List<string>();
        try
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (!string.IsNullOrWhiteSpace(userProfile))
            {
                roots.Add(Path.Combine(userProfile, "Downloads"));
                roots.Add(Path.Combine(userProfile, "Desktop"));
                roots.Add(Path.Combine(userProfile, "AppData", "Local"));
                roots.Add(Path.Combine(userProfile, "AppData", "Roaming"));
            }
        }
        catch
        {
        }

        roots.Add("C:\\ProgramData");
        roots.Add("C:\\Program Files");
        roots.Add("C:\\Program Files (x86)");

        foreach (var root in roots.Distinct())
        {
            if (!Directory.Exists(root))
            {
                continue;
            }

            try
            {
                foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
                {
                    if (IsFileSuspicious(file))
                    {
                        long size = 0;
                        try
                        {
                            size = new FileInfo(file).Length;
                        }
                        catch
                        {
                        }

                        suspiciousFiles.Add(new FileFinding
                        {
                            Path = file,
                            SizeBytes = size
                        });
                    }
                }
            }
            catch
            {
                // ignore access denied
            }
        }

        return suspiciousFiles;
    }

    private static bool IsFileSuspicious(string file)
    {
        var lower = file.ToLowerInvariant();
        if (SuspiciousFileExtensions.Any(ext => lower.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
        {
            if (SuspiciousFileKeywords.Any(keyword => lower.Contains(keyword)))
            {
                return true;
            }
        }

        return false;
    }

    private static List<string> CheckRegistry()
    {
        var suspiciousKeys = new List<string>();
        var keysToCheck = new[]
        {
            @"Software\\Cheat Engine",
            @"Software\\CheatEngine",
            @"Software\\GameOverlay",
            @"Software\\Injector",
            @"Software\\CS2\\Loader"
        };

        foreach (var keyPath in keysToCheck)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(keyPath);
                if (key != null)
                {
                    suspiciousKeys.Add($"HKCU\\{keyPath}");
                }
            }
            catch
            {
            }

            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(keyPath);
                if (key != null)
                {
                    suspiciousKeys.Add($"HKLM\\{keyPath}");
                }
            }
            catch
            {
            }
        }

        return suspiciousKeys.Distinct().ToList();
    }

    private static List<DrsEntry> ReadNvidiaDrs()
    {
        var result = new List<DrsEntry>();
        try
        {
            var drsPath = "C:\\ProgramData\\NVIDIA Corporation\\Drs\\nvAppTimestamps";
            if (!File.Exists(drsPath))
            {
                return result;
            }

            var bytes = File.ReadAllBytes(drsPath);
            var sanitized = SanitizeDrsString(bytes);
            foreach (var line in sanitized.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = line.Trim();
                if (trimmed.Length == 0)
                {
                    continue;
                }

                var parts = trimmed.Split(' ');
                if (parts.Length >= 2)
                {
                    var profile = parts[0];
                    var value = string.Join(' ', parts.Skip(1));
                    result.Add(new DrsEntry { Profile = profile, Value = value });
                }
                else
                {
                    result.Add(new DrsEntry { Profile = "entry", Value = trimmed });
                }
            }
        }
        catch
        {
        }

        return result;
    }

    private static string SanitizeDrsString(byte[] bytes)
    {
        var builder = new StringBuilder();
        foreach (var b in bytes)
        {
            if (b == 0)
            {
                builder.Append('\n');
                continue;
            }

            if (b < 32)
            {
                continue;
            }

            builder.Append((char)b);
        }

        return builder.ToString();
    }

    private static string FormatSize(ulong bytes)
    {
        double size = bytes;
        string[] units = { "B", "KB", "MB", "GB", "TB" };
        int index = 0;
        while (size >= 1024 && index < units.Length - 1)
        {
            size /= 1024;
            index++;
        }
        return $"{size:F1} {units[index]}";
    }
}
