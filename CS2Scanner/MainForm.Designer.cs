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
            this.reportBrowser = new System.Windows.Forms.WebBrowser();
            this.statusLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // reportBrowser
            // 
            this.reportBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.reportBrowser.Location = new System.Drawing.Point(0, 30);
            this.reportBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.reportBrowser.Name = "reportBrowser";
            this.reportBrowser.Size = new System.Drawing.Size(884, 531);
            this.reportBrowser.TabIndex = 0;
            // 
            // statusLabel
            // 
            this.statusLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.statusLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.statusLabel.ForeColor = System.Drawing.Color.FromArgb(32, 32, 32);
            this.statusLabel.Location = new System.Drawing.Point(0, 0);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Padding = new System.Windows.Forms.Padding(12, 6, 12, 6);
            this.statusLabel.Size = new System.Drawing.Size(884, 30);
            this.statusLabel.TabIndex = 1;
            this.statusLabel.Text = "Сканирование системы...";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 561);
            this.Controls.Add(this.reportBrowser);
            this.Controls.Add(this.statusLabel);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CS2 Scanner Report";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser reportBrowser;
        private System.Windows.Forms.Label statusLabel;
    }
}
