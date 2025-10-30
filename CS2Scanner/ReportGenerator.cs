using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CS2Scanner.Models;

namespace CS2Scanner;

internal static class ReportGenerator
{
    public static async Task<string> BuildReportAsync(SystemScanResult result)
    {
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Assets", "ReportTemplate.html");
        var template = await File.ReadAllTextAsync(templatePath, Encoding.UTF8);

        var replacements = new Dictionary<string, string>
        {
            ["SYSTEM_INFO"] = Html(result.SystemInfo),
            ["SCAN_TIME"] = Html(result.ScanDuration),
            ["MACHINE_NAME"] = Html(result.MachineName),
            ["VM_STATUS"] = Html(result.VmStatus),
            ["HW_SPEC"] = Html(result.HardwareSpec),
            ["VPN_STATUS"] = Html(result.VpnStatus),
            ["USER_COUNT"] = Html(result.UserCount.ToString()),
            ["BOOT_TIME"] = Html(result.BootTime),
            ["CHEAT_PROCESSES"] = BuildList(result.CheatProcesses, item => $"<tr><td>{Html(item)}</td></tr>", "Не обнаружено", 1),
            ["SUSPICIOUS_FILES"] = BuildList(result.SuspiciousFiles, item => $"<tr><td>{Html(item.Path)}</td><td>{Html(item.SizeDescription)}</td></tr>", "Не обнаружено", 2),
            ["SUSPICIOUS_REGISTRY"] = BuildList(result.SuspiciousRegistryKeys, item => $"<tr><td>{Html(item)}</td></tr>", "Не обнаружено", 1),
            ["ALL_PROCESSES"] = BuildList(result.Processes, ProcessRow, "Нет данных", 4),
            ["NVIDIA_DRS"] = BuildList(result.DrsEntries, item => $"<tr><td>{Html(item.Profile)}</td><td>{Html(item.Value)}</td></tr>", "Данные отсутствуют", 2),
            ["JUDGEMENT_TEXT"] = result.IsClean ? "Clean ✅" : "Возможна чит-активность ❌",
            ["JUDGEMENT_CLASS"] = result.IsClean ? "clean" : "cheating"
        };

        foreach (var replacement in replacements)
        {
            template = template.Replace("{{" + replacement.Key + "}}", replacement.Value);
        }

        return template;
    }

    private static string ProcessRow(ProcessDetail process)
    {
        return $"<tr><td>{Html(process.Name)}</td><td>{process.Pid}</td><td>{Html(process.ExecutablePath)}</td><td>{process.MemoryMb:0.##}</td></tr>";
    }

    private static string BuildList<T>(IEnumerable<T> items, Func<T, string> factory, string emptyLabel, int columnCount)
    {
        if (!items.Any())
        {
            return $"<tr><td colspan=\"{columnCount}\">{Html(emptyLabel)}</td></tr>";
        }

        var builder = new StringBuilder();
        foreach (var item in items)
        {
            builder.Append(factory(item));
        }

        return builder.ToString();
    }

    private static string Html(string value) => WebUtility.HtmlEncode(value);
}
