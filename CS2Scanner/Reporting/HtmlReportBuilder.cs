using System.Linq;
using System.Net;
using System.Text;
using CS2Scanner.Models;

namespace CS2Scanner.Reporting;

internal static class HtmlReportBuilder
{
    public static string Build(ScanReport report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<!DOCTYPE html>");
        builder.AppendLine("<html lang=\"ru\">");
        builder.AppendLine("<head>");
        builder.AppendLine("  <meta charset=\"UTF-8\" />");
        builder.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"/>");
        builder.AppendLine("  <title>CS2 Scan Report</title>");
        builder.AppendLine("  <style>");
        builder.AppendLine("    :root{ --bg1:#ff758c; --bg2:#ffa647; --bg3:#ff7eb3; --bg4:#ffd36b; --panel-bg: rgba(0,0,0,0.45); --panel-radius: 14px; --text:#ffffff; --muted: rgba(255,255,255,0.75); --table-border: rgba(255,255,255,0.12); }");
        builder.AppendLine("    html,body{ height:100%; margin:0; font-family: 'Segoe UI', Roboto, system-ui, -apple-system, 'Helvetica Neue', Arial; color:var(--text); -webkit-font-smoothing:antialiased; -moz-osx-font-smoothing:grayscale; background: linear-gradient(135deg,var(--bg1),var(--bg2),var(--bg3),var(--bg4)); background-size:400% 400%; animation: gradientShift 18s ease-in-out infinite; }");
        builder.AppendLine("    @keyframes gradientShift { 0% { background-position: 0% 50%; } 50% { background-position: 100% 50%; } 100% { background-position: 0% 50%; } }");
        builder.AppendLine("    h1 { text-align:center; margin:18px 0 8px; text-shadow:0 2px 8px rgba(0,0,0,0.25); }");
        builder.AppendLine("    .container { width:calc(100% - 40px); max-width:900px; margin:20px auto; background:var(--panel-bg); border-radius:var(--panel-radius); padding:20px; box-shadow: 0 8px 30px rgba(0,0,0,0.35); backdrop-filter: blur(6px); }");
        builder.AppendLine("    .row { display:flex; gap:18px; flex-wrap:wrap; }");
        builder.AppendLine("    .col { flex:1 1 240px; min-width:200px; }");
        builder.AppendLine("    .small { font-size:0.86rem; color:var(--muted); margin:4px 0 10px; }");
        builder.AppendLine("    .info-grid { display:grid; grid-template-columns: 1fr 1fr; gap:12px; margin-top:8px; }");
        builder.AppendLine("    .info-card { background: rgba(255,255,255,0.03); padding:10px; border-radius:10px; border:1px solid rgba(255,255,255,0.03); }");
        builder.AppendLine("    .info-card strong { display:block; margin-bottom:6px; font-weight:600; }");
        builder.AppendLine("    .info-value { font-size:0.95rem; color: #fff; }");
        builder.AppendLine("    table { width:100%; border-collapse:collapse; margin-top:12px; color:var(--text); font-size:0.92rem; background: transparent; }");
        builder.AppendLine("    th, td { padding:10px 8px; border-bottom:1px solid var(--table-border); text-align:left; }");
        builder.AppendLine("    th { background: rgba(255,255,255,0.04); font-weight:600; }");
        builder.AppendLine("    .muted { color:var(--muted); font-size:0.85rem; }");
        builder.AppendLine("    .judgement { font-size:1.1rem; text-align:center; margin-top:12px; font-weight:700; }");
        builder.AppendLine("    .clean { color:#8aff8a; }");
        builder.AppendLine("    .cheating { color:#ff6b6b; }");
        builder.AppendLine("    .footer { text-align:center; margin-top:18px; font-size:0.88rem; opacity:0.85; }");
        builder.AppendLine("    @media (max-width:640px){ .info-grid { grid-template-columns:1fr; } .row { gap:12px; } }");
        builder.AppendLine("  </style>");
        builder.AppendLine("</head>");
        builder.AppendLine("<body>");
        builder.AppendLine("  <h1>CS2 Scan Report</h1>");
        builder.AppendLine("  <div class=\"container\">");
        builder.AppendLine("    <h2>Системная информация</h2>");
        builder.AppendLine("    <div class=\"small\">Обзор системы и текущее состояние — данные получены сканером.</div>");
        builder.AppendLine("    <div class=\"info-grid\">");
        builder.AppendLine(BuildInfoCard("Система", report.SystemInfo, "ОС / архитектура"));
        builder.AppendLine(BuildInfoCard("Скан выполнен за", report.ScanTime, "Время выполнения"));
        builder.AppendLine(BuildInfoCard("Имя ПК", report.MachineName, "Environment.MachineName"));
        builder.AppendLine(BuildInfoCard("Виртуальная машина", report.VmStatus, "Определение гипервизора"));
        builder.AppendLine(BuildInfoCard("Характеристики", report.HardwareSpec, "CPU / GPU / RAM / Disk"));
        builder.AppendLine(BuildInfoCard("VPN", report.VpnStatus, "Подключён ли VPN"));
        builder.AppendLine(BuildInfoCard("Пользователи", report.UserCount.ToString(), "Число локальных учётных записей"));
        builder.AppendLine(BuildInfoCard("Время с последней перезагрузки", report.BootTime, "Uptime"));
        builder.AppendLine("    </div>");

        builder.AppendLine("    <h2 style=\"margin-top:18px;\">Подозрительные процессы</h2>");
        builder.AppendLine("    <table><thead><tr><th>Процесс</th></tr></thead><tbody>");
        if (report.SuspiciousProcesses.Any())
        {
            foreach (var process in report.SuspiciousProcesses)
            {
                builder.AppendLine($"      <tr><td>{WebUtility.HtmlEncode(process)}</td></tr>");
            }
        }
        else
        {
            builder.AppendLine("      <tr><td>Подозрительных процессов не обнаружено</td></tr>");
        }
        builder.AppendLine("    </tbody></table>");

        builder.AppendLine("    <h2 style=\"margin-top:14px;\">Подозрительные файлы</h2>");
        builder.AppendLine("    <table><thead><tr><th>Файл</th><th>Размер</th></tr></thead><tbody>");
        if (report.SuspiciousFiles.Any())
        {
            foreach (var file in report.SuspiciousFiles)
            {
                builder.AppendLine($"      <tr><td>{WebUtility.HtmlEncode(file.Path)}</td><td>{WebUtility.HtmlEncode(file.GetReadableSize())}</td></tr>");
            }
        }
        else
        {
            builder.AppendLine("      <tr><td colspan=\"2\">Подозрительных файлов не обнаружено</td></tr>");
        }
        builder.AppendLine("    </tbody></table>");

        builder.AppendLine("    <h2 style=\"margin-top:14px;\">Подозрительные ключи реестра</h2>");
        builder.AppendLine("    <table><thead><tr><th>Ключ</th></tr></thead><tbody>");
        if (report.SuspiciousRegistry.Any())
        {
            foreach (var key in report.SuspiciousRegistry)
            {
                builder.AppendLine($"      <tr><td>{WebUtility.HtmlEncode(key)}</td></tr>");
            }
        }
        else
        {
            builder.AppendLine("      <tr><td>Ничего не найдено</td></tr>");
        }
        builder.AppendLine("    </tbody></table>");

        builder.AppendLine("    <h2 style=\"margin-top:14px;\">Активные процессы</h2>");
        builder.AppendLine("    <div class=\"small\">Формат: Имя | PID | Путь | Память (MB)</div>");
        builder.AppendLine("    <table><thead><tr><th>Имя процесса</th><th>PID</th><th>Путь</th><th>Память (MB)</th></tr></thead><tbody>");
        foreach (var process in report.Processes)
        {
            builder.AppendLine($"      <tr><td>{WebUtility.HtmlEncode(process.Name)}</td><td>{process.Id}</td><td>{WebUtility.HtmlEncode(process.Path)}</td><td>{process.MemoryMb:F1}</td></tr>");
        }
        builder.AppendLine("    </tbody></table>");

        builder.AppendLine("    <h2 style=\"margin-top:14px;\">NVIDIA DRS (nvAppTimestamps)</h2>");
        builder.AppendLine("    <div class=\"small\">Конфигурация из nvAppTimestamps</div>");
        builder.AppendLine("    <table><thead><tr><th>Профиль / Параметр</th><th>Значение</th></tr></thead><tbody>");
        if (report.NvidiaDrs.Any())
        {
            foreach (var entry in report.NvidiaDrs)
            {
                builder.AppendLine($"      <tr><td>{WebUtility.HtmlEncode(entry.Profile)}</td><td>{WebUtility.HtmlEncode(entry.Value)}</td></tr>");
            }
        }
        else
        {
            builder.AppendLine("      <tr><td colspan=\"2\">Данные отсутствуют или файл недоступен</td></tr>");
        }
        builder.AppendLine("    </tbody></table>");

        var cssClass = report.Judgement.Contains("Clean", System.StringComparison.OrdinalIgnoreCase) ? "clean" : "cheating";
        builder.AppendLine($"    <div class=\"judgement {cssClass}\">{WebUtility.HtmlEncode(report.Judgement)}</div>");
        builder.AppendLine("  </div>");
        builder.AppendLine("  <div class=\"footer\">Отчёт создан сканером CS2 | © 2025</div>");
        builder.AppendLine("</body>");
        builder.AppendLine("</html>");
        return builder.ToString();
    }

    private static string BuildInfoCard(string title, string value, string subtitle)
    {
        return $"      <div class=\"info-card\"><strong>{WebUtility.HtmlEncode(title)}</strong><div class=\"info-value\">{WebUtility.HtmlEncode(value)}</div><div class=\"muted\">{WebUtility.HtmlEncode(subtitle)}</div></div>";
    }
}
