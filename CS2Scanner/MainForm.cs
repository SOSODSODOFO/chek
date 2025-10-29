using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CS2Scanner.Reporting;
using CS2Scanner.Scanning;
using CS2Scanner.Services;
using CS2Scanner.Utilities;

namespace CS2Scanner;

public partial class MainForm : Form
{
    private const string WebhookUrl = "https://discord.com/api/webhooks/1429778047363452999/W_5Yso3yyYgIOQdRRtbQE4_Zfr4J656wTL2ell9gQGrUeiEC-TTCIhqPPRzoheVRv6E4";
    private readonly DiscordWebhookService _webhookService = new(WebhookUrl);
    private string _authCode = string.Empty;
    private bool _startUnlocked;
    private bool _isScanning;
    private CancellationTokenSource? _scanCts;
    private double _overlayOpacity = 1.0;
    private float _gradientAngle;
    private double _pulse;

    private Color _instructionBaseColor;
    private Color _subInstructionBaseColor;
    private Color _hintBaseColor;
    private Color _codeTextBaseColor;

    public MainForm()
    {
        InitializeComponent();
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
    }

    private async void MainForm_Load(object? sender, EventArgs e)
    {
        _instructionBaseColor = instructionLabel.ForeColor;
        _subInstructionBaseColor = subInstructionLabel.ForeColor;
        _hintBaseColor = hintLabel.ForeColor;
        _codeTextBaseColor = codeTextBox.ForeColor;

        backgroundTimer.Start();
        statusLabel.Text = "Генерация проверочного кода...";

        try
        {
            _authCode = CodeGenerator.Generate(6);
            await _webhookService.SendMessageAsync($"🔐 Новый код подтверждения: `{_authCode}`");
            statusLabel.Text = "Код отправлен администратору. Ожидаем подтверждения.";
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Ошибка при отправке кода: " + ex.Message;
        }

        codeTextBox.Focus();
    }

    private async void CodeTextBoxOnTextChanged(object? sender, EventArgs e)
    {
        if (_startUnlocked)
        {
            return;
        }

        var raw = codeTextBox.Text.Trim().ToUpperInvariant();
        if (raw != codeTextBox.Text)
        {
            var selection = codeTextBox.SelectionStart;
            codeTextBox.Text = raw;
            codeTextBox.SelectionStart = Math.Min(selection, codeTextBox.Text.Length);
            return;
        }

        if (raw.Length < 6)
        {
            codeTextBox.ForeColor = _codeTextBaseColor;
            return;
        }

        if (!string.Equals(raw, _authCode, StringComparison.OrdinalIgnoreCase))
        {
            codeTextBox.ForeColor = Color.IndianRed;
            statusLabel.Text = "Неверный код. Проверьте ввод.";
            return;
        }

        _startUnlocked = true;
        codeTextBox.Enabled = false;
        statusLabel.Text = "Код принят. Готово к запуску проверки.";
        codeTextBox.ForeColor = Color.FromArgb(180, 255, 255, 255);

        try
        {
            await _webhookService.SendMessageAsync($"✅ Пользователь подтвердил вход кодом `{raw}`");
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Предупреждение: не удалось уведомить вебхук. " + ex.Message;
        }

        fadeTimer.Start();
        startButton.Visible = true;
        startButton.Enabled = true;
        startButton.Focus();
    }

    private void StartButtonOnClick(object? sender, EventArgs e)
    {
        if (!_startUnlocked || _isScanning)
        {
            return;
        }

        _ = RunScanAsync();
    }

    private async Task RunScanAsync()
    {
        _isScanning = true;
        startButton.Enabled = false;
        reportBrowser.Visible = true;
        reportBrowser.DocumentText = "";
        statusLabel.Text = "Запуск сканирования...";

        _scanCts?.Dispose();
        _scanCts = new CancellationTokenSource();

        var progress = new Progress<string>(message => statusLabel.Text = message);

        try
        {
            await _webhookService.SendMessageAsync("🚀 Начато сканирование системы.");
        }
        catch
        {
            // ignore
        }

        try
        {
            var report = await SystemInfoCollector.RunFullScanAsync(progress, _scanCts.Token).ConfigureAwait(false);
            var html = HtmlReportBuilder.Build(report);

            if (!IsDisposed)
            {
                BeginInvoke(new Action(() =>
                {
                    reportBrowser.DocumentText = html;
                    statusLabel.Text = "Сканирование завершено";
                }));
            }

            try
            {
                await _webhookService.SendReportAsync(html, _scanCts.Token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                {
                    BeginInvoke(new Action(() => statusLabel.Text = "Отчёт создан. Ошибка при отправке: " + ex.Message));
                }
            }
        }
        catch (OperationCanceledException)
        {
            statusLabel.Text = "Сканирование отменено.";
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Ошибка во время сканирования: " + ex.Message;
        }
        finally
        {
            _isScanning = false;
            if (!IsDisposed)
            {
                BeginInvoke(new Action(() => startButton.Enabled = true));
            }
        }
    }

    private void FadeTimerOnTick(object? sender, EventArgs e)
    {
        const double step = 0.05;
        _overlayOpacity = Math.Max(0, _overlayOpacity - step);
        ApplyOverlayOpacity(_overlayOpacity);

        if (_overlayOpacity <= 0)
        {
            fadeTimer.Stop();
            overlayPanel.Visible = false;
            reportBrowser.Visible = true;
            statusLabel.Text = "Нажмите Start для запуска проверки.";
        }
    }

    private void ApplyOverlayOpacity(double opacity)
    {
        var alphaPanel = (int)(opacity * 220);
        alphaPanel = Math.Max(0, Math.Min(255, alphaPanel));
        overlayPanel.BackColor = Color.FromArgb(alphaPanel, 10, 10, 10);

        var alphaText = (int)(opacity * 255);
        alphaText = Math.Max(0, Math.Min(255, alphaText));
        instructionLabel.ForeColor = Color.FromArgb(alphaText, _instructionBaseColor);
        subInstructionLabel.ForeColor = Color.FromArgb(alphaText, _subInstructionBaseColor);
        hintLabel.ForeColor = Color.FromArgb(alphaText, _hintBaseColor);
        codeTextBox.ForeColor = Color.FromArgb(Math.Max(alphaText, 40), _codeTextBaseColor);
        codeTextBox.BackColor = Color.FromArgb(Math.Max(alphaPanel, 30), 40, 40, 40);
        codeTextBox.BorderStyle = BorderStyle.None;
    }

    private void BackgroundTimerOnTick(object? sender, EventArgs e)
    {
        _gradientAngle += 0.8f;
        if (_gradientAngle > 360f)
        {
            _gradientAngle -= 360f;
        }

        _pulse += 0.015;
        if (_pulse > 1.0)
        {
            _pulse -= 1.0;
        }

        Invalidate();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        base.OnPaintBackground(e);
        var rect = ClientRectangle;
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return;
        }

        var pulseValue = (float)(Math.Sin(_pulse * Math.PI * 2) * 0.3 + 0.7);
        var color1 = Color.FromArgb(255, (int)(20 + 35 * pulseValue), (int)(20 + 30 * pulseValue), (int)(20 + 25 * pulseValue));
        var color2 = Color.FromArgb(255, (int)(10 + 15 * pulseValue), (int)(10 + 20 * pulseValue), (int)(10 + 30 * pulseValue));
        var color3 = Color.FromArgb(255, (int)(5 + 10 * pulseValue), (int)(5 + 15 * pulseValue), (int)(5 + 20 * pulseValue));

        using var brush = new LinearGradientBrush(rect, color1, color2, _gradientAngle);
        var blend = new ColorBlend
        {
            Colors = new[] { color1, color2, color3 },
            Positions = new[] { 0f, 0.5f, 1f }
        };
        brush.InterpolationColors = blend;
        e.Graphics.FillRectangle(brush, rect);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        backgroundTimer.Stop();
        fadeTimer.Stop();
        _scanCts?.Cancel();
        _scanCts?.Dispose();
        _webhookService.Dispose();
    }
}
