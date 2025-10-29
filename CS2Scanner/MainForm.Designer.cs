using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CS2Scanner;

partial class MainForm
{
    private IContainer? components = null;
    private Panel overlayPanel;
    private Label instructionLabel;
    private Label subInstructionLabel;
    private TextBox codeTextBox;
    private Label hintLabel;
    private Button startButton;
    private Label statusLabel;
    private WebBrowser reportBrowser;
    private Timer fadeTimer;
    private Timer backgroundTimer;

    private void InitializeComponent()
    {
        components = new Container();
        overlayPanel = new Panel();
        instructionLabel = new Label();
        subInstructionLabel = new Label();
        codeTextBox = new TextBox();
        hintLabel = new Label();
        startButton = new Button();
        statusLabel = new Label();
        reportBrowser = new WebBrowser();
        fadeTimer = new Timer(components);
        backgroundTimer = new Timer(components);
        overlayPanel.SuspendLayout();
        SuspendLayout();
        // 
        // overlayPanel
        // 
        overlayPanel.BackColor = Color.FromArgb(220, 10, 10, 10);
        overlayPanel.Controls.Add(hintLabel);
        overlayPanel.Controls.Add(codeTextBox);
        overlayPanel.Controls.Add(subInstructionLabel);
        overlayPanel.Controls.Add(instructionLabel);
        overlayPanel.Dock = DockStyle.Fill;
        overlayPanel.Location = new Point(0, 0);
        overlayPanel.Name = "overlayPanel";
        overlayPanel.Size = new Size(800, 800);
        overlayPanel.TabIndex = 0;
        // 
        // instructionLabel
        // 
        instructionLabel.AutoSize = true;
        instructionLabel.BackColor = Color.Transparent;
        instructionLabel.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Point);
        instructionLabel.ForeColor = Color.White;
        instructionLabel.Location = new Point(120, 260);
        instructionLabel.Name = "instructionLabel";
        instructionLabel.Size = new Size(584, 37);
        instructionLabel.TabIndex = 0;
        instructionLabel.Text = "Сейчас вам администратор скажет код";
        instructionLabel.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // subInstructionLabel
        // 
        subInstructionLabel.AutoSize = true;
        subInstructionLabel.BackColor = Color.Transparent;
        subInstructionLabel.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
        subInstructionLabel.ForeColor = Color.Silver;
        subInstructionLabel.Location = new Point(220, 320);
        subInstructionLabel.Name = "subInstructionLabel";
        subInstructionLabel.Size = new Size(365, 21);
        subInstructionLabel.TabIndex = 1;
        subInstructionLabel.Text = "Введите его в строку ниже, когда получите";
        subInstructionLabel.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // codeTextBox
        // 
        codeTextBox.Anchor = AnchorStyles.None;
        codeTextBox.BackColor = Color.FromArgb(40, 40, 40);
        codeTextBox.BorderStyle = BorderStyle.FixedSingle;
        codeTextBox.Font = new Font("Segoe UI", 26F, FontStyle.Bold, GraphicsUnit.Point);
        codeTextBox.ForeColor = Color.White;
        codeTextBox.Location = new Point(230, 370);
        codeTextBox.MaxLength = 6;
        codeTextBox.Name = "codeTextBox";
        codeTextBox.PlaceholderText = "000000";
        codeTextBox.Size = new Size(340, 54);
        codeTextBox.TabIndex = 2;
        codeTextBox.TextAlign = HorizontalAlignment.Center;
        codeTextBox.TextChanged += CodeTextBoxOnTextChanged;
        // 
        // hintLabel
        // 
        hintLabel.AutoSize = true;
        hintLabel.BackColor = Color.Transparent;
        hintLabel.Font = new Font("Segoe UI", 10F, FontStyle.Italic, GraphicsUnit.Point);
        hintLabel.ForeColor = Color.LightGray;
        hintLabel.Location = new Point(260, 440);
        hintLabel.Name = "hintLabel";
        hintLabel.Size = new Size(286, 19);
        hintLabel.TabIndex = 3;
        hintLabel.Text = "Код состоит из 6 символов (буквы и цифры)";
        hintLabel.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // startButton
        // 
        startButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        startButton.BackColor = Color.FromArgb(60, 60, 60);
        startButton.FlatAppearance.BorderSize = 0;
        startButton.FlatStyle = FlatStyle.Flat;
        startButton.Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point);
        startButton.ForeColor = Color.White;
        startButton.Location = new Point(620, 740);
        startButton.Name = "startButton";
        startButton.Size = new Size(160, 40);
        startButton.TabIndex = 4;
        startButton.Text = "Start";
        startButton.UseVisualStyleBackColor = false;
        startButton.Visible = false;
        startButton.Click += StartButtonOnClick;
        // 
        // statusLabel
        // 
        statusLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        statusLabel.AutoSize = true;
        statusLabel.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        statusLabel.ForeColor = Color.White;
        statusLabel.Location = new Point(20, 750);
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(0, 19);
        statusLabel.TabIndex = 5;
        // 
        // reportBrowser
        // 
        reportBrowser.Dock = DockStyle.Fill;
        reportBrowser.Location = new Point(0, 0);
        reportBrowser.MinimumSize = new Size(20, 20);
        reportBrowser.Name = "reportBrowser";
        reportBrowser.ScriptErrorsSuppressed = true;
        reportBrowser.Size = new Size(800, 800);
        reportBrowser.TabIndex = 6;
        reportBrowser.Visible = false;
        // 
        // fadeTimer
        // 
        fadeTimer.Interval = 30;
        fadeTimer.Tick += FadeTimerOnTick;
        // 
        // backgroundTimer
        // 
        backgroundTimer.Interval = 60;
        backgroundTimer.Tick += BackgroundTimerOnTick;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.Black;
        ClientSize = new Size(800, 800);
        Controls.Add(reportBrowser);
        Controls.Add(statusLabel);
        Controls.Add(startButton);
        Controls.Add(overlayPanel);
        DoubleBuffered = true;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "CS2 Anti-Cheat Scanner";
        Load += MainForm_Load;
        overlayPanel.ResumeLayout(false);
        overlayPanel.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }
}
