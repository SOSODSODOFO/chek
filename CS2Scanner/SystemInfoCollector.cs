using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace CS2Scanner
{
    public class SystemInfoCollector
    {
        private static readonly string[] SuspiciousProcessKeywords = new[]
        {
            "cheat", "hack", "inject", "trainer", "bypass", "loader", "radar", "xenos",
            "x64dbg", "x32dbg", "aimbot", "wallhack", "trigger", "macro", "spoof", "rage",
            "silent", "esp", "no recoil", "bhop", "script"
        };

        private static readonly string[] SuspiciousFileKeywords = new[]
        {
            "cheat", "inject", "hack", "trainer", "radar", "aim", "wall", "esp", "macro",
            "trigger", "bypass", "spoof", "rage", "silent", "loader", "injector", "xenos",
            "x64dbg", "x32dbg", "ragebot"
        };

        private static readonly string[] SuspiciousFileExtensions = new[]
        {
            ".exe", ".dll", ".sys", ".asi", ".zip", ".rar", ".7z", ".tar", ".gz",
            ".bat", ".cmd", ".ps1", ".vbs", ".cfg", ".ini", ".json", ".txt"
        };

        private static readonly string[] RegistryPathsToCheck = new[]
        {
            @"Software\\Cheat Engine",
            @"Software\\CheatEngine",
            @"Software\\GameTrainer",
            @"SOFTWARE\\Cheat Engine",
            @"SOFTWARE\\Wow6432Node\\Cheat Engine"
        };

        private static readonly (RegistryHive Hive, string Path)[] RegistryValueLocations = new[]
        {
            (RegistryHive.CurrentUser, @"Software\\Microsoft\\Windows\\CurrentVersion\\Run"),
            (RegistryHive.CurrentUser, @"Software\\Microsoft\\Windows\\CurrentVersion\\RunOnce"),
            (RegistryHive.LocalMachine, @"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run"),
            (RegistryHive.LocalMachine, @"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce"),
            (RegistryHive.LocalMachine, @"SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run"),
            (RegistryHive.LocalMachine, @"SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\RunOnce")
        };

        private static readonly string[] RegistryValueKeywords = new[]
        {
            "cheat", "hack", "inject", "trainer", "radar", "aim", "wall", "esp", "macro",
            "trigger", "bypass", "spoof", "rage", "silent", "loader", "injector", "xenos",
            "x64dbg", "x32dbg"
        };

        public ReportData Collect()
        {
            var report = new ReportData
            {
                MachineName = Environment.MachineName,
                SystemInfo = GetSystemInfo(),
                VmStatus = DetectVirtualMachine() ? "Да" : "Нет",
                HardwareSpec = BuildHardwareSpec(),
                VpnStatus = DetectVpnConnection() ? "Да" : "Нет",
                UserCount = GetLocalUserCount(),
                BootTime = GetUptime(),
            };

            var suspiciousProcesses = new List<string>();
            report.Processes = GetProcessList(suspiciousProcesses);
            report.SuspiciousProcesses = suspiciousProcesses.Count == 0
                ? new List<string> { "Нет" }
                : suspiciousProcesses;

            var suspiciousFiles = FindSuspiciousFiles();
            report.SuspiciousFiles = suspiciousFiles;

            var suspiciousRegistry = FindSuspiciousRegistryKeys();
            report.SuspiciousRegistry = suspiciousRegistry.Count == 0
                ? new List<string> { "Нет" }
                : suspiciousRegistry;

            report.NvidiaDrsEntries = ParseNvidiaDrsEntries();

            bool hasSuspiciousData = suspiciousProcesses.Any(sp => !string.Equals(sp, "Нет", StringComparison.OrdinalIgnoreCase))
                || suspiciousFiles.Count > 0
                || suspiciousRegistry.Any(key => !string.Equals(key, "Нет", StringComparison.OrdinalIgnoreCase));

            report.Judgement = hasSuspiciousData ? "Возможны читы ❌" : "Clean ✅";
            return report;
        }

        private static string GetSystemInfo()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Caption, Version FROM Win32_OperatingSystem");
                foreach (var obj in searcher.Get().Cast<ManagementObject>())
                {
                    string caption = obj["Caption"]?.ToString() ?? Environment.OSVersion.ToString();
                    string version = obj["Version"]?.ToString() ?? string.Empty;
                    return string.IsNullOrWhiteSpace(version)
                        ? $"{caption} ({RuntimeInformation.OSArchitecture})"
                        : $"{caption} {version} ({RuntimeInformation.OSArchitecture})";
                }
            }
            catch
            {
                // ignored - fallback below
            }

            return $"{Environment.OSVersion} ({RuntimeInformation.OSArchitecture})";
        }

        private static bool DetectVirtualMachine()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Model FROM Win32_ComputerSystem");
                foreach (var obj in searcher.Get().Cast<ManagementObject>())
                {
                    string manufacturer = (obj["Manufacturer"]?.ToString() ?? string.Empty).ToLowerInvariant();
                    string model = (obj["Model"]?.ToString() ?? string.Empty).ToLowerInvariant();

                    if (manufacturer.Contains("microsoft corporation") && model.Contains("virtual"))
                        return true;
                    if (manufacturer.Contains("vmware") || model.Contains("vmware"))
                        return true;
                    if (model.Contains("virtualbox"))
                        return true;
                    if (manufacturer.Contains("parallels"))
                        return true;
                    if (model.Contains("kvm") || manufacturer.Contains("qemu"))
                        return true;
                }
            }
            catch
            {
                // ignore errors and assume physical machine
            }

            return false;
        }

        private static string BuildHardwareSpec()
        {
            string cpu = "Не удалось определить CPU";
            string gpu = "Не удалось определить GPU";
            double ramGb = 0;
            double diskGb = 0;

            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
                var cpus = searcher.Get().Cast<ManagementObject>()
                    .Select(mo => mo["Name"]?.ToString()?.Trim())
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct()
                    .ToList();
                if (cpus.Count > 0)
                {
                    cpu = string.Join(", ", cpus);
                }
            }
            catch { }

            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController");
                var gpus = searcher.Get().Cast<ManagementObject>()
                    .Select(mo => mo["Name"]?.ToString()?.Trim())
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct()
                    .ToList();
                if (gpus.Count > 0)
                {
                    gpu = string.Join(", ", gpus);
                }
            }
            catch { }

            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
                foreach (var obj in searcher.Get().Cast<ManagementObject>())
                {
                    if (obj["TotalPhysicalMemory"] is ulong memory)
                    {
                        ramGb = Math.Round(memory / 1024d / 1024d / 1024d, 1);
                        break;
                    }
                }
            }
            catch { }

            try
            {
                foreach (var drive in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed && d.IsReady))
                {
                    diskGb += drive.TotalSize / 1024d / 1024d / 1024d;
                }
                diskGb = Math.Round(diskGb, 1);
            }
            catch { }

            return $"CPU: {cpu} • GPU: {gpu} • RAM: {ramGb:F1} GB • Disk: {diskGb:F1} GB";
        }

        private static bool DetectVpnConnection()
        {
            try
            {
                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != OperationalStatus.Up)
                        continue;

                    var type = ni.NetworkInterfaceType;
                    string description = ni.Description.ToLowerInvariant();
                    string name = ni.Name.ToLowerInvariant();

                    if (type == NetworkInterfaceType.Tunnel || type == NetworkInterfaceType.Ppp)
                        return true;

                    if (description.Contains("vpn") || description.Contains("virtual"))
                        return true;

                    if (name.Contains("vpn"))
                        return true;
                }
            }
            catch
            {
                // ignore network errors
            }

            return false;
        }

        private static int GetLocalUserCount()
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
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT LastBootUpTime FROM Win32_OperatingSystem");
                foreach (var obj in searcher.Get().Cast<ManagementObject>())
                {
                    string? raw = obj["LastBootUpTime"]?.ToString();
                    if (string.IsNullOrWhiteSpace(raw))
                        continue;

                    DateTime bootTime = ManagementDateTimeConverter.ToDateTime(raw);
                    TimeSpan uptime = DateTime.Now - bootTime;
                    return FormatTimeSpan(uptime);
                }
            }
            catch
            {
                // ignore errors
            }

            return "—";
        }

        private static string FormatTimeSpan(TimeSpan span)
        {
            if (span.TotalDays >= 1)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}d {1}h {2}m", (int)span.TotalDays, span.Hours, span.Minutes);
            }

            if (span.TotalHours >= 1)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}h {1}m", (int)span.TotalHours, span.Minutes);
            }

            return string.Format(CultureInfo.InvariantCulture, "{0}m", (int)Math.Max(1, span.TotalMinutes));
        }

        private static IReadOnlyList<ProcessDetails> GetProcessList(List<string> suspiciousProcesses)
        {
            var processes = new List<ProcessDetails>();
            try
            {
                foreach (var process in Process.GetProcesses().OrderByDescending(p => p.WorkingSet64))
                {
                    string name = process.ProcessName;
                    int pid = process.Id;
                    string path = "Недоступно";
                    double memoryMb = Math.Round(process.WorkingSet64 / 1024d / 1024d, 1);

                    try
                    {
                        path = process.MainModule?.FileName ?? path;
                    }
                    catch
                    {
                        // Access denied
                    }

                    processes.Add(new ProcessDetails
                    {
                        Name = name,
                        Pid = pid,
                        Path = path,
                        MemoryMb = memoryMb
                    });

                    string lowerName = name.ToLowerInvariant();
                    string lowerPath = path.ToLowerInvariant();
                    if (SuspiciousProcessKeywords.Any(k => lowerName.Contains(k) || lowerPath.Contains(k)))
                    {
                        suspiciousProcesses.Add($"{name} (PID {pid})");
                    }
                }
            }
            catch
            {
                // ignore
            }

            return processes;
        }

        private static List<SuspiciousFile> FindSuspiciousFiles()
        {
            var results = new List<SuspiciousFile>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var candidateDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            void AddIfExists(string? path)
            {
                if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                {
                    candidateDirectories.Add(path);
                }
            }

            AddIfExists(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
            AddIfExists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (!string.IsNullOrWhiteSpace(userProfile))
            {
                AddIfExists(Path.Combine(userProfile, "Downloads"));
                AddIfExists(Path.Combine(userProfile, "Desktop"));
                AddIfExists(Path.Combine(userProfile, "Documents"));
                AddIfExists(Path.Combine(userProfile, "AppData", "Local"));
                AddIfExists(Path.Combine(userProfile, "AppData", "LocalLow"));
                AddIfExists(Path.Combine(userProfile, "AppData", "Roaming"));
                AddIfExists(Path.Combine(userProfile, "AppData", "Local", "Temp"));
            }

            foreach (var dir in candidateDirectories)
            {
                foreach (var file in EnumerateFilesSafe(dir, maxDepth: 2))
                {
                    string lowerPath = file.ToLowerInvariant();
                    string name = Path.GetFileName(file);
                    string lowerName = name.ToLowerInvariant();
                    string extension = Path.GetExtension(file).ToLowerInvariant();

                    bool hasKeyword = SuspiciousFileKeywords.Any(k => lowerName.Contains(k) || lowerPath.Contains(k));
                    if (!hasKeyword)
                    {
                        if (!SuspiciousFileExtensions.Contains(extension))
                        {
                            continue;
                        }

                        string? directoryName = Path.GetDirectoryName(file);
                        string lowerDirectory = directoryName?.ToLowerInvariant() ?? string.Empty;
                        hasKeyword = SuspiciousFileKeywords.Any(k => lowerDirectory.Contains(k));
                        if (!hasKeyword)
                        {
                            continue;
                        }
                    }

                    if (!seen.Add(file))
                    {
                        continue;
                    }

                    try
                    {
                        var info = new FileInfo(file);
                        results.Add(new SuspiciousFile
                        {
                            Name = file,
                            Size = FormatFileSize(info.Length)
                        });
                    }
                    catch
                    {
                        results.Add(new SuspiciousFile
                        {
                            Name = file,
                            Size = "—"
                        });
                    }
                }
            }

            return results;
        }

        private static List<string> FindSuspiciousRegistryKeys()
        {
            var result = new List<string>();
            foreach (var path in RegistryPathsToCheck)
            {
                if (RegistryKeyExists(RegistryHive.CurrentUser, path))
                {
                    result.Add($"HKCU\\{path}");
                }

                if (RegistryKeyExists(RegistryHive.LocalMachine, path))
                {
                    result.Add($"HKLM\\{path}");
                }
            }

            foreach (var (hive, path) in RegistryValueLocations)
            {
                result.AddRange(FindSuspiciousRegistryValues(hive, path));
            }

            return result;
        }

        private static bool RegistryKeyExists(RegistryHive hive, string path)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
                using var key = baseKey.OpenSubKey(path);
                if (key != null)
                    return true;
            }
            catch
            {
                // ignore
            }

            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry32);
                using var key = baseKey.OpenSubKey(path);
                return key != null;
            }
            catch
            {
                return false;
            }
        }

        private static IEnumerable<string> FindSuspiciousRegistryValues(RegistryHive hive, string path)
        {
            var matches = new List<string>();

            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
                using var key = baseKey.OpenSubKey(path);
                if (key != null)
                {
                    matches.AddRange(InspectRegistryValues(key, hive, path));
                }
            }
            catch
            {
                // ignore registry access errors
            }

            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry32);
                using var key = baseKey.OpenSubKey(path);
                if (key != null)
                {
                    matches.AddRange(InspectRegistryValues(key, hive, path));
                }
            }
            catch
            {
                // ignore registry access errors
            }

            return matches;
        }

        private static IEnumerable<string> InspectRegistryValues(RegistryKey key, RegistryHive hive, string path)
        {
            var matches = new List<string>();
            foreach (var valueName in key.GetValueNames())
            {
                string lowerName = valueName.ToLowerInvariant();
                string valueData = key.GetValue(valueName)?.ToString()?.ToLowerInvariant() ?? string.Empty;

                if (RegistryValueKeywords.Any(k => lowerName.Contains(k) || valueData.Contains(k)))
                {
                    matches.Add($"{GetRegistryHivePrefix(hive)}\\{path}\\{valueName}");
                }
            }

            return matches;
        }

        private static string GetRegistryHivePrefix(RegistryHive hive)
        {
            return hive switch
            {
                RegistryHive.CurrentUser => "HKCU",
                RegistryHive.LocalMachine => "HKLM",
                RegistryHive.Users => "HKU",
                RegistryHive.ClassesRoot => "HKCR",
                RegistryHive.CurrentConfig => "HKCC",
                _ => hive.ToString()
            };
        }

        private static IEnumerable<string> EnumerateFilesSafe(string root, int maxDepth)
        {
            var stack = new Stack<(string path, int depth)>();
            stack.Push((root, 0));

            while (stack.Count > 0)
            {
                var (current, depth) = stack.Pop();
                string[] files = Array.Empty<string>();
                try
                {
                    files = Directory.GetFiles(current);
                }
                catch
                {
                    // ignore
                }

                foreach (var file in files)
                {
                    yield return file;
                }

                if (depth >= maxDepth)
                {
                    continue;
                }

                string[] directories;
                try
                {
                    directories = Directory.GetDirectories(current);
                }
                catch
                {
                    continue;
                }

                foreach (var directory in directories)
                {
                    stack.Push((directory, depth + 1));
                }
            }
        }

        private static IReadOnlyList<NvidiaDrsEntry> ParseNvidiaDrsEntries()
        {
            var entries = new List<NvidiaDrsEntry>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                string commonData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                if (string.IsNullOrWhiteSpace(commonData))
                {
                    return entries;
                }

                string drsPath = Path.Combine(commonData, "NVIDIA Corporation", "Drs", "nvAppTimestamps");
                if (!File.Exists(drsPath))
                {
                    return entries;
                }

                byte[] bytes = File.ReadAllBytes(drsPath);
                if (bytes.Length == 0)
                {
                    return entries;
                }

                var tokens = ExtractAsciiTokens(bytes);
                string? lastLabel = null;
                var pathRegex = new Regex(@"(//\?/)?[a-zA-Z]:/[^\s\r\n]+", RegexOptions.IgnoreCase);

                foreach (string token in tokens)
                {
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        continue;
                    }

                    var match = pathRegex.Match(token);
                    if (match.Success)
                    {
                        string rawPath = match.Value;
                        string normalizedPath = NormalizePathForDisplay(rawPath);
                        if (!seen.Add(normalizedPath))
                        {
                            lastLabel = null;
                            continue;
                        }

                        string prefix = token.Substring(0, match.Index).Trim();
                        string label = string.IsNullOrEmpty(prefix) ? lastLabel ?? "—" : prefix;

                        entries.Add(new NvidiaDrsEntry
                        {
                            Label = label,
                            Value = normalizedPath
                        });

                        lastLabel = null;
                    }
                    else if (token.Any(char.IsLetterOrDigit) && token.Length < 256)
                    {
                        lastLabel = token.Trim();
                    }
                }
            }
            catch
            {
                // ignore parsing errors
            }

            return entries;
        }

        private static List<string> ExtractAsciiTokens(byte[] bytes)
        {
            var tokens = new List<string>();
            var builder = new StringBuilder();

            foreach (byte b in bytes)
            {
                if (b >= 32 && b <= 126)
                {
                    builder.Append((char)b);
                }
                else
                {
                    if (builder.Length > 0)
                    {
                        tokens.Add(builder.ToString());
                        builder.Clear();
                    }
                }
            }

            if (builder.Length > 0)
            {
                tokens.Add(builder.ToString());
            }

            return tokens;
        }

        private static string NormalizePathForDisplay(string rawPath)
        {
            string normalized = rawPath.Replace('/', '\\');

            if (normalized.StartsWith("\\\\?\\", StringComparison.Ordinal))
            {
                normalized = normalized.Substring(4);
            }

            if (normalized.Length >= 2 && normalized[1] == ':')
            {
                normalized = char.ToUpperInvariant(normalized[0]) + normalized.Substring(1);
            }

            return normalized;
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes >= 1024 * 1024)
            {
                return $"{bytes / 1024d / 1024d:F1} MB";
            }

            if (bytes >= 1024)
            {
                return $"{bytes / 1024d:F0} KB";
            }

            return $"{bytes} B";
        }
    }
}
