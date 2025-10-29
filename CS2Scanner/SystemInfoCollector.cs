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
            "cheat", "hack", "inject", "trainer", "bypass", "loader", "radar", "spoof", "macro", "rage", "esp", "aimbot",
            "wall", "trigger", "silent"
        };

        private static readonly string[] SuspiciousFileKeywords = new[]
        {
            "cheat", "inject", "hack", "trainer", "radar", "aim", "wall", "esp", "rage", "spoof", "macro", "bypass", "loader"
        };

        private static readonly string[] SuspiciousExtensions = new[]
        {
            ".exe", ".dll", ".sys", ".asi", ".zip", ".rar", ".7z", ".pak", ".ct", ".cfg"
        };

        private static readonly string[] RunRegistryPaths = new[]
        {
            @"Software\\Microsoft\\Windows\\CurrentVersion\\Run",
            @"Software\\Microsoft\\Windows\\CurrentVersion\\RunOnce",
            @"Software\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run",
            @"Software\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\RunOnce"
        };

        private static readonly Regex MultiWhitespaceRegex = new(@"\s+", RegexOptions.Compiled);
        private static readonly Regex DrsProfileRegex = new(@"([A-Za-z0-9\.\-@_]+)\s+([A-Za-z]:/[^ ]+)", RegexOptions.Compiled);
        private static readonly Regex DrsPathRegex = new(@"([A-Za-z]:/[^ ]+)", RegexOptions.Compiled);
        private static readonly string[] RegistryPathsToCheck = new[]
        {
            @"Software\\Cheat Engine",
            @"Software\\CheatEngine",
            @"Software\\GameTrainer",
            @"SOFTWARE\\Cheat Engine",
            @"SOFTWARE\\Wow6432Node\\Cheat Engine"
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

            report.NvidiaDrsEntries = LoadNvidiaDrsEntries();

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
            var candidateDirectories = new List<string>();

            void AddIfExists(string? path)
            {
                if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                {
                    candidateDirectories.Add(path);
                }
            }

            AddIfExists(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
            AddIfExists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            AddIfExists(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments));
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (!string.IsNullOrWhiteSpace(userProfile))
            {
                AddIfExists(Path.Combine(userProfile, "Downloads"));
                AddIfExists(Path.Combine(userProfile, "AppData", "Local"));
                AddIfExists(Path.Combine(userProfile, "AppData", "Roaming"));
            }

            AddIfExists(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));

            foreach (var dir in candidateDirectories.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                foreach (var file in EnumerateFilesSafe(dir, maxDepth: 2))
                {
                    string extension = Path.GetExtension(file);
                    if (string.IsNullOrEmpty(extension) || !SuspiciousExtensions.Contains(extension.ToLowerInvariant()))
                    {
                        continue;
                    }

                    string lowerCandidate = file.ToLowerInvariant();
                    if (!SuspiciousFileKeywords.Any(k => lowerCandidate.Contains(k)))
                    {
                        continue;
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
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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

            foreach (var entry in InspectRunKeys(RegistryHive.CurrentUser))
            {
                result.Add(entry);
            }

            foreach (var entry in InspectRunKeys(RegistryHive.LocalMachine))
            {
                result.Add(entry);
            }

            return result.OrderBy(v => v, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static IEnumerable<string> InspectRunKeys(RegistryHive hive)
        {
            foreach (var path in RunRegistryPaths)
            {
                try
                {
                    using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
                    using var key = baseKey.OpenSubKey(path);
                    if (key != null)
                    {
                        foreach (var valueName in key.GetValueNames())
                        {
                            string? rawValue = key.GetValue(valueName)?.ToString();
                            if (string.IsNullOrWhiteSpace(rawValue))
                                continue;

                            string lower = rawValue.ToLowerInvariant();
                            bool keywordMatch = SuspiciousFileKeywords.Any(k => lower.Contains(k));
                            bool extensionMatch = SuspiciousExtensions.Any(ext => lower.Contains(ext));
                            if (!keywordMatch && !extensionMatch)
                                continue;

                            yield return $"{GetHivePrefix(hive)}\\{path} -> {valueName} = {rawValue}";
                        }
                    }
                }
                catch
                {
                    // ignore access errors
                }

                try
                {
                    using var baseKey32 = RegistryKey.OpenBaseKey(hive, RegistryView.Registry32);
                    using var key32 = baseKey32.OpenSubKey(path);
                    if (key32 == null)
                        continue;

                    foreach (var valueName in key32.GetValueNames())
                    {
                        string? rawValue = key32.GetValue(valueName)?.ToString();
                        if (string.IsNullOrWhiteSpace(rawValue))
                            continue;

                        string lower = rawValue.ToLowerInvariant();
                        bool keywordMatch = SuspiciousFileKeywords.Any(k => lower.Contains(k));
                        bool extensionMatch = SuspiciousExtensions.Any(ext => lower.Contains(ext));
                        if (!keywordMatch && !extensionMatch)
                            continue;

                        yield return $"{GetHivePrefix(hive)}\\{path} -> {valueName} = {rawValue}";
                    }
                }
                catch
                {
                    // ignore access errors
                }
            }
        }

        private static string GetHivePrefix(RegistryHive hive)
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

        private static IReadOnlyList<NvidiaDrsEntry> LoadNvidiaDrsEntries()
        {
            var entries = new List<NvidiaDrsEntry>();
            try
            {
                string basePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                if (string.IsNullOrWhiteSpace(basePath))
                {
                    return entries;
                }

                string filePath = Path.Combine(basePath, "NVIDIA Corporation", "Drs", "nvAppTimestamps");
                if (!File.Exists(filePath))
                {
                    return entries;
                }

                byte[] raw = File.ReadAllBytes(filePath);
                if (raw.Length == 0)
                {
                    return entries;
                }

                var printable = new StringBuilder(raw.Length);
                foreach (byte b in raw)
                {
                    if (b >= 32 && b <= 126)
                    {
                        printable.Append((char)b);
                    }
                    else
                    {
                        printable.Append(' ');
                    }
                }

                string sanitized = MultiWhitespaceRegex.Replace(printable.ToString(), " ").Trim();
                if (sanitized.Length == 0)
                {
                    return entries;
                }

                var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (Match match in DrsProfileRegex.Matches(sanitized))
                {
                    string profile = match.Groups[1].Value;
                    string value = match.Groups[2].Value.Replace('/', '\\');
                    string key = profile + "|" + value;
                    if (unique.Add(key))
                    {
                        entries.Add(new NvidiaDrsEntry
                        {
                            ProfileOrKey = profile,
                            Value = value
                        });
                    }
                }

                if (entries.Count == 0)
                {
                    int index = 1;
                    foreach (Match match in DrsPathRegex.Matches(sanitized))
                    {
                        string value = match.Value.Replace('/', '\\');
                        if (unique.Add(value))
                        {
                            entries.Add(new NvidiaDrsEntry
                            {
                                ProfileOrKey = $"Path #{index++}",
                                Value = value
                            });
                        }
                    }
                }
            }
            catch
            {
                // ignore read/parsing errors
            }

            return entries;
        }

        private static IEnumerable<string> EnumerateFilesSafe(string root, int maxDepth)
        {
            var stack = new Stack<(string Directory, int Depth)>();
            stack.Push((root, 0));

            while (stack.Count > 0)
            {
                var (current, depth) = stack.Pop();

                IEnumerable<string> files;
                try
                {
                    files = Directory.EnumerateFiles(current, "*", SearchOption.TopDirectoryOnly);
                }
                catch
                {
                    continue;
                }

                foreach (var file in files)
                {
                    yield return file;
                }

                if (depth >= maxDepth)
                {
                    continue;
                }

                IEnumerable<string> subDirectories;
                try
                {
                    subDirectories = Directory.EnumerateDirectories(current, "*", SearchOption.TopDirectoryOnly);
                }
                catch
                {
                    continue;
                }

                foreach (var subDir in subDirectories)
                {
                    stack.Push((subDir, depth + 1));
                }
            }
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
