using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChekScanner;

public sealed class MainForm : Form
{
    private const string WebhookUrl = "https://discord.com/api/webhooks/1429778047363452999/W_5Yso3yyYgIOQdRRtbQE4_Zfr4J656wTL2ell9gQGrUeiEC-TTCIhqPPRzoheVRv6E4";

    private readonly DiscordWebhookClient _webhookClient = new(WebhookUrl);
    private readonly SystemScanner _scanner = new();

    private readonly Panel _overlayPanel;
    private readonly Label _headlineLabel;
    private readonly Label _subLabel;
    private readonly TextBox _codeInput;
    private readonly Button _confirmButton;
    private readonly Label _overlayStatusLabel;
    private readonly Button _startButton;
    private readonly Label _statusLabel;
    private readonly Panel _contentPanel;
    private readonly Timer _overlayFadeTimer;

    private readonly Random _random = new();
    private string _sessionCode = string.Empty;
    private int _overlayAlpha = 220;
    private bool _hasCodeBeenSent;
    private float _gradientOffset;
    private readonly Timer _gradientTimer;

    public MainForm()
    {
        Text = "CS2 Deep Scan";
        ClientSize = new Size(800, 800);
        MinimumSize = Size;
        MaximumSize = Size;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Color.Black;
        DoubleBuffered = true;

        _gradientTimer = new Timer { Interval = 80 };
        _gradientTimer.Tick += (_, _) =>
        {
            _gradientOffset += 0.01f;
            if (_gradientOffset > 1f)
            {
                _gradientOffset = 0f;
            }
            Invalidate(new Rectangle(Point.Empty, ClientSize));
        };

        _contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(28),
            BackColor = Color.FromArgb(40, 40, 40)
        };

        Controls.Add(_contentPanel);

        var brandingLabel = new Label
        {
            Text = "CS2 Multi-Layer Integrity Scan",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 22f, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(40, 40)
        };
        _contentPanel.Controls.Add(brandingLabel);

        _statusLabel = new Label
        {
            Text = "Ожидание запуска",
            ForeColor = Color.FromArgb(180, 180, 180),
            Font = new Font("Segoe UI", 10f, FontStyle.Regular),
            AutoSize = true,
            Location = new Point(42, 100)
        };
        _contentPanel.Controls.Add(_statusLabel);

        _startButton = new Button
        {
            Text = "Start",
            Visible = false,
            Enabled = false,
            Size = new Size(160, 48),
            Location = new Point(40, 140),
            BackColor = Color.FromArgb(30, 30, 30),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 12f, FontStyle.Bold)
        };
        _startButton.FlatAppearance.BorderSize = 0;
        _startButton.Click += async (_, _) => await BeginScanAsync();
        _contentPanel.Controls.Add(_startButton);

        _overlayPanel = new Panel
        {
            Size = new Size(620, 240),
            Location = new Point((ClientSize.Width - 620) / 2, (ClientSize.Height - 240) / 2),
            BackColor = Color.FromArgb(_overlayAlpha, 20, 20, 20)
        };
        _overlayPanel.Anchor = AnchorStyles.None;
        Controls.Add(_overlayPanel);
        _overlayPanel.BringToFront();

        _headlineLabel = new Label
        {
            Text = "Сейчас администратор сообщит вам код",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 16f, FontStyle.Bold),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 70
        };
        _overlayPanel.Controls.Add(_headlineLabel);

        _subLabel = new Label
        {
            Text = "Введите его в строку ниже, чтобы открыть инструменты диагностики",
            ForeColor = Color.FromArgb(200, 200, 200),
            Font = new Font("Segoe UI", 11f, FontStyle.Regular),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 40
        };
        _overlayPanel.Controls.Add(_subLabel);

        _codeInput = new TextBox
        {
            MaxLength = 6,
            Font = new Font("Consolas", 20f, FontStyle.Bold),
            TextAlign = HorizontalAlignment.Center,
            Width = 260,
            Location = new Point((_overlayPanel.Width - 260) / 2, 120),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(28, 28, 28),
            ForeColor = Color.White
        };
        _overlayPanel.Controls.Add(_codeInput);

        _confirmButton = new Button
        {
            Text = "Подтвердить",
            Width = 180,
            Height = 44,
            Location = new Point((_overlayPanel.Width - 180) / 2, 170),
            BackColor = Color.FromArgb(50, 50, 50),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 11f, FontStyle.Bold)
        };
        _confirmButton.FlatAppearance.BorderSize = 0;
        _confirmButton.Click += ConfirmButtonOnClick;
        _overlayPanel.Controls.Add(_confirmButton);

        _overlayStatusLabel = new Label
        {
            Text = "Код генерируется...",
            ForeColor = Color.FromArgb(180, 100, 255),
            Font = new Font("Segoe UI", 10f, FontStyle.Italic),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Bottom,
            Height = 30
        };
        _overlayPanel.Controls.Add(_overlayStatusLabel);

        _overlayFadeTimer = new Timer { Interval = 30 };
        _overlayFadeTimer.Tick += (_, _) =>
        {
            _overlayAlpha -= 15;
            if (_overlayAlpha <= 0)
            {
                _overlayFadeTimer.Stop();
                _overlayPanel.Visible = false;
                _startButton.Visible = true;
                _startButton.Enabled = true;
                _statusLabel.Text = "Подготовка завершена. Готовы запустить проверку.";
                return;
            }

            _overlayPanel.BackColor = Color.FromArgb(Math.Max(_overlayAlpha, 0), 20, 20, 20);
            _overlayPanel.Refresh();
        };

        Shown += async (_, _) => await PrepareAsync();
        Resize += (_, _) => CenterOverlay();

        _gradientTimer.Start();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        var rect = new Rectangle(Point.Empty, ClientSize);
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            base.OnPaintBackground(e);
            return;
        }

        using var path = new GraphicsPath();
        path.AddRectangle(rect);
        using var brush = new System.Drawing.Drawing2D.PathGradientBrush(path)
        {
            CenterColor = Color.FromArgb(80, 70, 20, 90),
            SurroundColors = new[]
            {
                Color.FromArgb(255, 5, 5, 10),
                Color.FromArgb(255, 15, 15, 25),
                Color.FromArgb(255, 5, 5, 10)
            },
            CenterPoint = new PointF(rect.Width * (0.3f + 0.4f * _gradientOffset), rect.Height * (0.3f + 0.4f * (1f - _gradientOffset)))
        };

        e.Graphics.FillRectangle(new SolidBrush(Color.Black), rect);
        e.Graphics.FillRectangle(brush, rect);
    }

    private async Task PrepareAsync()
    {
        if (!OperatingSystem.IsWindows())
        {
            MessageBox.Show("Данное приложение предназначено только для Windows.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
            return;
        }

        if (_hasCodeBeenSent)
        {
            return;
        }

        _sessionCode = GenerateCode();
        _overlayStatusLabel.Text = "Передаём код оператору...";

        try
        {
            await _webhookClient.SendMessageAsync($"🛡️ Новый запрос на проверку. Код доступа: `{_sessionCode}`");
            _overlayStatusLabel.Text = "Код отправлен. Дождитесь сообщения администратора.";
            _hasCodeBeenSent = true;
        }
        catch (Exception ex)
        {
            _overlayStatusLabel.Text = "Не удалось отправить код. Попробуйте ещё раз.";
            Debug.WriteLine(ex);
        }
    }

    private void ConfirmButtonOnClick(object? sender, EventArgs e)
    {
        var input = _codeInput.Text.Trim().ToUpperInvariant();
        if (string.Equals(input, _sessionCode, StringComparison.OrdinalIgnoreCase))
        {
            _overlayStatusLabel.Text = "Код принят. Открываем панель управления...";
            _confirmButton.Enabled = false;
            _codeInput.Enabled = false;
            _overlayFadeTimer.Start();
        }
        else
        {
            _overlayStatusLabel.Text = "Неверный код. Проверьте сообщение администратора.";
            _overlayStatusLabel.ForeColor = Color.FromArgb(255, 180, 80, 80);
        }
    }

    private async Task BeginScanAsync()
    {
        _startButton.Enabled = false;
        _statusLabel.Text = "Сканирование компьютера...";

        try
        {
            var sw = Stopwatch.StartNew();
            var result = await Task.Run(() => _scanner.PerformScan());
            sw.Stop();
            result.ScanDuration = sw.Elapsed;

            _statusLabel.Text = "Формируем отчёт...";
            var builder = new HtmlReportBuilder();
            var html = builder.BuildReport(result);

            var exportDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CS2_ScanReports");
            Directory.CreateDirectory(exportDirectory);
            var fileName = $"CS2_Report_{DateTime.Now:yyyyMMdd_HHmmss}.html";
            var filePath = Path.Combine(exportDirectory, fileName);
            File.WriteAllText(filePath, html);

            _statusLabel.Text = "Отправка результатов оператору...";
            await _webhookClient.SendReportAsync("Готов новый отчёт сканирования.", fileName, System.Text.Encoding.UTF8.GetBytes(html));

            _statusLabel.Text = $"Сканирование завершено. Отчёт сохранён: {filePath}";
            MessageBox.Show($"Готово! Отчёт сохранён по адресу:\n{filePath}", "Сканирование завершено", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Ошибка во время сканирования. Подробности в логе.";
            Debug.WriteLine(ex);
            MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _startButton.Enabled = true;
        }
    }

    private void CenterOverlay()
    {
        if (_overlayPanel.Visible)
        {
            _overlayPanel.Location = new Point((ClientSize.Width - _overlayPanel.Width) / 2, (ClientSize.Height - _overlayPanel.Height) / 2);
        }
    }

    private string GenerateCode()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        return new string(Enumerable.Range(0, 6).Select(_ => alphabet[_random.Next(alphabet.Length)]).ToArray());
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _gradientTimer.Dispose();
            _overlayFadeTimer.Dispose();
            _webhookClient.Dispose();
        }

        base.Dispose(disposing);
    }
}
