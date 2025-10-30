using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;
using CS2Scanner.Scanning;
using CS2Scanner.Services;

namespace CS2Scanner;

public partial class MainForm : Form
{
    private readonly SystemScanner _scanner = new();
    private readonly WebhookClient _webhookClient = new("https://discord.com/api/webhooks/1429778047363452999/W_5Yso3yyYgIOQdRRtbQE4_Zfr4J656wTL2ell9gQGrUeiEC-TTCIhqPPRzoheVRv6E4");
    private readonly Dictionary<string, Label> _summaryValueLabels = new();

    private string _verificationCode = string.Empty;
    private bool _codeAccepted;
    private bool _isScanning;
    private float _backgroundPulse;
    private PointF _cursorPosition = new(200, 200);

    public MainForm()
    {
        InitializeComponent();
        DoubleBuffered = true;
        reportPanel.BackColor = Color.FromArgb(160, 22, 22, 22);
        codePanel.BackColor = Color.FromArgb(180, 24, 24, 24);
        contentPanel.BackColor = Color.Transparent;
        CreateSummaryCards();
        SetupDataGrids();
        AttachInteractiveBackground(this);
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        await PrepareVerificationAsync();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        _webhookClient.Dispose();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = ClientRectangle;
        graphics.Clear(Color.Black);

        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return;
        }

        var highlightRect = new RectangleF(-rect.Width, -rect.Height, rect.Width * 3f, rect.Height * 3f);
        using var path = new GraphicsPath();
        path.AddEllipse(highlightRect);

        var pulse = (float)Math.Sin(_backgroundPulse);
        var pulse2 = (float)Math.Cos(_backgroundPulse / 2f);
        var centerColor = Color.FromArgb(160, (int)(35 + 20 * pulse), (int)(35 + 10 * pulse2), (int)(40 + 20 * pulse));

        using var brush = new PathGradientBrush(path)
        {
            CenterPoint = _cursorPosition,
            CenterColor = centerColor,
            SurroundColors = new[] { Color.FromArgb(255, 5, 5, 5) }
        };
        graphics.FillRectangle(brush, rect);

        using var overlay = new LinearGradientBrush(rect, Color.FromArgb(50, 0, 0, 0), Color.FromArgb(10, 10, 10, 10), 135f);
        graphics.FillRectangle(overlay, rect);
    }

    private async Task PrepareVerificationAsync()
    {
        _codeAccepted = false;
        startButton.Visible = false;
        codePanel.Visible = true;
        codePanel.Height = 144;
        codeInput.Enabled = true;
        codeInput.Clear();
        codeStatusLabel.Text = "Отправляем код...";

        _verificationCode = GenerateVerificationCode(6);
        try
        {
            await _webhookClient.SendVerificationCodeAsync(_verificationCode);
            codeStatusLabel.Text = "Код отправлен. Ожидаем подтверждение.";
        }
        catch (Exception ex)
        {
            codeStatusLabel.Text = "Не удалось отправить код.";
            MessageBox.Show(this, "Ошибка при отправке кода в веб-хук: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        codeInput.Focus();
    }

    private static string GenerateVerificationCode(int length)
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        Span<char> chars = stackalloc char[length];
        Span<byte> bytes = stackalloc byte[length];
        RandomNumberGenerator.Fill(bytes);
        for (var i = 0; i < length; i++)
        {
            chars[i] = alphabet[bytes[i] % alphabet.Length];
        }

        return new string(chars);
    }

    private async void CodeInput_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            await ValidateCodeAsync();
        }
    }

    private async Task ValidateCodeAsync()
    {
        if (_codeAccepted)
        {
            return;
        }

        var input = codeInput.Text.Trim().ToUpperInvariant();
        if (input.Length != _verificationCode.Length)
        {
            codeStatusLabel.Text = "Код должен содержать 6 символов.";
            ShakeControl(codeInput);
            return;
        }

        if (!string.Equals(input, _verificationCode, StringComparison.OrdinalIgnoreCase))
        {
            codeStatusLabel.Text = "Код не совпадает. Попробуйте снова.";
            ShakeControl(codeInput);
            return;
        }

        _codeAccepted = true;
        codeStatusLabel.Text = "Код подтверждён!";
        codeInput.Enabled = false;
        await CollapseCodePanelAsync();
        startButton.Visible = true;
        startButton.Focus();
    }

    private async Task CollapseCodePanelAsync()
    {
        var source = new TaskCompletionSource<bool>();
        var initial = codePanel.Height;
        var timer = new Timer { Interval = 15 };
        timer.Tick += (_, _) =>
        {
            if (codePanel.Height <= 10)
            {
                timer.Stop();
                timer.Dispose();
                codePanel.Visible = false;
                codePanel.Height = initial;
                source.TrySetResult(true);
                return;
            }

            codePanel.Height -= 8;
        };
        timer.Start();
        await source.Task.ConfigureAwait(true);
    }

    private void ShakeControl(Control control)
    {
        const int amplitude = 6;
        var ticks = 0;
        var origin = control.Location;
        var timer = new Timer { Interval = 15 };
        timer.Tick += (_, _) =>
        {
            ticks++;
            var offset = (int)(Math.Sin(ticks * Math.PI / 4) * amplitude);
            control.Location = new Point(origin.X + offset, origin.Y);
            if (ticks >= 12)
            {
                timer.Stop();
                timer.Dispose();
                control.Location = origin;
            }
        };
        timer.Start();
    }

    private async void StartButton_ClickAsync(object? sender, EventArgs e)
    {
        if (_isScanning)
        {
            return;
        }

        _isScanning = true;
        startButton.Enabled = false;
        startButton.Text = "Сканируем...";

        await RunScanAsync();

        startButton.Text = "START";
        startButton.Enabled = true;
        _isScanning = false;
    }

    private async Task RunScanAsync()
    {
        try
        {
            var report = await Task.Run(() => _scanner.Scan());
            await ShowReportAsync(report);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Ошибка во время сканирования: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task ShowReportAsync(SystemReport report)
    {
        DisplayReport(report);
        await AnimateReportRevealAsync();
    }

    private void DisplayReport(SystemReport report)
    {
        reportPanel.Visible = true;
        UpdateSummary("system", report.SystemInfo);
        UpdateSummary("scan", report.ScanDuration);
        UpdateSummary("machine", report.MachineName);
        UpdateSummary("vm", report.VmStatus);
        UpdateSummary("hardware", report.HardwareSpec);
        UpdateSummary("vpn", report.VpnStatus);
        UpdateSummary("users", report.UserCount.ToString());
        UpdateSummary("boot", report.BootTime);

        FillProcessesTable(report);
        FillSuspiciousProcesses(report);
        FillSuspiciousFiles(report);
        FillRegistryTable(report);
        FillNvidiaStub();

        judgementLabel.Text = report.Judgement;
        judgementLabel.ForeColor = report.Judgement.Contains("Clean", StringComparison.OrdinalIgnoreCase)
            ? Color.FromArgb(0x8A, 0xFF, 0x8A)
            : Color.FromArgb(0xFF, 0x6B, 0x6B);
    }

    private void FillSuspiciousProcesses(SystemReport report)
    {
        suspiciousProcessesGrid.Rows.Clear();
        if (report.SuspiciousProcesses.Count == 0)
        {
            suspiciousProcessesGrid.Rows.Add("— подозрительные процессы не обнаружены —");
        }
        else
        {
            foreach (var process in report.SuspiciousProcesses)
            {
                suspiciousProcessesGrid.Rows.Add(process);
            }
        }
    }

    private void FillSuspiciousFiles(SystemReport report)
    {
        suspiciousFilesGrid.Rows.Clear();
        if (report.SuspiciousFiles.Count == 0)
        {
            suspiciousFilesGrid.Rows.Add("—", "—");
            return;
        }

        foreach (var file in report.SuspiciousFiles)
        {
            var label = file.Exists ? file.Size : "Не найден";
            suspiciousFilesGrid.Rows.Add(file.Path, label);
        }
    }

    private void FillRegistryTable(SystemReport report)
    {
        suspiciousRegistryGrid.Rows.Clear();
        if (report.SuspiciousRegistryKeys.Count == 0)
        {
            suspiciousRegistryGrid.Rows.Add("— записи не обнаружены —");
            return;
        }

        foreach (var key in report.SuspiciousRegistryKeys)
        {
            suspiciousRegistryGrid.Rows.Add(key);
        }
    }

    private void FillProcessesTable(SystemReport report)
    {
        allProcessesGrid.Rows.Clear();
        foreach (var process in report.Processes)
        {
            allProcessesGrid.Rows.Add(process.Name, process.Id, string.IsNullOrWhiteSpace(process.Location) ? "—" : process.Location, process.MemoryMb.ToString("0.0"));
        }
    }

    private void FillNvidiaStub()
    {
        nvidiaDrsGrid.Rows.Clear();
        nvidiaDrsGrid.Rows.Add("@-- stub: заполняется позже --", "—");
    }

    private async Task AnimateReportRevealAsync()
    {
        const int steps = 20;
        const int maxAlpha = 160;
        reportPanel.BackColor = Color.FromArgb(0, 22, 22, 22);
        for (var i = 0; i <= steps; i++)
        {
            var alpha = (int)(maxAlpha * i / (float)steps);
            reportPanel.BackColor = Color.FromArgb(alpha, 22, 22, 22);
            await Task.Delay(15);
        }
    }

    private void UpdateSummary(string key, string value)
    {
        if (_summaryValueLabels.TryGetValue(key, out var label))
        {
            label.Text = value;
        }
    }

    private void CreateSummaryCards()
    {
        summaryFlow.Controls.Clear();
        _summaryValueLabels.Clear();
        summaryFlow.SuspendLayout();
        AddCard("Система", "—", "ОС / архитектура", "system");
        AddCard("Скан выполнен за", "—", "Время выполнения", "scan");
        AddCard("Имя ПК", "—", "Environment.MachineName", "machine");
        AddCard("Виртуальная машина", "—", "Определение гипервизора", "vm");
        AddCard("Характеристики", "—", "CPU / GPU / RAM / Disk", "hardware");
        AddCard("VPN", "—", "Подключён ли VPN", "vpn");
        AddCard("Пользователи", "—", "Число локальных учётных записей", "users");
        AddCard("Uptime", "—", "Время с последней перезагрузки", "boot");
        summaryFlow.ResumeLayout();
    }

    private void AddCard(string title, string value, string hint, string key)
    {
        var panel = new Panel
        {
            Width = 160,
            Height = 70,
            Margin = new Padding(0, 0, 12, 12),
            BackColor = Color.FromArgb(40, 255, 255, 255),
            Padding = new Padding(10)
        };

        var titleLabel = new Label
        {
            Text = title,
            ForeColor = Color.WhiteSmoke,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point),
            Dock = DockStyle.Top,
            Height = 18
        };

        var valueLabel = new Label
        {
            Text = value,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
            Dock = DockStyle.Top,
            Height = 24
        };

        var hintLabel = new Label
        {
            Text = hint,
            ForeColor = Color.FromArgb(180, 220, 220, 220),
            Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point),
            Dock = DockStyle.Top,
            Height = 16
        };

        panel.Controls.Add(hintLabel);
        panel.Controls.Add(valueLabel);
        panel.Controls.Add(titleLabel);
        summaryFlow.Controls.Add(panel);
        _summaryValueLabels[key] = valueLabel;
    }

    private void SetupDataGrids()
    {
        ConfigureGrid(suspiciousProcessesGrid);
        ConfigureGrid(suspiciousFilesGrid);
        ConfigureGrid(suspiciousRegistryGrid);
        ConfigureGrid(allProcessesGrid);
        ConfigureGrid(nvidiaDrsGrid);

        suspiciousProcessesGrid.Columns.Clear();
        suspiciousProcessesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Процесс",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            ReadOnly = true,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });

        suspiciousFilesGrid.Columns.Clear();
        suspiciousFilesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Файл",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            ReadOnly = true,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });
        suspiciousFilesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Размер",
            Width = 90,
            ReadOnly = true,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });

        suspiciousRegistryGrid.Columns.Clear();
        suspiciousRegistryGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Ключ",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            ReadOnly = true,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });

        allProcessesGrid.Columns.Clear();
        allProcessesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Имя процесса",
            Width = 120,
            ReadOnly = true,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });
        allProcessesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "PID",
            Width = 50,
            ReadOnly = true,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });
        allProcessesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Путь",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            ReadOnly = true,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });
        allProcessesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Память (MB)",
            Width = 90,
            ReadOnly = true,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });

        nvidiaDrsGrid.Columns.Clear();
        nvidiaDrsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Профиль / Параметр",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            ReadOnly = true,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });
        nvidiaDrsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Значение",
            Width = 120,
            ReadOnly = true,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });
    }

    private static void ConfigureGrid(DataGridView grid)
    {
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(50, 50, 50);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.Gainsboro;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        grid.DefaultCellStyle.BackColor = Color.FromArgb(22, 22, 22);
        grid.DefaultCellStyle.ForeColor = Color.WhiteSmoke;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(60, 60, 60);
        grid.DefaultCellStyle.SelectionForeColor = Color.White;
        grid.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        grid.GridColor = Color.FromArgb(45, 45, 45);
        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.RowHeadersVisible = false;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.BackgroundColor = Color.FromArgb(18, 18, 18);
        grid.MultiSelect = false;
        grid.ScrollBars = ScrollBars.Vertical;
    }

    private void AttachInteractiveBackground(Control control)
    {
        control.MouseMove += HandleMouseMove;
        foreach (Control child in control.Controls)
        {
            AttachInteractiveBackground(child);
        }
    }

    private void HandleMouseMove(object? sender, MouseEventArgs e)
    {
        if (sender is not Control source)
        {
            return;
        }

        var screenPoint = source.PointToScreen(e.Location);
        _cursorPosition = PointToClient(screenPoint);
        Invalidate();
    }

    private void BackgroundTimer_Tick(object? sender, EventArgs e)
    {
        _backgroundPulse += 0.05f;
        if (_backgroundPulse > Math.PI * 2)
        {
            _backgroundPulse -= (float)(Math.PI * 2);
        }

        Invalidate();
    }
}
