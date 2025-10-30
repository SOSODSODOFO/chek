using System.Drawing;
using System.Windows.Forms;

namespace CS2Scanner;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
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
        contentPanel = new Panel();
        reportPanel = new Panel();
        nvidiaDrsLabel = new Label();
        nvidiaDrsGrid = new DataGridView();
        allProcessesLabel = new Label();
        allProcessesGrid = new DataGridView();
        suspiciousRegistryLabel = new Label();
        suspiciousRegistryGrid = new DataGridView();
        suspiciousFilesLabel = new Label();
        suspiciousFilesGrid = new DataGridView();
        suspiciousProcessesLabel = new Label();
        suspiciousProcessesGrid = new DataGridView();
        summaryFlow = new FlowLayoutPanel();
        reportTitleLabel = new Label();
        judgementLabel = new Label();
        codePanel = new Panel();
        codeStatusLabel = new Label();
        instructionsSubtitleLabel = new Label();
        instructionsTitleLabel = new Label();
        codeInput = new TextBox();
        startButton = new Button();
        backgroundTimer = new Timer(components);
        contentPanel.SuspendLayout();
        reportPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)nvidiaDrsGrid).BeginInit();
        ((System.ComponentModel.ISupportInitialize)allProcessesGrid).BeginInit();
        ((System.ComponentModel.ISupportInitialize)suspiciousRegistryGrid).BeginInit();
        ((System.ComponentModel.ISupportInitialize)suspiciousFilesGrid).BeginInit();
        ((System.ComponentModel.ISupportInitialize)suspiciousProcessesGrid).BeginInit();
        codePanel.SuspendLayout();
        SuspendLayout();
        // 
        // contentPanel
        // 
        contentPanel.BackColor = Color.FromArgb(10, 10, 10);
        contentPanel.Controls.Add(reportPanel);
        contentPanel.Controls.Add(codePanel);
        contentPanel.Controls.Add(startButton);
        contentPanel.Dock = DockStyle.Fill;
        contentPanel.Location = new Point(0, 0);
        contentPanel.Name = "contentPanel";
        contentPanel.Padding = new Padding(18, 18, 18, 18);
        contentPanel.Size = new Size(400, 400);
        contentPanel.TabIndex = 0;
        // 
        // reportPanel
        // 
        reportPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        reportPanel.AutoScroll = true;
        reportPanel.BackColor = Color.FromArgb(22, 22, 22, 160);
        reportPanel.BorderStyle = BorderStyle.None;
        reportPanel.Controls.Add(judgementLabel);
        reportPanel.Controls.Add(nvidiaDrsLabel);
        reportPanel.Controls.Add(nvidiaDrsGrid);
        reportPanel.Controls.Add(allProcessesLabel);
        reportPanel.Controls.Add(allProcessesGrid);
        reportPanel.Controls.Add(suspiciousRegistryLabel);
        reportPanel.Controls.Add(suspiciousRegistryGrid);
        reportPanel.Controls.Add(suspiciousFilesLabel);
        reportPanel.Controls.Add(suspiciousFilesGrid);
        reportPanel.Controls.Add(suspiciousProcessesLabel);
        reportPanel.Controls.Add(suspiciousProcessesGrid);
        reportPanel.Controls.Add(summaryFlow);
        reportPanel.Controls.Add(reportTitleLabel);
        reportPanel.Location = new Point(18, 180);
        reportPanel.Name = "reportPanel";
        reportPanel.Padding = new Padding(12);
        reportPanel.Size = new Size(364, 202);
        reportPanel.TabIndex = 2;
        reportPanel.Visible = false;
        // 
        // nvidiaDrsLabel
        // 
        nvidiaDrsLabel.AutoSize = true;
        nvidiaDrsLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        nvidiaDrsLabel.ForeColor = Color.WhiteSmoke;
        nvidiaDrsLabel.Location = new Point(12, 552);
        nvidiaDrsLabel.Margin = new Padding(3, 16, 3, 0);
        nvidiaDrsLabel.Name = "nvidiaDrsLabel";
        nvidiaDrsLabel.Size = new Size(121, 15);
        nvidiaDrsLabel.TabIndex = 10;
        nvidiaDrsLabel.Text = "NVIDIA DRS (stub)";
        // 
        // nvidiaDrsGrid
        // 
        nvidiaDrsGrid.AllowUserToAddRows = false;
        nvidiaDrsGrid.AllowUserToDeleteRows = false;
        nvidiaDrsGrid.AllowUserToResizeRows = false;
        nvidiaDrsGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        nvidiaDrsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        nvidiaDrsGrid.BackgroundColor = Color.FromArgb(18, 18, 18);
        nvidiaDrsGrid.BorderStyle = BorderStyle.None;
        nvidiaDrsGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        nvidiaDrsGrid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        nvidiaDrsGrid.ColumnHeadersHeight = 28;
        nvidiaDrsGrid.EnableHeadersVisualStyles = false;
        nvidiaDrsGrid.GridColor = Color.FromArgb(45, 45, 45);
        nvidiaDrsGrid.Location = new Point(12, 570);
        nvidiaDrsGrid.MultiSelect = false;
        nvidiaDrsGrid.Name = "nvidiaDrsGrid";
        nvidiaDrsGrid.ReadOnly = true;
        nvidiaDrsGrid.RowHeadersVisible = false;
        nvidiaDrsGrid.RowTemplate.Height = 28;
        nvidiaDrsGrid.ScrollBars = ScrollBars.Vertical;
        nvidiaDrsGrid.Size = new Size(340, 82);
        nvidiaDrsGrid.TabIndex = 11;
        // 
        // allProcessesLabel
        // 
        allProcessesLabel.AutoSize = true;
        allProcessesLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        allProcessesLabel.ForeColor = Color.WhiteSmoke;
        allProcessesLabel.Location = new Point(12, 394);
        allProcessesLabel.Margin = new Padding(3, 16, 3, 0);
        allProcessesLabel.Name = "allProcessesLabel";
        allProcessesLabel.Size = new Size(115, 15);
        allProcessesLabel.TabIndex = 8;
        allProcessesLabel.Text = "Активные процессы";
        // 
        // allProcessesGrid
        // 
        allProcessesGrid.AllowUserToAddRows = false;
        allProcessesGrid.AllowUserToDeleteRows = false;
        allProcessesGrid.AllowUserToResizeRows = false;
        allProcessesGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        allProcessesGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        allProcessesGrid.BackgroundColor = Color.FromArgb(18, 18, 18);
        allProcessesGrid.BorderStyle = BorderStyle.None;
        allProcessesGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        allProcessesGrid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        allProcessesGrid.ColumnHeadersHeight = 28;
        allProcessesGrid.EnableHeadersVisualStyles = false;
        allProcessesGrid.GridColor = Color.FromArgb(45, 45, 45);
        allProcessesGrid.Location = new Point(12, 412);
        allProcessesGrid.MultiSelect = false;
        allProcessesGrid.Name = "allProcessesGrid";
        allProcessesGrid.ReadOnly = true;
        allProcessesGrid.RowHeadersVisible = false;
        allProcessesGrid.RowTemplate.Height = 28;
        allProcessesGrid.ScrollBars = ScrollBars.Vertical;
        allProcessesGrid.Size = new Size(340, 132);
        allProcessesGrid.TabIndex = 9;
        // 
        // suspiciousRegistryLabel
        // 
        suspiciousRegistryLabel.AutoSize = true;
        suspiciousRegistryLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        suspiciousRegistryLabel.ForeColor = Color.WhiteSmoke;
        suspiciousRegistryLabel.Location = new Point(12, 308);
        suspiciousRegistryLabel.Margin = new Padding(3, 16, 3, 0);
        suspiciousRegistryLabel.Name = "suspiciousRegistryLabel";
        suspiciousRegistryLabel.Size = new Size(162, 15);
        suspiciousRegistryLabel.TabIndex = 6;
        suspiciousRegistryLabel.Text = "Подозрительные ключи реестра";
        // 
        // suspiciousRegistryGrid
        // 
        suspiciousRegistryGrid.AllowUserToAddRows = false;
        suspiciousRegistryGrid.AllowUserToDeleteRows = false;
        suspiciousRegistryGrid.AllowUserToResizeRows = false;
        suspiciousRegistryGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        suspiciousRegistryGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        suspiciousRegistryGrid.BackgroundColor = Color.FromArgb(18, 18, 18);
        suspiciousRegistryGrid.BorderStyle = BorderStyle.None;
        suspiciousRegistryGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        suspiciousRegistryGrid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        suspiciousRegistryGrid.ColumnHeadersHeight = 28;
        suspiciousRegistryGrid.EnableHeadersVisualStyles = false;
        suspiciousRegistryGrid.GridColor = Color.FromArgb(45, 45, 45);
        suspiciousRegistryGrid.Location = new Point(12, 326);
        suspiciousRegistryGrid.MultiSelect = false;
        suspiciousRegistryGrid.Name = "suspiciousRegistryGrid";
        suspiciousRegistryGrid.ReadOnly = true;
        suspiciousRegistryGrid.RowHeadersVisible = false;
        suspiciousRegistryGrid.RowTemplate.Height = 28;
        suspiciousRegistryGrid.ScrollBars = ScrollBars.Vertical;
        suspiciousRegistryGrid.Size = new Size(340, 65);
        suspiciousRegistryGrid.TabIndex = 7;
        // 
        // suspiciousFilesLabel
        // 
        suspiciousFilesLabel.AutoSize = true;
        suspiciousFilesLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        suspiciousFilesLabel.ForeColor = Color.WhiteSmoke;
        suspiciousFilesLabel.Location = new Point(12, 222);
        suspiciousFilesLabel.Margin = new Padding(3, 16, 3, 0);
        suspiciousFilesLabel.Name = "suspiciousFilesLabel";
        suspiciousFilesLabel.Size = new Size(141, 15);
        suspiciousFilesLabel.TabIndex = 4;
        suspiciousFilesLabel.Text = "Подозрительные файлы";
        // 
        // suspiciousFilesGrid
        // 
        suspiciousFilesGrid.AllowUserToAddRows = false;
        suspiciousFilesGrid.AllowUserToDeleteRows = false;
        suspiciousFilesGrid.AllowUserToResizeRows = false;
        suspiciousFilesGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        suspiciousFilesGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        suspiciousFilesGrid.BackgroundColor = Color.FromArgb(18, 18, 18);
        suspiciousFilesGrid.BorderStyle = BorderStyle.None;
        suspiciousFilesGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        suspiciousFilesGrid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        suspiciousFilesGrid.ColumnHeadersHeight = 28;
        suspiciousFilesGrid.EnableHeadersVisualStyles = false;
        suspiciousFilesGrid.GridColor = Color.FromArgb(45, 45, 45);
        suspiciousFilesGrid.Location = new Point(12, 240);
        suspiciousFilesGrid.MultiSelect = false;
        suspiciousFilesGrid.Name = "suspiciousFilesGrid";
        suspiciousFilesGrid.ReadOnly = true;
        suspiciousFilesGrid.RowHeadersVisible = false;
        suspiciousFilesGrid.RowTemplate.Height = 28;
        suspiciousFilesGrid.ScrollBars = ScrollBars.Vertical;
        suspiciousFilesGrid.Size = new Size(340, 65);
        suspiciousFilesGrid.TabIndex = 5;
        // 
        // suspiciousProcessesLabel
        // 
        suspiciousProcessesLabel.AutoSize = true;
        suspiciousProcessesLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        suspiciousProcessesLabel.ForeColor = Color.WhiteSmoke;
        suspiciousProcessesLabel.Location = new Point(12, 154);
        suspiciousProcessesLabel.Margin = new Padding(3, 16, 3, 0);
        suspiciousProcessesLabel.Name = "suspiciousProcessesLabel";
        suspiciousProcessesLabel.Size = new Size(155, 15);
        suspiciousProcessesLabel.TabIndex = 2;
        suspiciousProcessesLabel.Text = "Подозрительные процессы";
        // 
        // suspiciousProcessesGrid
        // 
        suspiciousProcessesGrid.AllowUserToAddRows = false;
        suspiciousProcessesGrid.AllowUserToDeleteRows = false;
        suspiciousProcessesGrid.AllowUserToResizeRows = false;
        suspiciousProcessesGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        suspiciousProcessesGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        suspiciousProcessesGrid.BackgroundColor = Color.FromArgb(18, 18, 18);
        suspiciousProcessesGrid.BorderStyle = BorderStyle.None;
        suspiciousProcessesGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        suspiciousProcessesGrid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        suspiciousProcessesGrid.ColumnHeadersHeight = 28;
        suspiciousProcessesGrid.EnableHeadersVisualStyles = false;
        suspiciousProcessesGrid.GridColor = Color.FromArgb(45, 45, 45);
        suspiciousProcessesGrid.Location = new Point(12, 172);
        suspiciousProcessesGrid.MultiSelect = false;
        suspiciousProcessesGrid.Name = "suspiciousProcessesGrid";
        suspiciousProcessesGrid.ReadOnly = true;
        suspiciousProcessesGrid.RowHeadersVisible = false;
        suspiciousProcessesGrid.RowTemplate.Height = 28;
        suspiciousProcessesGrid.ScrollBars = ScrollBars.Vertical;
        suspiciousProcessesGrid.Size = new Size(340, 47);
        suspiciousProcessesGrid.TabIndex = 3;
        // 
        // summaryFlow
        // 
        summaryFlow.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        summaryFlow.AutoSize = true;
        summaryFlow.FlowDirection = FlowDirection.LeftToRight;
        summaryFlow.Location = new Point(12, 60);
        summaryFlow.Margin = new Padding(0);
        summaryFlow.Name = "summaryFlow";
        summaryFlow.Padding = new Padding(0, 4, 0, 0);
        summaryFlow.Size = new Size(340, 68);
        summaryFlow.TabIndex = 1;
        summaryFlow.WrapContents = true;
        // 
        // reportTitleLabel
        // 
        reportTitleLabel.AutoSize = true;
        reportTitleLabel.Font = new Font("Segoe UI", 13F, FontStyle.Bold, GraphicsUnit.Point);
        reportTitleLabel.ForeColor = Color.WhiteSmoke;
        reportTitleLabel.Location = new Point(8, 18);
        reportTitleLabel.Name = "reportTitleLabel";
        reportTitleLabel.Size = new Size(154, 25);
        reportTitleLabel.TabIndex = 0;
        reportTitleLabel.Text = "CS2 Scan Report";
        // 
        // judgementLabel
        // 
        judgementLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        judgementLabel.Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
        judgementLabel.ForeColor = Color.WhiteSmoke;
        judgementLabel.Location = new Point(12, 665);
        judgementLabel.Name = "judgementLabel";
        judgementLabel.Size = new Size(340, 26);
        judgementLabel.TabIndex = 12;
        judgementLabel.Text = "—";
        judgementLabel.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // codePanel
        // 
        codePanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        codePanel.BackColor = Color.FromArgb(24, 24, 24, 180);
        codePanel.Controls.Add(codeStatusLabel);
        codePanel.Controls.Add(instructionsSubtitleLabel);
        codePanel.Controls.Add(instructionsTitleLabel);
        codePanel.Controls.Add(codeInput);
        codePanel.Location = new Point(18, 18);
        codePanel.Name = "codePanel";
        codePanel.Padding = new Padding(18);
        codePanel.Size = new Size(364, 144);
        codePanel.TabIndex = 0;
        // 
        // codeStatusLabel
        // 
        codeStatusLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        codeStatusLabel.AutoSize = true;
        codeStatusLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        codeStatusLabel.ForeColor = Color.DarkGray;
        codeStatusLabel.Location = new Point(21, 116);
        codeStatusLabel.Name = "codeStatusLabel";
        codeStatusLabel.Size = new Size(178, 15);
        codeStatusLabel.TabIndex = 3;
        codeStatusLabel.Text = "Ожидаем подтверждения кода";
        // 
        // instructionsSubtitleLabel
        // 
        instructionsSubtitleLabel.AutoSize = true;
        instructionsSubtitleLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        instructionsSubtitleLabel.ForeColor = Color.Gainsboro;
        instructionsSubtitleLabel.Location = new Point(21, 56);
        instructionsSubtitleLabel.Name = "instructionsSubtitleLabel";
        instructionsSubtitleLabel.Size = new Size(178, 15);
        instructionsSubtitleLabel.TabIndex = 1;
        instructionsSubtitleLabel.Text = "Введите его в данную строку";
        // 
        // instructionsTitleLabel
        // 
        instructionsTitleLabel.AutoSize = true;
        instructionsTitleLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
        instructionsTitleLabel.ForeColor = Color.WhiteSmoke;
        instructionsTitleLabel.Location = new Point(21, 28);
        instructionsTitleLabel.Name = "instructionsTitleLabel";
        instructionsTitleLabel.Size = new Size(265, 19);
        instructionsTitleLabel.TabIndex = 0;
        instructionsTitleLabel.Text = "Сейчас вам администратор скажет код";
        // 
        // codeInput
        // 
        codeInput.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        codeInput.BackColor = Color.FromArgb(14, 14, 14);
        codeInput.BorderStyle = BorderStyle.FixedSingle;
        codeInput.CharacterCasing = CharacterCasing.Upper;
        codeInput.Font = new Font("Consolas", 20F, FontStyle.Bold, GraphicsUnit.Point);
        codeInput.ForeColor = Color.WhiteSmoke;
        codeInput.Location = new Point(21, 82);
        codeInput.MaxLength = 6;
        codeInput.Name = "codeInput";
        codeInput.PlaceholderText = "••••••";
        codeInput.Size = new Size(322, 39);
        codeInput.TabIndex = 2;
        codeInput.TextAlign = HorizontalAlignment.Center;
        codeInput.KeyDown += CodeInput_KeyDown;
        // 
        // startButton
        // 
        startButton.Anchor = AnchorStyles.Top;
        startButton.FlatAppearance.BorderSize = 0;
        startButton.FlatStyle = FlatStyle.Flat;
        startButton.Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
        startButton.ForeColor = Color.White;
        startButton.Location = new Point(148, 168);
        startButton.Name = "startButton";
        startButton.Size = new Size(104, 40);
        startButton.TabIndex = 1;
        startButton.Text = "START";
        startButton.UseVisualStyleBackColor = true;
        startButton.Visible = false;
        startButton.Click += StartButton_ClickAsync;
        // 
        // backgroundTimer
        // 
        backgroundTimer.Enabled = true;
        backgroundTimer.Interval = 40;
        backgroundTimer.Tick += BackgroundTimer_Tick;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.Black;
        ClientSize = new Size(400, 400);
        Controls.Add(contentPanel);
        DoubleBuffered = true;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "CS2 Scanner";
        contentPanel.ResumeLayout(false);
        reportPanel.ResumeLayout(false);
        reportPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)nvidiaDrsGrid).EndInit();
        ((System.ComponentModel.ISupportInitialize)allProcessesGrid).EndInit();
        ((System.ComponentModel.ISupportInitialize)suspiciousRegistryGrid).EndInit();
        ((System.ComponentModel.ISupportInitialize)suspiciousFilesGrid).EndInit();
        ((System.ComponentModel.ISupportInitialize)suspiciousProcessesGrid).EndInit();
        codePanel.ResumeLayout(false);
        codePanel.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private Panel contentPanel = null!;
    private Panel reportPanel = null!;
    private Label reportTitleLabel = null!;
    private FlowLayoutPanel summaryFlow = null!;
    private DataGridView suspiciousProcessesGrid = null!;
    private Label suspiciousProcessesLabel = null!;
    private Label suspiciousFilesLabel = null!;
    private DataGridView suspiciousFilesGrid = null!;
    private Label suspiciousRegistryLabel = null!;
    private DataGridView suspiciousRegistryGrid = null!;
    private Label allProcessesLabel = null!;
    private DataGridView allProcessesGrid = null!;
    private Label nvidiaDrsLabel = null!;
    private DataGridView nvidiaDrsGrid = null!;
    private Panel codePanel = null!;
    private Label instructionsTitleLabel = null!;
    private Label instructionsSubtitleLabel = null!;
    private TextBox codeInput = null!;
    private Button startButton = null!;
    private Label codeStatusLabel = null!;
    private Label judgementLabel = null!;
    private Timer backgroundTimer = null!;
}
