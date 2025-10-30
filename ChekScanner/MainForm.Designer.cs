using System.Windows.Forms;

namespace ChekScanner;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer? components = null;

    private Panel entryPanel = null!;
    private Label adminPromptLabel = null!;
    private TextBox codeTextBox = null!;
    private Button verifyButton = null!;
    private Button startButton = null!;
    private Label startHintLabel = null!;
    private Panel scanPanel = null!;
    private TableLayoutPanel infoTable = null!;
    private Label systemInfoHeader = null!;
    private Label suspiciousProcessesHeader = null!;
    private ListBox suspiciousProcessesListBox = null!;
    private Label suspiciousFilesHeader = null!;
    private ListView suspiciousFilesListView = null!;
    private Label suspiciousRegistryHeader = null!;
    private ListBox suspiciousRegistryListBox = null!;
    private Label allProcessesHeader = null!;
    private ListView allProcessesListView = null!;
    private Label nvidiaDrsHeader = null!;
    private ListView nvidiaDrsListView = null!;
    private Label judgementLabel = null!;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        SuspendLayout();

        Font = new System.Drawing.Font("Segoe UI", 9F);
        ForeColor = System.Drawing.Color.White;
        BackColor = System.Drawing.Color.Black;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new System.Drawing.Size(400, 400);
        Text = "CS2 Scan Report";
        DoubleBuffered = true;
        Padding = new Padding(16);

        entryPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 140,
            BackColor = System.Drawing.Color.FromArgb(64, 64, 64),
            Padding = new Padding(12),
            Margin = new Padding(0, 0, 0, 12)
        };

        adminPromptLabel = new Label
        {
            Dock = DockStyle.Top,
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold),
            Text = "Сейчас вам администратор скажет код\nВведите его в данную строку",
            Height = 60
        };

        codeTextBox = new TextBox
        {
            Dock = DockStyle.Top,
            MaxLength = 6,
            BorderStyle = BorderStyle.FixedSingle,
            CharacterCasing = CharacterCasing.Upper,
            Font = new System.Drawing.Font("Consolas", 16F, System.Drawing.FontStyle.Bold),
            TextAlign = HorizontalAlignment.Center,
            Margin = new Padding(0, 10, 0, 10)
        };

        verifyButton = new Button
        {
            Dock = DockStyle.Top,
            Height = 36,
            FlatStyle = FlatStyle.Flat,
            Text = "Проверить",
            BackColor = System.Drawing.Color.FromArgb(30, 144, 255)
        };
        verifyButton.FlatAppearance.BorderSize = 0;

        startButton = new Button
        {
            Dock = DockStyle.Top,
            Height = 38,
            FlatStyle = FlatStyle.Flat,
            Text = "Start Scan",
            BackColor = System.Drawing.Color.FromArgb(72, 201, 176),
            Visible = false
        };
        startButton.FlatAppearance.BorderSize = 0;

        startHintLabel = new Label
        {
            Dock = DockStyle.Top,
            Text = "Нажмите «Start Scan», чтобы запустить проверку",
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            Font = new System.Drawing.Font("Segoe UI", 9F),
            Height = 28,
            ForeColor = System.Drawing.Color.FromArgb(200, 200, 200),
            Visible = false
        };

        entryPanel.Controls.Add(verifyButton);
        entryPanel.Controls.Add(codeTextBox);
        entryPanel.Controls.Add(adminPromptLabel);

        scanPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = System.Drawing.Color.FromArgb(32, 32, 32),
            Padding = new Padding(12),
            AutoScroll = true,
            Visible = false
        };

        systemInfoHeader = new Label
        {
            Dock = DockStyle.Top,
            Text = "Системная информация",
            Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold),
            Height = 26
        };

        infoTable = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 4,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(0, 6, 0, 6)
        };
        infoTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        infoTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        for (int i = 0; i < 4; i++)
        {
            infoTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }
        infoTable.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;

        suspiciousProcessesHeader = new Label
        {
            Dock = DockStyle.Top,
            Text = "Подозрительные процессы",
            Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold),
            Height = 26,
            Margin = new Padding(0, 12, 0, 0)
        };

        suspiciousProcessesListBox = new ListBox
        {
            Dock = DockStyle.Top,
            Height = 70,
            BackColor = System.Drawing.Color.FromArgb(24, 24, 24),
            ForeColor = System.Drawing.Color.White,
            BorderStyle = BorderStyle.None
        };

        suspiciousFilesHeader = new Label
        {
            Dock = DockStyle.Top,
            Text = "Подозрительные файлы",
            Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold),
            Height = 26,
            Margin = new Padding(0, 12, 0, 0)
        };

        suspiciousFilesListView = new ListView
        {
            Dock = DockStyle.Top,
            Height = 90,
            View = View.Details,
            FullRowSelect = true,
            BackColor = System.Drawing.Color.FromArgb(24, 24, 24),
            ForeColor = System.Drawing.Color.White,
            BorderStyle = BorderStyle.None
        };
        suspiciousFilesListView.Columns.Add("Файл", 200);
        suspiciousFilesListView.Columns.Add("Размер", 120);

        suspiciousRegistryHeader = new Label
        {
            Dock = DockStyle.Top,
            Text = "Подозрительные ключи реестра",
            Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold),
            Height = 26,
            Margin = new Padding(0, 12, 0, 0)
        };

        suspiciousRegistryListBox = new ListBox
        {
            Dock = DockStyle.Top,
            Height = 70,
            BackColor = System.Drawing.Color.FromArgb(24, 24, 24),
            ForeColor = System.Drawing.Color.White,
            BorderStyle = BorderStyle.None
        };

        allProcessesHeader = new Label
        {
            Dock = DockStyle.Top,
            Text = "Активные процессы",
            Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold),
            Height = 26,
            Margin = new Padding(0, 12, 0, 0)
        };

        allProcessesListView = new ListView
        {
            Dock = DockStyle.Top,
            Height = 120,
            View = View.Details,
            FullRowSelect = true,
            BackColor = System.Drawing.Color.FromArgb(24, 24, 24),
            ForeColor = System.Drawing.Color.White,
            BorderStyle = BorderStyle.None
        };
        allProcessesListView.Columns.Add("Имя", 120);
        allProcessesListView.Columns.Add("PID", 60);
        allProcessesListView.Columns.Add("Путь", 180);
        allProcessesListView.Columns.Add("Память (MB)", 90);

        nvidiaDrsHeader = new Label
        {
            Dock = DockStyle.Top,
            Text = "NVIDIA DRS (stub)",
            Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold),
            Height = 26,
            Margin = new Padding(0, 12, 0, 0)
        };

        nvidiaDrsListView = new ListView
        {
            Dock = DockStyle.Top,
            Height = 80,
            View = View.Details,
            FullRowSelect = true,
            BackColor = System.Drawing.Color.FromArgb(24, 24, 24),
            ForeColor = System.Drawing.Color.White,
            BorderStyle = BorderStyle.None
        };
        nvidiaDrsListView.Columns.Add("Профиль / Параметр", 200);
        nvidiaDrsListView.Columns.Add("Значение", 120);

        judgementLabel = new Label
        {
            Dock = DockStyle.Top,
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold),
            Height = 30,
            Margin = new Padding(0, 14, 0, 0)
        };

        scanPanel.Controls.Add(judgementLabel);
        scanPanel.Controls.Add(nvidiaDrsListView);
        scanPanel.Controls.Add(nvidiaDrsHeader);
        scanPanel.Controls.Add(allProcessesListView);
        scanPanel.Controls.Add(allProcessesHeader);
        scanPanel.Controls.Add(suspiciousRegistryListBox);
        scanPanel.Controls.Add(suspiciousRegistryHeader);
        scanPanel.Controls.Add(suspiciousFilesListView);
        scanPanel.Controls.Add(suspiciousFilesHeader);
        scanPanel.Controls.Add(suspiciousProcessesListBox);
        scanPanel.Controls.Add(suspiciousProcessesHeader);
        scanPanel.Controls.Add(infoTable);
        scanPanel.Controls.Add(systemInfoHeader);

        Controls.Add(scanPanel);
        Controls.Add(startButton);
        Controls.Add(startHintLabel);
        Controls.Add(entryPanel);

        ResumeLayout(false);
    }

    #endregion
}
