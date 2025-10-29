using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace CS2Scanner
{
    public partial class MainForm : Form
    {
        private const string WebhookUrl = "https://discord.com/api/webhooks/1429778047363452999/W_5Yso3yyYgIOQdRRtbQE4_Zfr4J656wTL2ell9gQGrUeiEC-TTCIhqPPRzoheVRv6E4";

        private readonly Random _random = new();
        private string _pendingCode = string.Empty;
        private bool _codeVerified;
        private Timer? _dissolveTimer;
        private Timer? _startButtonTimer;
        private int _dissolveAlpha;
        private int _startButtonTicks;
        private bool _scanInProgress;
        private readonly Size _startButtonTargetSize = new(220, 64);

        public MainForm()
        {
            InitializeComponent();
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (ClientRectangle.Width <= 0 || ClientRectangle.Height <= 0)
            {
                base.OnPaintBackground(e);
                return;
            }

            using var brush = new LinearGradientBrush(ClientRectangle, Color.FromArgb(8, 8, 12), Color.FromArgb(32, 32, 36), 135f);
            e.Graphics.FillRectangle(brush, ClientRectangle);
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            reportPanel.Visible = false;
            reportBrowser.DocumentText = string.Empty;
            statusLabel.Text = "Готовимся к сканированию...";

            startScanButton.Visible = false;
            startScanButton.Enabled = false;
            startScanButton.Text = string.Empty;
            startScanButton.Margin = new Padding(0, 24, 0, 0);

            introTitleLabel.Text = "Сейчас вам администратор скажет код";
            codeStatusLabel.ForeColor = Color.FromArgb(200, 200, 210);
            introSubtitleLabel.Text = "Введите его в данную строку ниже.";

            _pendingCode = GenerateAccessCode();
            codeTextBox.Clear();
            codeTextBox.Focus();

            codeStatusLabel.Text = "Отправляем код администратору...";
            try
            {
                await DiscordWebhookNotifier.SendAccessCodeAsync(WebhookUrl, _pendingCode);
                codeStatusLabel.Text = "Код отправлен. Введите его, чтобы продолжить.";
            }
            catch (Exception ex)
            {
                codeStatusLabel.ForeColor = Color.FromArgb(230, 120, 120);
                codeStatusLabel.Text = "Не удалось отправить код. Проверьте подключение и попробуйте снова.";
                MessageBox.Show(this, ex.Message, "Ошибка отправки", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task StartScanAsync()
        {
            if (!_codeVerified)
            {
                if (introPanel.Visible)
                {
                    codeStatusLabel.ForeColor = Color.FromArgb(230, 120, 120);
                    codeStatusLabel.Text = "Сначала подтвердите код, отправленный администратором.";
                    codeTextBox.Focus();
                    codeTextBox.SelectAll();
                }
                else
                {
                    statusLabel.Text = "Доступ не подтверждён. Вернитесь на предыдущий экран и введите код.";
                }

                return;
            }

            if (_scanInProgress)
            {
                return;
            }

            _scanInProgress = true;
            startScanButton.Enabled = false;
            startScanButton.Text = "Сканирование...";

            introPanel.Visible = false;
            reportPanel.Visible = true;
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
                statusLabel.Text = "Сканирование завершено. Отчёт готов.";

                await DiscordWebhookNotifier.SendReportAsync(WebhookUrl, report, html);
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Ошибка: " + ex.Message;
                MessageBox.Show(this, ex.ToString(), "Ошибка сканера", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _scanInProgress = false;
                startScanButton.Enabled = true;
                startScanButton.Text = "Start";
            }
        }

        private void VerifyAccessCode()
        {
            if (_codeVerified)
            {
                return;
            }

            string input = codeTextBox.Text.Trim().ToUpperInvariant();
            if (input.Length != 6)
            {
                codeStatusLabel.ForeColor = Color.FromArgb(230, 120, 120);
                codeStatusLabel.Text = "Код должен содержать 6 символов.";
                codeTextBox.Focus();
                codeTextBox.SelectAll();
                return;
            }

            if (!string.Equals(input, _pendingCode, StringComparison.OrdinalIgnoreCase))
            {
                codeStatusLabel.ForeColor = Color.FromArgb(230, 120, 120);
                codeStatusLabel.Text = "Код не совпадает. Попробуйте ещё раз.";
                codeTextBox.Focus();
                codeTextBox.SelectAll();
                return;
            }

            _codeVerified = true;
            codeStatusLabel.ForeColor = Color.FromArgb(120, 220, 180);
            codeStatusLabel.Text = "Код принят. Подготовка интерфейса...";

            codeTextBox.Enabled = false;
            verifyButton.Enabled = false;

            StartDissolveAnimation();
        }

        private void StartDissolveAnimation()
        {
            _dissolveAlpha = 180;

            codeTextBox.Visible = false;
            verifyButton.Visible = false;

            _dissolveTimer?.Stop();
            _dissolveTimer?.Dispose();

            _dissolveTimer = new Timer { Interval = 16 };
            _dissolveTimer.Tick += DissolveTimerOnTick;
            _dissolveTimer.Start();
        }

        private void DissolveTimerOnTick(object? sender, EventArgs e)
        {
            _dissolveAlpha = Math.Max(0, _dissolveAlpha - 12);
            codeInputPanel.BackColor = Color.FromArgb(_dissolveAlpha, 22, 22, 22);

            int targetHeight = 36;
            if (codeInputPanel.Height > targetHeight)
            {
                codeInputPanel.Height = Math.Max(targetHeight, codeInputPanel.Height - 10);
                codeInputPanel.Padding = new Padding(
                    Math.Min(codeInputPanel.Padding.Left + 2, 60),
                    Math.Min(codeInputPanel.Padding.Top + 1, 40),
                    Math.Min(codeInputPanel.Padding.Right + 2, 60),
                    Math.Min(codeInputPanel.Padding.Bottom + 1, 40));
            }

            if (_dissolveAlpha <= 20)
            {
                _dissolveTimer?.Stop();
                if (_dissolveTimer != null)
                {
                    _dissolveTimer.Tick -= DissolveTimerOnTick;
                    _dissolveTimer.Dispose();
                    _dissolveTimer = null;
                }

                codeInputPanel.Visible = false;
                introTitleLabel.Text = "Доступ подтверждён";
                introSubtitleLabel.Text = "Запустите сканирование, когда будете готовы.";
                RevealStartButton();
            }
        }

        private void RevealStartButton()
        {
            startScanButton.Visible = true;
            startScanButton.Enabled = false;
            startScanButton.Text = string.Empty;
            startScanButton.Size = new Size(4, 4);

            codeStatusLabel.ForeColor = Color.FromArgb(200, 200, 210);
            codeStatusLabel.Text = "Нажмите кнопку Start, чтобы начать проверку.";

            _startButtonTicks = 0;
            _startButtonTimer?.Stop();
            _startButtonTimer?.Dispose();

            _startButtonTimer = new Timer { Interval = 16 };
            _startButtonTimer.Tick += StartButtonTimerOnTick;
            _startButtonTimer.Start();
        }

        private void StartButtonTimerOnTick(object? sender, EventArgs e)
        {
            _startButtonTicks++;
            double progress = Math.Min(1.0, _startButtonTicks / 30.0);
            int width = Math.Max(4, (int)(_startButtonTargetSize.Width * progress));
            int height = Math.Max(4, (int)(_startButtonTargetSize.Height * progress));
            startScanButton.Size = new Size(width, height);

            if (progress >= 1.0)
            {
                _startButtonTimer?.Stop();
                if (_startButtonTimer != null)
                {
                    _startButtonTimer.Tick -= StartButtonTimerOnTick;
                    _startButtonTimer.Dispose();
                    _startButtonTimer = null;
                }

                startScanButton.Text = "Start";
                startScanButton.Enabled = true;
            }
            else if (progress > 0.6)
            {
                startScanButton.Text = "Start";
            }
        }

        private string GenerateAccessCode()
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            Span<char> buffer = stackalloc char[6];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = alphabet[_random.Next(alphabet.Length)];
            }

            return new string(buffer);
        }

        private static string FormatElapsed(TimeSpan elapsed)
        {
            if (elapsed.TotalSeconds < 60)
            {
                return $"{elapsed.TotalSeconds:F1}s";
            }

            return $"{(int)elapsed.TotalMinutes}m {elapsed.Seconds}s";
        }

        private void verifyButton_Click(object sender, EventArgs e)
        {
            VerifyAccessCode();
        }

        private async void startScanButton_Click(object sender, EventArgs e)
        {
            await StartScanAsync();
        }

        private void codeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                VerifyAccessCode();
            }
        }

        private void codeTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_codeVerified)
            {
                return;
            }

            if (codeStatusLabel.ForeColor == Color.FromArgb(230, 120, 120))
            {
                codeStatusLabel.ForeColor = Color.FromArgb(200, 200, 210);
                codeStatusLabel.Text = "После ввода нажмите \"Проверить\".";
            }
        }
    }
}
