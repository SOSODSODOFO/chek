using System.Diagnostics;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CS2Scanner;

public partial class MainForm : Form
{
    private readonly WebhookClient _webhookClient;
    private readonly ScannerService _scannerService;
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private readonly Color[] _gradientColors =
    {
        Color.FromArgb(12, 12, 32),
        Color.FromArgb(32, 8, 56),
        Color.FromArgb(12, 32, 56),
        Color.FromArgb(32, 16, 72)
    };

    private string? _generatedCode;
    private float _fadeValue = 1f;
    private bool _fadeOutActive;
    private float _startButtonFade;
    private float _gradientShift;
    private readonly HttpClient _httpClient = new();

    public MainForm()
    {
        InitializeComponent();
        DoubleBuffered = true;
        _webhookClient = new WebhookClient(_httpClient);
        _scannerService = new ScannerService();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        _httpClient.Dispose();
    }

    private async void MainForm_Load(object? sender, EventArgs e)
    {
        animationTimer.Start();
        try
        {
            await reportView.EnsureCoreWebView2Async();
        }
        catch
        {
            // WebView2 runtime may be missing on the current machine, but the rest of the app can continue working.
        }

        await GenerateAndSendCodeAsync();
    }

    private async Task GenerateAndSendCodeAsync()
    {
        instructionLabel.Visible = true;
        codeTextBox.Visible = true;
        startButton.Visible = false;
        statusLabel.Text = "Генерация проверочного кода";

        _generatedCode = CodeGenerator.Generate(6);

        try
        {
            statusLabel.Text = "Отправка кода администратору";
            await _webhookClient.SendCodeAsync(_generatedCode);
            statusLabel.Text = "Код отправлен. Ожидайте инструкций администратора";
        }
        catch (HttpRequestException ex)
        {
            statusLabel.Text = "Не удалось отправить код: " + ex.Message;
        }
    }

    private async Task ValidateCodeAsync()
    {
        if (string.IsNullOrWhiteSpace(codeTextBox.Text) || string.IsNullOrWhiteSpace(_generatedCode))
        {
            return;
        }

        var input = codeTextBox.Text.Trim();
        if (string.Equals(input, _generatedCode, StringComparison.OrdinalIgnoreCase))
        {
            statusLabel.Text = "Код подтверждён. Можно начинать";
            _fadeOutActive = true;
            _fadeValue = 1f;
            _startButtonFade = 0f;
        }
        else
        {
            statusLabel.Text = "Неверный код. Попробуйте ещё раз";
            codeTextBox.SelectAll();
            await ShakeAsync(codeTextBox);
        }
    }

    private async Task ShakeAsync(Control control)
    {
        var original = control.Location;
        for (var i = 0; i < 6; i++)
        {
            control.Location = new Point(original.X + (i % 2 == 0 ? 6 : -6), original.Y);
            await Task.Delay(28);
        }

        control.Location = original;
    }

    private async void CodeTextBox_KeyDownAsync(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            await ValidateCodeAsync();
        }
    }

    private async void StartButton_ClickAsync(object? sender, EventArgs e)
    {
        startButton.Enabled = false;
        statusLabel.Visible = true;
        statusLabel.Text = "Сканирование системы...";

        try
        {
            var progress = new Progress<string>(message => statusLabel.Text = message);
            var result = await _scannerService.ScanAsync(progress);

            statusLabel.Text = "Формирование отчёта";
            var html = await ReportGenerator.BuildReportAsync(result);

            if (reportView.CoreWebView2 is null)
            {
                try
                {
                    await reportView.EnsureCoreWebView2Async();
                }
                catch
                {
                    // ignored
                }
            }

            if (reportView.CoreWebView2 is not null)
            {
                reportView.CoreWebView2.NavigateToString(html);
            }

            statusLabel.Text = "Отправка отчёта";
            await _webhookClient.SendReportAsync(html);
            statusLabel.Text = "Отчёт отправлен. Сканы завершены";
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Ошибка: " + ex.Message;
        }
        finally
        {
            startButton.Enabled = true;
        }
    }

    private void AnimationTimer_Tick(object? sender, EventArgs e)
    {
        _gradientShift += 0.0025f;
        if (_gradientShift > 1f)
        {
            _gradientShift -= 1f;
        }

        if (_fadeOutActive)
        {
            _fadeValue -= 0.05f;
            if (_fadeValue <= 0f)
            {
                _fadeValue = 0f;
                _fadeOutActive = false;
                codeTextBox.Visible = false;
                instructionLabel.Visible = false;
                startButton.Visible = true;
                startButton.Enabled = true;
                startButton.ForeColor = Color.FromArgb(0, Color.White);
                startButton.BackColor = Blend(Color.FromArgb(45, 170, 220), Color.FromArgb(120, 80, 220), 0f);
            }
            ApplyFade(_fadeValue);
        }
        else if (startButton.Visible && _startButtonFade < 1f)
        {
            _startButtonFade = Math.Min(1f, _startButtonFade + 0.05f);
            ApplyStartButtonFade(_startButtonFade);
        }

        Invalidate();
    }

    private void ApplyFade(float value)
    {
        value = Math.Clamp(value, 0f, 1f);
        var alpha = (int)(255 * value);
        instructionLabel.ForeColor = Color.FromArgb(alpha, Color.White);
        codeTextBox.ForeColor = Color.FromArgb(alpha, Color.White);

        var backgroundAlpha = (int)(40 + 215 * value);
        backgroundAlpha = Math.Clamp(backgroundAlpha, 40, 255);
        codeTextBox.BackColor = Color.FromArgb(backgroundAlpha, 32, 32, 32);
    }

    private void ApplyStartButtonFade(float value)
    {
        value = Math.Clamp(value, 0f, 1f);
        var alpha = (int)(255 * value);
        startButton.ForeColor = Color.FromArgb(alpha, Color.White);
        startButton.BackColor = Blend(Color.FromArgb(45, 170, 220), Color.FromArgb(120, 80, 220), value);
    }

    private static Color Blend(Color from, Color to, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        var r = (int)(from.R + (to.R - from.R) * t);
        var g = (int)(from.G + (to.G - from.G) * t);
        var b = (int)(from.B + (to.B - from.B) * t);
        return Color.FromArgb(r, g, b);
    }

    private void MainForm_Paint(object? sender, PaintEventArgs e)
    {
        var rect = ClientRectangle;
        if (rect.Width == 0 || rect.Height == 0)
        {
            return;
        }

        var t = (float)Math.Abs(Math.Sin(_stopwatch.Elapsed.TotalSeconds * 0.2));
        var c1 = Blend(_gradientColors[0], _gradientColors[1], (t + _gradientShift) % 1f);
        var c2 = Blend(_gradientColors[2], _gradientColors[3], (1f - t + _gradientShift) % 1f);

        using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(rect, c1, c2, 135f);
        e.Graphics.FillRectangle(brush, rect);
        using var overlay = new SolidBrush(Color.FromArgb(30, 0, 0, 0));
        e.Graphics.FillRectangle(overlay, rect);
    }
}
