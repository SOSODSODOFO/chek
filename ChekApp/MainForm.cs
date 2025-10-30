using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChekApp
{
    public sealed class MainForm : Form
    {
        private const string WebhookUrl = "https://discord.com/api/webhooks/1429778047363452999/W_5Yso3yyYgIOQdRRtbQE4_Zfr4J656wTL2ell9gQGrUeiEC-TTCIhqPPRzoheVRv6E4";
        private readonly HttpClient _httpClient = new();
        private readonly Random _random = new();
        private readonly Panel _introPanel = new();
        private readonly Label _introTitle = new();
        private readonly Label _introSubtitle = new();
        private readonly TextBox _codeInput = new();
        private readonly Button _submitCodeButton = new();
        private readonly Button _startButton = new();
        private readonly WebBrowser _reportBrowser = new();
        private readonly Panel _reportPanel = new();
        private string _verificationCode = string.Empty;
        private Timer? _dissolveTimer;

        public MainForm()
        {
            SuspendLayout();
            AutoScaleMode = AutoScaleMode.None;
            DoubleBuffered = true;
            Text = "CS2 Scan";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            ClientSize = new Size(400, 400);
            BackColor = Color.Black;
            StartPosition = FormStartPosition.CenterScreen;

            var gradientPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black
            };
            gradientPanel.Paint += DrawGradientBackground;
            Controls.Add(gradientPanel);

            _introPanel.Dock = DockStyle.Fill;
            _introPanel.BackColor = Color.FromArgb(12, 12, 12);
            _introPanel.Padding = new Padding(24, 32, 24, 24);

            _introTitle.ForeColor = Color.White;
            _introTitle.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
            _introTitle.TextAlign = ContentAlignment.MiddleCenter;
            _introTitle.AutoSize = false;
            _introTitle.Dock = DockStyle.Top;
            _introTitle.Height = 60;
            _introTitle.Text = "Сейчас вам администратор скажет код";

            _introSubtitle.ForeColor = Color.FromArgb(180, 180, 180);
            _introSubtitle.Font = new Font("Segoe UI", 10f, FontStyle.Regular);
            _introSubtitle.TextAlign = ContentAlignment.MiddleCenter;
            _introSubtitle.AutoSize = false;
            _introSubtitle.Dock = DockStyle.Top;
            _introSubtitle.Height = 40;
            _introSubtitle.Margin = new Padding(0, 12, 0, 24);
            _introSubtitle.Text = "Введите его в данную строку";

            _codeInput.Font = new Font("Consolas", 16f, FontStyle.Bold);
            _codeInput.ForeColor = Color.White;
            _codeInput.BackColor = Color.FromArgb(32, 32, 32);
            _codeInput.BorderStyle = BorderStyle.FixedSingle;
            _codeInput.MaxLength = 6;
            _codeInput.CharacterCasing = CharacterCasing.Upper;
            _codeInput.TextAlign = HorizontalAlignment.Center;
            _codeInput.Dock = DockStyle.Top;
            _codeInput.Height = 50;
            _codeInput.Margin = new Padding(0, 24, 0, 12);
            _codeInput.KeyDown += CodeInputOnKeyDown;

            _submitCodeButton.Text = "Подтвердить";
            _submitCodeButton.Dock = DockStyle.Top;
            _submitCodeButton.Height = 44;
            _submitCodeButton.FlatStyle = FlatStyle.Flat;
            _submitCodeButton.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 60);
            _submitCodeButton.FlatAppearance.BorderSize = 1;
            _submitCodeButton.BackColor = Color.FromArgb(50, 50, 50);
            _submitCodeButton.ForeColor = Color.White;
            _submitCodeButton.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            _submitCodeButton.Margin = new Padding(0, 18, 0, 0);
            _submitCodeButton.Click += SubmitCodeButtonOnClick;

            _startButton.Text = "Start";
            _startButton.Dock = DockStyle.Bottom;
            _startButton.Height = 52;
            _startButton.Visible = false;
            _startButton.BackColor = Color.FromArgb(0, 122, 204);
            _startButton.ForeColor = Color.White;
            _startButton.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            _startButton.FlatStyle = FlatStyle.Flat;
            _startButton.FlatAppearance.BorderSize = 0;
            _startButton.Margin = new Padding(0, 24, 0, 0);
            _startButton.Click += StartButtonOnClick;

            _introPanel.Controls.Add(_startButton);
            _introPanel.Controls.Add(_submitCodeButton);
            _introPanel.Controls.Add(_codeInput);
            _introPanel.Controls.Add(_introSubtitle);
            _introPanel.Controls.Add(_introTitle);
            gradientPanel.Controls.Add(_introPanel);

            _reportPanel.Dock = DockStyle.Fill;
            _reportPanel.Visible = false;
            _reportPanel.Padding = new Padding(0);
            _reportPanel.BackColor = Color.Transparent;

            _reportBrowser.Dock = DockStyle.Fill;
            _reportBrowser.AllowWebBrowserDrop = false;
            _reportBrowser.IsWebBrowserContextMenuEnabled = false;
            _reportBrowser.WebBrowserShortcutsEnabled = false;
            _reportBrowser.ScriptErrorsSuppressed = true;
            _reportPanel.Controls.Add(_reportBrowser);
            gradientPanel.Controls.Add(_reportPanel);

            Shown += async (_, _) => await InitializeAsync();

            ResumeLayout(false);
        }

        private void DrawGradientBackground(object? sender, PaintEventArgs e)
        {
            var rect = ClientRectangle;
            using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(rect, Color.FromArgb(15, 15, 15), Color.FromArgb(40, 40, 40), 135f);
            e.Graphics.FillRectangle(brush, rect);
        }

        private async Task InitializeAsync()
        {
            _verificationCode = GenerateCode();
            try
            {
                await SendCodeToWebhookAsync(_verificationCode);
            }
            catch
            {
                // Ignore webhook errors but could log them if needed.
            }
        }

        private string GenerateCode()
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var sb = new StringBuilder(6);
            for (var i = 0; i < 6; i++)
            {
                var index = _random.Next(alphabet.Length);
                sb.Append(alphabet[index]);
            }

            return sb.ToString();
        }

        private async Task SendCodeToWebhookAsync(string code)
        {
            var payload = new { content = $"Новый код проверки: `{code}`" };
            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            await _httpClient.PostAsync(WebhookUrl, content);
        }

        private void CodeInputOnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ValidateCode();
            }
        }

        private void SubmitCodeButtonOnClick(object? sender, EventArgs e)
        {
            ValidateCode();
        }

        private void ValidateCode()
        {
            var input = _codeInput.Text.Trim().ToUpperInvariant();
            if (input.Length != 6)
            {
                ShakeControl(_codeInput);
                return;
            }

            if (!string.Equals(input, _verificationCode, StringComparison.OrdinalIgnoreCase))
            {
                ShakeControl(_codeInput);
                _codeInput.SelectAll();
                return;
            }

            BeginDissolve();
        }

        private void BeginDissolve()
        {
            _codeInput.Enabled = false;
            _submitCodeButton.Enabled = false;

            _dissolveTimer = new Timer { Interval = 15 };
            var step = 0;
            var startHeight = _introPanel.Height;
            _dissolveTimer.Tick += (_, _) =>
            {
                step++;
                var progress = Math.Min(1f, step / 60f);
                var eased = EaseInOut(progress);
                _introPanel.Padding = new Padding(24, (int)(32 + 60 * eased), 24, 24);
                _codeInput.BackColor = Color.FromArgb((int)(32 + 80 * eased), (int)(32 + 80 * eased), (int)(32 + 80 * eased));
                _codeInput.ForeColor = Color.FromArgb((int)(255 - 120 * eased), (int)(255 - 120 * eased), (int)(255 - 120 * eased));
                _introTitle.ForeColor = Color.FromArgb((int)(255 - 180 * eased), (int)(255 - 180 * eased), (int)(255 - 180 * eased));
                _introSubtitle.ForeColor = Color.FromArgb((int)(180 - 160 * eased), (int)(180 - 160 * eased), (int)(180 - 160 * eased));
                _introPanel.BackColor = Color.FromArgb((int)(12 + 40 * eased), (int)(12 + 40 * eased), (int)(12 + 40 * eased));

                if (Math.Abs(progress - 1f) < 0.0001f)
                {
                    _dissolveTimer!.Stop();
                    _introTitle.Visible = false;
                    _introSubtitle.Visible = false;
                    _codeInput.Visible = false;
                    _submitCodeButton.Visible = false;
                    _introPanel.Padding = new Padding(24);
                    _startButton.Visible = true;
                    _startButton.Focus();
                }
            };
            _dissolveTimer.Start();
        }

        private static float EaseInOut(float value)
        {
            return value < 0.5f
                ? 4f * value * value * value
                : 1f - MathF.Pow(-2f * value + 2f, 3f) / 2f;
        }

        private void ShakeControl(Control control)
        {
            var originalLocation = control.Location;
            var shakeTimer = new Timer { Interval = 15 };
            var iteration = 0;
            shakeTimer.Tick += (_, _) =>
            {
                iteration++;
                var offset = (int)(Math.Sin(iteration * 0.6) * 6);
                control.Location = new Point(originalLocation.X + offset, originalLocation.Y);
                if (iteration > 20)
                {
                    shakeTimer.Stop();
                    control.Location = originalLocation;
                }
            };
            shakeTimer.Start();
        }

        private async void StartButtonOnClick(object? sender, EventArgs e)
        {
            _startButton.Enabled = false;
            _startButton.Text = "Scanning...";

            var report = await Task.Run(() => SystemScanner.GenerateReport());

            _reportBrowser.DocumentText = ReportHtmlBuilder.Build(report);
            _reportPanel.Visible = true;
            _introPanel.Visible = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _httpClient.Dispose();
                _dissolveTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
