namespace CS2Scanner
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.backgroundPanel = new System.Windows.Forms.Panel();
            this.reportPanel = new System.Windows.Forms.Panel();
            this.reportBrowser = new System.Windows.Forms.WebBrowser();
            this.statusLabel = new System.Windows.Forms.Label();
            this.introPanel = new System.Windows.Forms.Panel();
            this.introLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.introTitleLabel = new System.Windows.Forms.Label();
            this.introSubtitleLabel = new System.Windows.Forms.Label();
            this.codeInputPanel = new System.Windows.Forms.Panel();
            this.codeInputLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.codeTextBox = new System.Windows.Forms.TextBox();
            this.verifyButton = new System.Windows.Forms.Button();
            this.codeStatusLabel = new System.Windows.Forms.Label();
            this.startScanButton = new System.Windows.Forms.Button();
            this.backgroundPanel.SuspendLayout();
            this.reportPanel.SuspendLayout();
            this.introPanel.SuspendLayout();
            this.introLayoutPanel.SuspendLayout();
            this.codeInputPanel.SuspendLayout();
            this.codeInputLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // backgroundPanel
            //
            this.backgroundPanel.BackColor = System.Drawing.Color.Transparent;
            this.backgroundPanel.Controls.Add(this.reportPanel);
            this.backgroundPanel.Controls.Add(this.introPanel);
            this.backgroundPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.backgroundPanel.Location = new System.Drawing.Point(0, 0);
            this.backgroundPanel.Name = "backgroundPanel";
            this.backgroundPanel.Padding = new System.Windows.Forms.Padding(48, 48, 48, 48);
            this.backgroundPanel.Size = new System.Drawing.Size(800, 800);
            this.backgroundPanel.TabIndex = 0;
            //
            // reportPanel
            //
            this.reportPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(26)))), ((int)(((byte)(28)))));
            this.reportPanel.Controls.Add(this.reportBrowser);
            this.reportPanel.Controls.Add(this.statusLabel);
            this.reportPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.reportPanel.Location = new System.Drawing.Point(48, 48);
            this.reportPanel.Name = "reportPanel";
            this.reportPanel.Padding = new System.Windows.Forms.Padding(30, 32, 30, 30);
            this.reportPanel.Size = new System.Drawing.Size(704, 704);
            this.reportPanel.TabIndex = 1;
            //
            // reportBrowser
            //
            this.reportBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.reportBrowser.Location = new System.Drawing.Point(30, 86);
            this.reportBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.reportBrowser.Name = "reportBrowser";
            this.reportBrowser.Size = new System.Drawing.Size(644, 588);
            this.reportBrowser.TabIndex = 1;
            //
            // statusLabel
            //
            this.statusLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.statusLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.statusLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(218)))), ((int)(((byte)(225)))));
            this.statusLabel.Location = new System.Drawing.Point(30, 32);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Padding = new System.Windows.Forms.Padding(8, 8, 8, 16);
            this.statusLabel.Size = new System.Drawing.Size(644, 54);
            this.statusLabel.TabIndex = 0;
            this.statusLabel.Text = "Готовимся к сканированию...";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // introPanel
            //
            this.introPanel.BackColor = System.Drawing.Color.Transparent;
            this.introPanel.Controls.Add(this.introLayoutPanel);
            this.introPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.introPanel.Location = new System.Drawing.Point(48, 48);
            this.introPanel.Name = "introPanel";
            this.introPanel.Padding = new System.Windows.Forms.Padding(20, 40, 20, 40);
            this.introPanel.Size = new System.Drawing.Size(704, 704);
            this.introPanel.TabIndex = 0;
            //
            // introLayoutPanel
            //
            this.introLayoutPanel.ColumnCount = 1;
            this.introLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.introLayoutPanel.Controls.Add(this.introTitleLabel, 0, 0);
            this.introLayoutPanel.Controls.Add(this.introSubtitleLabel, 0, 1);
            this.introLayoutPanel.Controls.Add(this.codeInputPanel, 0, 2);
            this.introLayoutPanel.Controls.Add(this.codeStatusLabel, 0, 3);
            this.introLayoutPanel.Controls.Add(this.startScanButton, 0, 4);
            this.introLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.introLayoutPanel.Location = new System.Drawing.Point(20, 40);
            this.introLayoutPanel.Name = "introLayoutPanel";
            this.introLayoutPanel.RowCount = 6;
            this.introLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.introLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.introLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.introLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.introLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.introLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.introLayoutPanel.Size = new System.Drawing.Size(664, 624);
            this.introLayoutPanel.TabIndex = 0;
            //
            // introTitleLabel
            //
            this.introTitleLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.introTitleLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.introTitleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(235)))));
            this.introTitleLabel.Location = new System.Drawing.Point(3, 0);
            this.introTitleLabel.Name = "introTitleLabel";
            this.introTitleLabel.Size = new System.Drawing.Size(658, 120);
            this.introTitleLabel.TabIndex = 0;
            this.introTitleLabel.Text = "Сейчас вам администратор скажет код";
            this.introTitleLabel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            //
            // introSubtitleLabel
            //
            this.introSubtitleLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.introSubtitleLabel.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.introSubtitleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(170)))), ((int)(((byte)(180)))));
            this.introSubtitleLabel.Location = new System.Drawing.Point(3, 120);
            this.introSubtitleLabel.Name = "introSubtitleLabel";
            this.introSubtitleLabel.Size = new System.Drawing.Size(658, 70);
            this.introSubtitleLabel.TabIndex = 1;
            this.introSubtitleLabel.Text = "Введите его в данную строку ниже";
            this.introSubtitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // codeInputPanel
            //
            this.codeInputPanel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.codeInputPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(22)))), ((int)(((byte)(22)))), ((int)(((byte)(22)))));
            this.codeInputPanel.Controls.Add(this.codeInputLayoutPanel);
            this.codeInputPanel.Location = new System.Drawing.Point(142, 203);
            this.codeInputPanel.Name = "codeInputPanel";
            this.codeInputPanel.Padding = new System.Windows.Forms.Padding(32, 28, 32, 28);
            this.codeInputPanel.Size = new System.Drawing.Size(380, 108);
            this.codeInputPanel.TabIndex = 2;
            //
            // codeInputLayoutPanel
            //
            this.codeInputLayoutPanel.ColumnCount = 2;
            this.codeInputLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.codeInputLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.codeInputLayoutPanel.Controls.Add(this.codeTextBox, 0, 0);
            this.codeInputLayoutPanel.Controls.Add(this.verifyButton, 1, 0);
            this.codeInputLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.codeInputLayoutPanel.Location = new System.Drawing.Point(32, 28);
            this.codeInputLayoutPanel.Name = "codeInputLayoutPanel";
            this.codeInputLayoutPanel.RowCount = 1;
            this.codeInputLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.codeInputLayoutPanel.Size = new System.Drawing.Size(316, 52);
            this.codeInputLayoutPanel.TabIndex = 0;
            //
            // codeTextBox
            //
            this.codeTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(15)))), ((int)(((byte)(18)))));
            this.codeTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.codeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.codeTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.codeTextBox.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.codeTextBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(240)))));
            this.codeTextBox.Location = new System.Drawing.Point(3, 3);
            this.codeTextBox.MaxLength = 6;
            this.codeTextBox.Name = "codeTextBox";
            this.codeTextBox.PlaceholderText = "••••••";
            this.codeTextBox.Size = new System.Drawing.Size(190, 40);
            this.codeTextBox.TabIndex = 0;
            this.codeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.codeTextBox.TextChanged += new System.EventHandler(this.codeTextBox_TextChanged);
            this.codeTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.codeTextBox_KeyDown);
            //
            // verifyButton
            //
            this.verifyButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(110)))), ((int)(((byte)(255)))));
            this.verifyButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.verifyButton.FlatAppearance.BorderSize = 0;
            this.verifyButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.verifyButton.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.verifyButton.ForeColor = System.Drawing.Color.White;
            this.verifyButton.Location = new System.Drawing.Point(199, 3);
            this.verifyButton.Name = "verifyButton";
            this.verifyButton.Size = new System.Drawing.Size(114, 46);
            this.verifyButton.TabIndex = 1;
            this.verifyButton.Text = "Проверить";
            this.verifyButton.UseVisualStyleBackColor = false;
            this.verifyButton.Click += new System.EventHandler(this.verifyButton_Click);
            //
            // codeStatusLabel
            //
            this.codeStatusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.codeStatusLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.codeStatusLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(210)))));
            this.codeStatusLabel.Location = new System.Drawing.Point(3, 350);
            this.codeStatusLabel.Name = "codeStatusLabel";
            this.codeStatusLabel.Size = new System.Drawing.Size(658, 80);
            this.codeStatusLabel.TabIndex = 3;
            this.codeStatusLabel.Text = "Отправляем код администратору...";
            this.codeStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // startScanButton
            //
            this.startScanButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.startScanButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(140)))), ((int)(((byte)(255)))));
            this.startScanButton.FlatAppearance.BorderSize = 0;
            this.startScanButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.startScanButton.Font = new System.Drawing.Font("Segoe UI Semibold", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.startScanButton.ForeColor = System.Drawing.Color.White;
            this.startScanButton.Location = new System.Drawing.Point(222, 433);
            this.startScanButton.Margin = new System.Windows.Forms.Padding(0, 24, 0, 0);
            this.startScanButton.Name = "startScanButton";
            this.startScanButton.Size = new System.Drawing.Size(220, 64);
            this.startScanButton.TabIndex = 4;
            this.startScanButton.Text = "Start";
            this.startScanButton.UseVisualStyleBackColor = false;
            this.startScanButton.Visible = false;
            this.startScanButton.Click += new System.EventHandler(this.startScanButton_Click);
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(800, 800);
            this.Controls.Add(this.backgroundPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CS2 Scanner";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.backgroundPanel.ResumeLayout(false);
            this.reportPanel.ResumeLayout(false);
            this.introPanel.ResumeLayout(false);
            this.introLayoutPanel.ResumeLayout(false);
            this.codeInputPanel.ResumeLayout(false);
            this.codeInputLayoutPanel.ResumeLayout(false);
            this.codeInputLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel backgroundPanel;
        private System.Windows.Forms.Panel introPanel;
        private System.Windows.Forms.TableLayoutPanel introLayoutPanel;
        private System.Windows.Forms.Label introTitleLabel;
        private System.Windows.Forms.Label introSubtitleLabel;
        private System.Windows.Forms.Panel codeInputPanel;
        private System.Windows.Forms.TableLayoutPanel codeInputLayoutPanel;
        private System.Windows.Forms.TextBox codeTextBox;
        private System.Windows.Forms.Button verifyButton;
        private System.Windows.Forms.Label codeStatusLabel;
        private System.Windows.Forms.Button startScanButton;
        private System.Windows.Forms.Panel reportPanel;
        private System.Windows.Forms.WebBrowser reportBrowser;
        private System.Windows.Forms.Label statusLabel;
    }
}
