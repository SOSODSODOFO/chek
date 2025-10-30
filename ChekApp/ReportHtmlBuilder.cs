using System.Globalization;
using System.Linq;
using System.Text;

namespace ChekApp
{
    public static class ReportHtmlBuilder
    {
        public static string Build(ScanReport report)
        {
            var builder = new StringBuilder();
            builder.AppendLine("<!DOCTYPE html>");
            builder.AppendLine("<html lang=\"ru\"><head><meta charset=\"utf-8\">\n<title>CS2 Scan Report</title>");
            builder.AppendLine("<style>");
            builder.AppendLine(StyleSheet);
            builder.AppendLine("</style></head><body>");
            builder.AppendLine("  <h1>CS2 Scan Report</h1>");
            builder.AppendLine("  <div class=\"container\">");
            builder.AppendLine("    <h2>Системная информация</h2>");
            builder.AppendLine("    <div class=\"small\">Обзор системы и текущее состояние — данные собраны автоматически.</div>");
            builder.AppendLine("    <div class=\"info-grid\">");
            builder.AppendLine(InfoCard("Система", report.SystemInfo, "ОС / архитектура"));
            builder.AppendLine(InfoCard("Скан выполнен за", report.ScanDuration, "Время выполнения"));
            builder.AppendLine(InfoCard("Имя ПК", report.MachineName, "Environment.MachineName"));
            builder.AppendLine(InfoCard("Виртуальная машина", report.VirtualMachineStatus, "Определение гипервизора"));
            builder.AppendLine(InfoCard("Характеристики", report.HardwareSpec, "CPU / GPU / RAM / Disk"));
            builder.AppendLine(InfoCard("VPN", report.VpnStatus, "Подключён ли VPN"));
            builder.AppendLine(InfoCard("Пользователи в системе", report.UserCount.ToString(CultureInfo.InvariantCulture), "Число локальных учётных записей"));
            builder.AppendLine(InfoCard("Время с последней перезагрузки", report.Uptime, "Uptime"));
            builder.AppendLine("    </div>");

            builder.AppendLine("    <h2 style=\"margin-top:18px;\">Подозрительные процессы</h2>");
            builder.AppendLine("    <table><thead><tr><th>Процесс</th></tr></thead><tbody>");
            foreach (var process in report.CheatProcesses)
            {
                builder.AppendLine($"      <tr><td>{System.Net.WebUtility.HtmlEncode(process)}</td></tr>");
            }
            builder.AppendLine("    </tbody></table>");

            builder.AppendLine("    <h2 style=\"margin-top:14px;\">Подозрительные файлы</h2>");
            builder.AppendLine("    <table><thead><tr><th>Файл</th><th>Размер</th></tr></thead><tbody>");
            foreach (var file in report.SuspiciousFiles)
            {
                builder.AppendLine($"      <tr><td>{System.Net.WebUtility.HtmlEncode(file.Path)}</td><td>{System.Net.WebUtility.HtmlEncode(file.Size)}</td></tr>");
            }
            builder.AppendLine("    </tbody></table>");

            builder.AppendLine("    <h2 style=\"margin-top:14px;\">Подозрительные ключи реестра</h2>");
            builder.AppendLine("    <table><thead><tr><th>Ключ</th></tr></thead><tbody>");
            foreach (var key in report.SuspiciousRegistry)
            {
                builder.AppendLine($"      <tr><td>{System.Net.WebUtility.HtmlEncode(key)}</td></tr>");
            }
            builder.AppendLine("    </tbody></table>");

            builder.AppendLine("    <h2 style=\"margin-top:14px;\">Активные процессы</h2>");
            builder.AppendLine("    <div class=\"small\">Точный список процессов системы — отображается в формате: <strong>Имя | PID | Путь | Память (MB)</strong></div>");
            builder.AppendLine("    <table><thead><tr><th>Имя процесса</th><th>PID</th><th>Путь</th><th>Память (MB)</th></tr></thead><tbody>");
            foreach (var process in report.Processes)
            {
                builder.AppendLine($"      <tr><td>{System.Net.WebUtility.HtmlEncode(process.Name)}</td><td>{process.Id}</td><td>{System.Net.WebUtility.HtmlEncode(process.FilePath)}</td><td>{process.MemoryMb:F2}</td></tr>");
            }
            builder.AppendLine("    </tbody></table>");

            builder.AppendLine("    <h2 style=\"margin-top:14px;\">NVIDIA DRS (stub)</h2>");
            builder.AppendLine("    <div class=\"small\">Таблица-заглушка для конфигурации NVIDIA DRS — данные добавятся позже.</div>");
            builder.AppendLine("    <table><thead><tr><th>Профиль / Параметр</th><th>Значение</th></tr></thead><tbody><tr><td colspan=\"2\" class=\"muted\">— stub: заполняется позже —</td></tr></tbody></table>");

            builder.AppendLine($"    <div class=\"judgement {(report.Judgement.Contains("Clean") ? "clean" : "cheating")}\">{report.Judgement}</div>");
            builder.AppendLine("  </div>");
            builder.AppendLine("  <div class=\"footer\">Отчёт создан сканером CS2 | © 2025</div>");
            builder.AppendLine("</body></html>");
            return builder.ToString();
        }

        private static string InfoCard(string title, string value, string subtitle)
        {
            var valueEncoded = System.Net.WebUtility.HtmlEncode(value);
            var subtitleEncoded = System.Net.WebUtility.HtmlEncode(subtitle);
            return $"      <div class=\"info-card\"><strong>{System.Net.WebUtility.HtmlEncode(title)}</strong><div class=\"info-value\">{valueEncoded}</div><div class=\"muted\">{subtitleEncoded}</div></div>";
        }

        private const string StyleSheet = @"  :root {
      --bg1:#0d0d0d;
      --bg2:#161922;
      --bg3:#1f1a2e;
      --bg4:#1b1f3a;
      --text:#f8f9ff;
      --panel-bg: rgba(17,19,27,0.78);
      --panel-radius:18px;
      --muted: rgba(255,255,255,0.75);
      --table-border: rgba(255,255,255,0.12);
    }
    html,body{
      height:100%;
      margin:0;
      font-family: 'Segoe UI', Roboto, system-ui, -apple-system, 'Helvetica Neue', Arial;
      color:var(--text);
      -webkit-font-smoothing:antialiased;
      -moz-osx-font-smoothing:grayscale;
      background: linear-gradient(135deg,var(--bg1),var(--bg2),var(--bg3),var(--bg4));
      background-size:400% 400%;
      animation: gradientShift 18s ease-in-out infinite;
    }
    @keyframes gradientShift {
      0% { background-position: 0% 50%; }
      50% { background-position: 100% 50%; }
      100% { background-position: 0% 50%; }
    }

    h1 { text-align:center; margin:18px 0 8px; text-shadow:0 2px 8px rgba(0,0,0,0.25); }
    .container {
      width:calc(100% - 40px);
      max-width:900px;
      margin:20px auto;
      background:var(--panel-bg);
      border-radius:var(--panel-radius);
      padding:20px;
      box-shadow: 0 8px 30px rgba(0,0,0,0.35);
      backdrop-filter: blur(6px);
    }

    .row { display:flex; gap:18px; flex-wrap:wrap; }
    .col { flex:1 1 240px; min-width:200px; }
    .small { font-size:0.86rem; color:var(--muted); margin:4px 0 10px; }

    .info-grid { display:grid; grid-template-columns: 1fr 1fr; gap:12px; margin-top:8px; }
    .info-card {
      background: rgba(255,255,255,0.03);
      padding:10px;
      border-radius:10px;
      border:1px solid rgba(255,255,255,0.03);
    }
    .info-card strong { display:block; margin-bottom:6px; font-weight:600; }
    .info-value { font-size:0.95rem; color: #fff; }

    table {
      width:100%;
      border-collapse:collapse;
      margin-top:12px;
      color:var(--text);
      font-size:0.92rem;
      background: transparent;
    }
    th, td {
      padding:10px 8px;
      border-bottom:1px solid var(--table-border);
      text-align:left;
    }
    th {
      background: rgba(255,255,255,0.04);
      font-weight:600;
    }
    .muted { color:var(--muted); font-size:0.85rem; }

    .judgement {
      font-size:1.1rem;
      text-align:center;
      margin-top:12px;
      font-weight:700;
    }
    .clean { color:#8aff8a; }
    .cheating { color:#ff6b6b; }

    .footer { text-align:center; margin-top:18px; font-size:0.88rem; opacity:0.85; }

    @media (max-width:640px){
      .info-grid { grid-template-columns:1fr; }
      .row { gap:12px; }
    }";
    }
}
