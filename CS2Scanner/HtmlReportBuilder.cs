using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace CS2Scanner
{
    public static class HtmlReportBuilder
    {
        public static string Build(ReportData data)
        {
            string templatePath = Path.Combine(AppContext.BaseDirectory, "Assets", "report_template.html");
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException("Не найден шаблон отчёта", templatePath);
            }

            string template = File.ReadAllText(templatePath, Encoding.UTF8);

            string cheatProcessRows = BuildRows(data.SuspiciousProcesses.Select(WebUtility.HtmlEncode), singleColumn: true);
            string fileRows = BuildFileRows(data.SuspiciousFiles);
            string registryRows = BuildRows(data.SuspiciousRegistry.Select(WebUtility.HtmlEncode), singleColumn: true);
            string processRows = BuildProcessRows(data.Processes);
            string nvidiaDrsRows = BuildNvidiaDrsRows(data.NvidiaDrsEntries);

            string judgementClass = data.Judgement.Contains("Clean", StringComparison.OrdinalIgnoreCase) ? "clean" : "cheating";

            return template
                .Replace("{{SystemInfo}}", WebUtility.HtmlEncode(data.SystemInfo))
                .Replace("{{ScanTime}}", WebUtility.HtmlEncode(data.ScanTime))
                .Replace("{{MachineName}}", WebUtility.HtmlEncode(data.MachineName))
                .Replace("{{VmStatus}}", WebUtility.HtmlEncode(data.VmStatus))
                .Replace("{{HardwareSpec}}", WebUtility.HtmlEncode(data.HardwareSpec))
                .Replace("{{VpnStatus}}", WebUtility.HtmlEncode(data.VpnStatus))
                .Replace("{{UserCount}}", WebUtility.HtmlEncode(data.UserCount.ToString()))
                .Replace("{{BootTime}}", WebUtility.HtmlEncode(data.BootTime))
                .Replace("{{Judgement}}", WebUtility.HtmlEncode(data.Judgement))
                .Replace("{{JudgementClass}}", judgementClass)
                .Replace("{{CheatProcessRows}}", cheatProcessRows)
                .Replace("{{SuspiciousFileRows}}", fileRows)
                .Replace("{{SuspiciousRegistryRows}}", registryRows)
                .Replace("{{AllProcessRows}}", processRows)
                .Replace("{{NvidiaDrsRows}}", nvidiaDrsRows);
        }

        private static string BuildRows(IEnumerable<string> values, bool singleColumn)
        {
            var sb = new StringBuilder();
            foreach (string value in values)
            {
                sb.Append("<tr>");
                sb.Append(singleColumn ? "<td>" : "<td colspan=\"2\">");
                sb.Append(value);
                sb.Append("</td></tr>");
            }

            if (sb.Length == 0)
            {
                sb.Append(singleColumn ? "<tr><td>Нет</td></tr>" : "<tr><td colspan=\"2\">Нет</td></tr>");
            }

            return sb.ToString();
        }

        private static string BuildFileRows(IReadOnlyList<SuspiciousFile> files)
        {
            if (files.Count == 0)
            {
                return "<tr><td colspan=\"2\">Нет</td></tr>";
            }

            var sb = new StringBuilder();
            foreach (var file in files)
            {
                sb.Append("<tr>");
                sb.Append("<td>").Append(WebUtility.HtmlEncode(file.Name)).Append("</td>");
                sb.Append("<td>").Append(WebUtility.HtmlEncode(file.Size)).Append("</td>");
                sb.Append("</tr>");
            }

            return sb.ToString();
        }

        private static string BuildProcessRows(IReadOnlyList<ProcessDetails> processes)
        {
            if (processes.Count == 0)
            {
                return "<tr><td colspan=\"4\">Нет данных</td></tr>";
            }

            var sb = new StringBuilder();
            foreach (var process in processes)
            {
                sb.Append("<tr>");
                sb.Append("<td>").Append(WebUtility.HtmlEncode(process.Name)).Append("</td>");
                sb.Append("<td>").Append(process.Pid).Append("</td>");
                sb.Append("<td>").Append(WebUtility.HtmlEncode(process.Path)).Append("</td>");
                sb.Append("<td>").Append(process.MemoryMb.ToString("F1")).Append("</td>");
                sb.Append("</tr>");
            }

            return sb.ToString();
        }

        private static string BuildNvidiaDrsRows(IReadOnlyList<NvidiaDrsEntry> entries)
        {
            if (entries.Count == 0)
            {
                return "<tr><td colspan=\"2\">Нет</td></tr>";
            }

            var sb = new StringBuilder();
            foreach (var entry in entries)
            {
                sb.Append("<tr>");
                sb.Append("<td>").Append(WebUtility.HtmlEncode(entry.Label)).Append("</td>");
                sb.Append("<td>").Append(WebUtility.HtmlEncode(entry.Value)).Append("</td>");
                sb.Append("</tr>");
            }

            return sb.ToString();
        }
    }
}
