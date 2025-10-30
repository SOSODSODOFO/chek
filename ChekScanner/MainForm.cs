using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChekScanner;

public partial class MainForm : Form
{
    private readonly DiscordWebhookClient _webhookClient;
    private readonly SystemScanner _scanner;
    private readonly Random _random = new();
    private string? _generatedCode;
    private readonly Timer _dissolveTimer;
    private float _dissolveProgress;
    private int _entryPanelInitialHeight;
    private readonly Timer _backgroundTimer;
    private float _gradientAngle;
    private readonly Dictionary<string, Label> _infoValueLabels = new();

    private static readonly (string Key, string Title, string Hint)[] InfoItems =
    {
        ("SystemInfo", "Система", "ОС / архитектура"),
        ("ScanDuration", "Скан выполнен за", "Время выполнения"),
        ("MachineName", "Имя ПК", "Environment.MachineName"),
        ("Virtualization", "Виртуальная машина", "Определение гипервизора"),
        ("Hardware", "Характеристики", "CPU / GPU / RAM / Disk"),
        ("Vpn", "VPN", "Подключён ли VPN"),
        ("UserCount", "Пользователи в системе", "Локальные учётные записи"),
        ("Uptime", "Время с последней перезагрузки", "Uptime")
    };

    public MainForm()
    {
        InitializeComponent();

        _webhookClient = new DiscordWebhookClient(new Uri("https://discord.com/api/webhooks/1429778047363452999/W_5Yso3yyYgIOQdRRtbQE4_Zfr4J656wTL2ell9gQGrUeiEC-TTCIhqPPRzoheVRv6E4"));
        _scanner = new SystemScanner();

        verifyButton.Click += VerifyButtonOnClick;
        startButton.Click += async (_, _) => await StartScanAsync();
        codeTextBox.KeyDown += CodeTextBoxOnKeyDown;

        _dissolveTimer = new Timer { Interval = 18 };
        _dissolveTimer.Tick += (_, _) => AnimateDissolve();

        _backgroundTimer = new Timer { Interval = 45 };
        _backgroundTimer.Tick += (_, _) =>
        {
            _gradientAngle += 1.5f;
            if (_gradientAngle >= 360f)
            {
                _gradientAngle -= 360f;
            }
            Invalidate();
        };
        _backgroundTimer.Start();

        BuildInfoTable();

        Load += async (_, _) => await InitializeVerificationAsync();
        FormClosed += (_, _) =>
        {
            _dissolveTimer.Stop();
            _backgroundTimer.Stop();
            _webhookClient.Dispose();
        };
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        var rect = ClientRectangle;
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            base.OnPaintBackground(e);
            return;
        }

        using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
            rect,
            Color.FromArgb(255, 10, 10, 10),
            Color.FromArgb(255, 25, 25, 25),
            _gradientAngle);
        var blend = new System.Drawing.Drawing2D.ColorBlend
        {
            Positions = new[] { 0f, 0.5f, 1f },
            Colors = new[]
            {
                Color.FromArgb(255, 10, 10, 10),
                Color.FromArgb(255, 30, 30, 30),
                Color.FromArgb(255, 10, 10, 10)
            }
        };
        brush.InterpolationColors = blend;
        e.Graphics.FillRectangle(brush, rect);
    }

    private async Task InitializeVerificationAsync()
    {
        await Task.Delay(250);
        _generatedCode = GenerateCode();
        await _webhookClient.TrySendMessageAsync($"Новый проверочный код: `{_generatedCode}`");
        adminPromptLabel.Text = "Сейчас вам администратор скажет код\nВведите его в данную строку";
        codeTextBox.Focus();
        _entryPanelInitialHeight = entryPanel.Height;
        startHintLabel.Visible = false;
    }

    private void CodeTextBoxOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            VerifyButtonOnClick(sender, EventArgs.Empty);
            e.Handled = true;
            e.SuppressKeyPress = true;
        }
    }

    private async void VerifyButtonOnClick(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(codeTextBox.Text) || _generatedCode is null)
        {
            ShakeControl(codeTextBox);
            return;
        }

        if (string.Equals(codeTextBox.Text.Trim(), _generatedCode, StringComparison.OrdinalIgnoreCase))
        {
            verifyButton.Enabled = false;
            codeTextBox.Enabled = false;
            adminPromptLabel.Text = "Код подтверждён. Подготовка...";
            startHintLabel.Text = "Нажмите «Start Scan», чтобы запустить проверку";
            await _webhookClient.TrySendMessageAsync("Код подтверждён пользователем.");
            _dissolveProgress = 0f;
            _dissolveTimer.Start();
        }
        else
        {
            adminPromptLabel.Text = "Код не совпал. Попробуйте снова.";
            ShakeControl(codeTextBox);
            codeTextBox.SelectAll();
        }
    }

    private void AnimateDissolve()
    {
        const float step = 0.05f;
        _dissolveProgress += step;

        if (_dissolveProgress >= 1f)
        {
            _dissolveTimer.Stop();
            entryPanel.Visible = false;
            startButton.Visible = true;
            startHintLabel.Visible = true;
            startButton.Focus();
            adminPromptLabel.ForeColor = Color.White;
            return;
        }

        var eased = EaseOutCubic(_dissolveProgress);
        entryPanel.Height = Math.Max(10, (int)(_entryPanelInitialHeight * (1f - eased)));
        var alpha = (int)(255 * (1f - eased));
        if (alpha < 0) alpha = 0;
        var fadedColor = Color.FromArgb(alpha, 255, 255, 255);
        adminPromptLabel.ForeColor = fadedColor;
        codeTextBox.ForeColor = fadedColor;
        verifyButton.ForeColor = fadedColor;
        entryPanel.BackColor = Color.FromArgb(Math.Max(10, alpha / 2), 20, 20, 20);
    }

    private async Task StartScanAsync()
    {
        try
        {
            startButton.Enabled = false;
            startButton.Text = "Сканирование...";
            scanPanel.Visible = true;
            startHintLabel.Text = "Сканирование запущено. Ожидайте результаты...";
            judgementLabel.Text = "Выполняется анализ...";
            judgementLabel.ForeColor = Color.LightGray;

            var result = await _scanner.ScanAsync();

            PopulateInfoCards(result);
            PopulateSuspiciousLists(result);
            PopulateProcessList(result);
            PopulateDrs(result);

            judgementLabel.Text = result.Judgement;
            judgementLabel.ForeColor = result.Judgement.Contains("Clean", StringComparison.OrdinalIgnoreCase)
                ? Color.FromArgb(138, 255, 138)
                : Color.FromArgb(255, 107, 107);
        }
        catch (Exception ex)
        {
            judgementLabel.Text = "Ошибка: " + ex.Message;
            judgementLabel.ForeColor = Color.FromArgb(255, 107, 107);
        }
        finally
        {
            startButton.Text = "Start Scan";
            startButton.Enabled = true;
            startHintLabel.Text = "Проверка завершена. При необходимости повторите сканирование.";
        }
    }

    private void PopulateInfoCards(SystemScanResult result)
    {
        UpdateInfoValue("SystemInfo", result.SystemInfo);
        UpdateInfoValue("ScanDuration", result.ScanDuration);
        UpdateInfoValue("MachineName", result.MachineName);
        UpdateInfoValue("Virtualization", result.VirtualizationStatus);
        UpdateInfoValue("Hardware", result.HardwareSummary);
        UpdateInfoValue("Vpn", result.VpnStatus);
        UpdateInfoValue("UserCount", result.LocalUserCount.ToString());
        UpdateInfoValue("Uptime", result.Uptime);
    }

    private void PopulateSuspiciousLists(SystemScanResult result)
    {
        suspiciousProcessesListBox.BeginUpdate();
        suspiciousProcessesListBox.Items.Clear();
        foreach (var process in result.SuspiciousProcesses.DefaultIfEmpty("Нет"))
        {
            suspiciousProcessesListBox.Items.Add(process);
        }
        suspiciousProcessesListBox.EndUpdate();

        suspiciousFilesListView.BeginUpdate();
        suspiciousFilesListView.Items.Clear();
        foreach (var file in result.SuspiciousFiles)
        {
            suspiciousFilesListView.Items.Add(new ListViewItem(new[] { file.Path, file.Size }));
        }
        if (!result.SuspiciousFiles.Any())
        {
            suspiciousFilesListView.Items.Add(new ListViewItem(new[] { "Нет", string.Empty }));
        }
        suspiciousFilesListView.EndUpdate();

        suspiciousRegistryListBox.BeginUpdate();
        suspiciousRegistryListBox.Items.Clear();
        foreach (var key in result.SuspiciousRegistryKeys.DefaultIfEmpty("Нет"))
        {
            suspiciousRegistryListBox.Items.Add(key);
        }
        suspiciousRegistryListBox.EndUpdate();
    }

    private void PopulateProcessList(SystemScanResult result)
    {
        allProcessesListView.BeginUpdate();
        allProcessesListView.Items.Clear();
        foreach (var process in result.Processes)
        {
            allProcessesListView.Items.Add(new ListViewItem(new[]
            {
                process.Name,
                process.Id.ToString(),
                process.Path,
                process.MemoryMegabytes.ToString("F0")
            }));
        }
        allProcessesListView.EndUpdate();
    }

    private void PopulateDrs(SystemScanResult result)
    {
        nvidiaDrsListView.BeginUpdate();
        nvidiaDrsListView.Items.Clear();
        foreach (var entry in result.NvidiaDrsEntries)
        {
            nvidiaDrsListView.Items.Add(new ListViewItem(new[] { entry.Profile, entry.Value }));
        }
        if (!result.NvidiaDrsEntries.Any())
        {
            nvidiaDrsListView.Items.Add(new ListViewItem(new[] { "—", "—" }));
        }
        nvidiaDrsListView.EndUpdate();
    }

    private void BuildInfoTable()
    {
        infoTable.SuspendLayout();
        infoTable.Controls.Clear();
        _infoValueLabels.Clear();

        var index = 0;
        foreach (var (key, title, hint) in InfoItems)
        {
            var card = CreateInfoCard(title, hint);
            var valueLabel = (Label)card.Tag;
            _infoValueLabels[key] = valueLabel;
            var row = index / infoTable.ColumnCount;
            var column = index % infoTable.ColumnCount;
            infoTable.Controls.Add(card, column, row);
            index++;
        }

        infoTable.ResumeLayout();
    }

    private Panel CreateInfoCard(string title, string hint)
    {
        var container = new Panel
        {
            Margin = new Padding(4),
            Padding = new Padding(8),
            BackColor = Color.FromArgb(40, 40, 40),
            BorderStyle = BorderStyle.FixedSingle,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Fill
        };

        var titleLabel = new Label
        {
            Dock = DockStyle.Top,
            Text = title,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            AutoSize = true
        };

        var valueLabel = new Label
        {
            Dock = DockStyle.Top,
            Text = "—",
            Font = new Font("Segoe UI", 9F),
            AutoSize = true
        };

        var hintLabel = new Label
        {
            Dock = DockStyle.Top,
            Text = hint,
            Font = new Font("Segoe UI", 8F),
            ForeColor = Color.FromArgb(180, 180, 180),
            AutoSize = true
        };

        container.Controls.Add(hintLabel);
        container.Controls.Add(valueLabel);
        container.Controls.Add(titleLabel);
        container.Tag = valueLabel;

        return container;
    }

    private void UpdateInfoValue(string key, string value)
    {
        if (_infoValueLabels.TryGetValue(key, out var label))
        {
            label.Text = value;
        }
    }

    private string GenerateCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        return new string(Enumerable.Range(0, 6).Select(_ => chars[_random.Next(chars.Length)]).ToArray());
    }

    private static float EaseOutCubic(float x)
    {
        var t = Math.Clamp(x, 0f, 1f);
        return 1f - (float)Math.Pow(1f - t, 3);
    }

    private void ShakeControl(Control control)
    {
        var originalColor = control.BackColor;
        var accent = Color.FromArgb(255, 70, 70);
        var timer = new Timer { Interval = 90 };
        var tick = 0;
        timer.Tick += (_, _) =>
        {
            control.BackColor = tick % 2 == 0 ? accent : originalColor;
            tick++;
            if (tick > 5)
            {
                timer.Stop();
                control.BackColor = originalColor;
                timer.Dispose();
            }
        };
        timer.Start();
    }
}
