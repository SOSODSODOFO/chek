using Microsoft.Web.WebView2.WinForms;

namespace CS2Scanner
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel headerPanel;
        private System.Windows.Forms.Label instructionLabel;
        private System.Windows.Forms.TextBox codeTextBox;
        private System.Windows.Forms.Button startButton;
        private WebView2 reportView;
        private System.Windows.Forms.Panel contentPanel;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Timer animationTimer;

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
            this.components = new System.ComponentModel.Container();
            this.headerPanel = new System.Windows.Forms.Panel();
            this.statusLabel = new System.Windows.Forms.Label();
            this.startButton = new System.Windows.Forms.Button();
            this.codeTextBox = new System.Windows.Forms.TextBox();
            this.instructionLabel = new System.Windows.Forms.Label();
            this.contentPanel = new System.Windows.Forms.Panel();
            this.reportView = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.animationTimer = new System.Windows.Forms.Timer(this.components);
            this.headerPanel.SuspendLayout();
            this.contentPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.reportView)).BeginInit();
            this.SuspendLayout();
            // 
            // headerPanel
            // 
            this.headerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.headerPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(12)))), ((int)(((byte)(12)))));
            this.headerPanel.Controls.Add(this.statusLabel);
            this.headerPanel.Controls.Add(this.startButton);
            this.headerPanel.Controls.Add(this.codeTextBox);
            this.headerPanel.Controls.Add(this.instructionLabel);
            this.headerPanel.Location = new System.Drawing.Point(32, 32);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Padding = new System.Windows.Forms.Padding(24);
            this.headerPanel.Size = new System.Drawing.Size(736, 160);
            this.headerPanel.TabIndex = 0;
            // 
            // statusLabel
            // 
            this.statusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.statusLabel.AutoSize = true;
            this.statusLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.statusLabel.ForeColor = System.Drawing.Color.Gainsboro;
            this.statusLabel.Location = new System.Drawing.Point(27, 121);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(167, 19);
            this.statusLabel.TabIndex = 3;
            this.statusLabel.Text = "Генерация проверочного кода";
            // 
            // startButton
            // 
            this.startButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.startButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(170)))), ((int)(((byte)(220)))));
            this.startButton.Enabled = false;
            this.startButton.FlatAppearance.BorderSize = 0;
            this.startButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.startButton.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.startButton.ForeColor = System.Drawing.Color.White;
            this.startButton.Location = new System.Drawing.Point(274, 92);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(188, 44);
            this.startButton.TabIndex = 2;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = false;
            this.startButton.Visible = false;
            this.startButton.Click += new System.EventHandler(this.StartButton_ClickAsync);
            // 
            // codeTextBox
            // 
            this.codeTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.codeTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.codeTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.codeTextBox.Font = new System.Drawing.Font("Consolas", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.codeTextBox.ForeColor = System.Drawing.Color.White;
            this.codeTextBox.Location = new System.Drawing.Point(230, 68);
            this.codeTextBox.MaxLength = 6;
            this.codeTextBox.Name = "codeTextBox";
            this.codeTextBox.PlaceholderText = "••••••";
            this.codeTextBox.Size = new System.Drawing.Size(276, 36);
            this.codeTextBox.TabIndex = 1;
            this.codeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.codeTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CodeTextBox_KeyDownAsync);
            // 
            // instructionLabel
            // 
            this.instructionLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.instructionLabel.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.instructionLabel.ForeColor = System.Drawing.Color.White;
            this.instructionLabel.Location = new System.Drawing.Point(24, 24);
            this.instructionLabel.Name = "instructionLabel";
            this.instructionLabel.Size = new System.Drawing.Size(688, 41);
            this.instructionLabel.TabIndex = 0;
            this.instructionLabel.Text = "Сейчас вам администратор скажет код. Введите его в данную строку";
            this.instructionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // contentPanel
            // 
            this.contentPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.contentPanel.BackColor = System.Drawing.Color.Transparent;
            this.contentPanel.Controls.Add(this.reportView);
            this.contentPanel.Location = new System.Drawing.Point(32, 216);
            this.contentPanel.Name = "contentPanel";
            this.contentPanel.Padding = new System.Windows.Forms.Padding(0, 16, 0, 0);
            this.contentPanel.Size = new System.Drawing.Size(736, 552);
            this.contentPanel.TabIndex = 1;
            // 
            // reportView
            // 
            this.reportView.AllowExternalDrop = true;
            this.reportView.CreationProperties = null;
            this.reportView.DefaultBackgroundColor = System.Drawing.Color.Transparent;
            this.reportView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.reportView.Location = new System.Drawing.Point(0, 16);
            this.reportView.Name = "reportView";
            this.reportView.Size = new System.Drawing.Size(736, 536);
            this.reportView.Source = new System.Uri("about:blank", System.UriKind.Absolute);
            this.reportView.TabIndex = 0;
            this.reportView.ZoomFactor = 1D;
            // 
            // animationTimer
            // 
            this.animationTimer.Interval = 16;
            this.animationTimer.Tick += new System.EventHandler(this.AnimationTimer_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(800, 800);
            this.Controls.Add(this.contentPanel);
            this.Controls.Add(this.headerPanel);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CS2 Scanner";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.MainForm_Paint);
            this.headerPanel.ResumeLayout(false);
            this.headerPanel.PerformLayout();
            this.contentPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.reportView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
    }
}
