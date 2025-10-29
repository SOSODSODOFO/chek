using System;
using System.Linq;
using System.Net;
using System.Text;

namespace ChekScanner;

public sealed class HtmlReportBuilder
{
    public string BuildReport(ScanResult result)
    {
        var reportData = new ReportData(result);
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"ru\">");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"UTF-8\" />");
        sb.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"/>");
        sb.AppendLine("  <title>CS2 Scan Report</title>");
        sb.AppendLine("  <style>");
        sb.Append(reportData.Styles);
        sb.AppendLine("  </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("  <h1>CS2 Scan Report</h1>");
        sb.AppendLine("  <div class=\"container\">");
        sb.AppendLine("    <h2>Системная информация</h2>");
        sb.AppendLine("    <div class=\"small\">Обзор системы и текущее состояние — данные сформированы автоматически.</div>");

        sb.AppendLine("    <div class=\"info-grid\">");
        sb.AppendLine(reportData.SystemCards);
        sb.AppendLine("    </div>");

        sb.AppendLine("    <h2 style=\"margin-top:18px;\">Подозрительные процессы</h2>");
        sb.AppendLine(reportData.SuspiciousProcessesTable);

        sb.AppendLine("    <h2 style=\"margin-top:14px;\">Подозрительные файлы</h2>");
        sb.AppendLine(reportData.SuspiciousFilesTable);

        sb.AppendLine("    <h2 style=\"margin-top:14px;\">Подозрительные ключи реестра</h2>");
        sb.AppendLine(reportData.SuspiciousRegistryTable);

        sb.AppendLine("    <h2 style=\"margin-top:14px;\">Активные процессы</h2>");
        sb.AppendLine("    <div class=\"small\">Точный список процессов системы — отображается в формате: <strong>Имя | PID | Путь | Память (MB)</strong></div>");
        sb.AppendLine(reportData.AllProcessesTable);

        sb.AppendLine("    <h2 style=\"margin-top:14px;\">NVIDIA DRS (данные)</h2>");
        sb.AppendLine(reportData.NvidiaDrsTable);

        sb.AppendLine($"    <div class=\"judgement {reportData.JudgementClass}\">{WebUtility.HtmlEncode(result.Judgement)}</div>");
        sb.AppendLine("  </div>");
        sb.AppendLine("  <div class=\"footer\">Отчёт создан сканером CS2 | © 2025</div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private sealed record ReportData
    {
        private readonly ScanResult _result;
        public string Styles { get; }
        public string SystemCards { get; }
        public string SuspiciousProcessesTable { get; }
        public string SuspiciousFilesTable { get; }
        public string SuspiciousRegistryTable { get; }
        public string AllProcessesTable { get; }
        public string NvidiaDrsTable { get; }
        public string JudgementClass { get; }

        public ReportData(ScanResult result)
        {
            _result = result;
            Styles = BuildStyles();
            SystemCards = BuildSystemCards();
            SuspiciousProcessesTable = BuildSuspiciousProcessesTable();
            SuspiciousFilesTable = BuildSuspiciousFilesTable();
            SuspiciousRegistryTable = BuildSuspiciousRegistryTable();
            AllProcessesTable = BuildAllProcessesTable();
            NvidiaDrsTable = BuildNvidiaDrsTable();
            JudgementClass = result.Judgement.Contains("Clean", StringComparison.OrdinalIgnoreCase) ? "clean" : "cheating";
        }

        private static string BuildStyles() => @"
    :root{
      --bg1:#ff758c; --bg2:#ffa647; --bg3:#ff7eb3; --bg4:#ffd36b;
      --panel-bg: rgba(0,0,0,0.45);
      --panel-radius: 14px;
      --text:#ffffff;
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
    }
  ";

        private string BuildSystemCards()
        {
            var cards = new (string Title, string Value, string Hint)[]
            {
                ("Система", Html(_result.SystemInfo), "ОС / архитектура"),
                ("Скан выполнен за", Html(FormatDuration(_result.ScanDuration)), "Время выполнения"),
                ("Имя ПК", Html(_result.MachineName), "Environment.MachineName"),
                ("Виртуальная машина", Html(_result.VmStatus), "Определение гипервизора"),
                ("Характеристики", Html(_result.HardwareSummary), "CPU / GPU / RAM / Disk"),
                ("VPN", Html(_result.VpnStatus), "Подключён ли VPN"),
                ("Пользователи в системе", Html(_result.UserCount.ToString()), "Число локальных учётных записей"),
                ("Время с последней перезагрузки", Html(_result.BootTime), "Uptime"),
            };

            var sb = new StringBuilder();
            foreach (var card in cards)
            {
                sb.AppendLine("      <div class=\"info-card\">");
                sb.AppendLine($"        <strong>{card.Title}</strong>");
                sb.AppendLine($"        <div class=\"info-value\">{card.Value}</div>");
                sb.AppendLine($"        <div class=\"muted\">{card.Hint}</div>");
                sb.AppendLine("      </div>");
            }

            return sb.ToString();
        }

        private string BuildSuspiciousProcessesTable()
        {
            if (_result.SuspiciousProcesses.Count == 0)
            {
                return "    <table id=\"processTable\"><thead><tr><th>Процесс</th></tr></thead><tbody><tr><td>Подозрительные процессы не обнаружены</td></tr></tbody></table>";
            }

            var rows = string.Join(Environment.NewLine, _result.SuspiciousProcesses
                .Select(p => $"<tr><td>{Html($"{p.Name} | {p.Pid} | {p.Path}")}</td></tr>"));

            return $"    <table id=\"processTable\"><thead><tr><th>Процесс</th></tr></thead><tbody>{rows}</tbody></table>";
        }

        private string BuildSuspiciousFilesTable()
        {
            if (_result.SuspiciousFiles.Count == 0)
            {
                return "    <table id=\"fileTable\"><thead><tr><th>Файл</th><th>Размер</th></tr></thead><tbody><tr><td colspan=\"2\">Подозрительных файлов не найдено</td></tr></tbody></table>";
            }

            var sb = new StringBuilder();
            foreach (var file in _result.SuspiciousFiles)
            {
                sb.AppendLine($"<tr><td>{Html(file.Name)}</td><td>{Html(file.Size)}</td></tr>");
            }

            return $"    <table id=\"fileTable\"><thead><tr><th>Файл</th><th>Размер</th></tr></thead><tbody>{sb}</tbody></table>";
        }

        private string BuildSuspiciousRegistryTable()
        {
            if (_result.SuspiciousRegistry.Count == 0)
            {
                return "    <table id=\"registryTable\"><thead><tr><th>Ключ</th></tr></thead><tbody><tr><td>Подозрительных ключей реестра не обнаружено</td></tr></tbody></table>";
            }

            var rows = string.Join(Environment.NewLine, _result.SuspiciousRegistry
                .Select(entry => $"<tr><td>{Html(entry)}</td></tr>"));

            return $"    <table id=\"registryTable\"><thead><tr><th>Ключ</th></tr></thead><tbody>{rows}</tbody></table>";
        }

        private string BuildAllProcessesTable()
        {
            if (_result.Processes.Count == 0)
            {
                return "    <table id=\"allProcessesTable\"><thead><tr><th>Имя процесса</th><th>PID</th><th>Путь</th><th>Память (MB)</th></tr></thead><tbody><tr><td colspan=\"4\">Нет данных</td></tr></tbody></table>";
            }

            var sb = new StringBuilder();
            foreach (var process in _result.Processes)
            {
                sb.AppendLine($"<tr><td>{Html(process.Name)}</td><td>{process.Pid}</td><td>{Html(process.Path)}</td><td>{process.MemoryMb:0.##}</td></tr>");
            }

            return $"    <table id=\"allProcessesTable\"><thead><tr><th>Имя процесса</th><th>PID</th><th>Путь</th><th>Память (MB)</th></tr></thead><tbody>{sb}</tbody></table>";
        }

        private string BuildNvidiaDrsTable()
        {
            if (_result.NvidiaDrsEntries.Count == 0)
            {
                return "    <table id=\"nvidiaDrsTable\"><thead><tr><th>Профиль / Параметр</th><th>Значение</th></tr></thead><tbody><tr><td colspan=\"2\">Нет данных в nvAppTimestamps</td></tr></tbody></table>";
            }

            var sb = new StringBuilder();
            foreach (var entry in _result.NvidiaDrsEntries)
            {
                sb.AppendLine($"<tr><td>{Html(entry.Key)}</td><td>{Html(entry.Value)}</td></tr>");
            }

            return $"    <table id=\"nvidiaDrsTable\"><thead><tr><th>Профиль / Параметр</th><th>Значение</th></tr></thead><tbody>{sb}</tbody></table>";
        }

        private static string Html(string value) => WebUtility.HtmlEncode(value);

        private static string FormatDuration(TimeSpan span)
        {
            if (span.TotalMilliseconds <= 0)
            {
                return "—";
            }

            if (span.TotalHours >= 1)
            {
                return $"{(int)span.TotalHours}h {span.Minutes}m {span.Seconds}s";
            }

            return $"{span.Minutes}m {span.Seconds}s";
        }
    }
}
