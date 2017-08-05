namespace WGClanIconDownload
{
    partial class Mainform
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.start_button = new System.Windows.Forms.Button();
            this.checkedListBoxRegion = new System.Windows.Forms.CheckedListBox();
            this.threads_trackBar = new System.Windows.Forms.TrackBar();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.Message_richTextBox = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.threads_trackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // start_button
            // 
            this.start_button.Location = new System.Drawing.Point(374, 12);
            this.start_button.Name = "start_button";
            this.start_button.Size = new System.Drawing.Size(97, 39);
            this.start_button.TabIndex = 0;
            this.start_button.Text = "start";
            this.start_button.UseVisualStyleBackColor = true;
            this.start_button.Click += new System.EventHandler(this.start_button_Click);
            // 
            // checkedListBoxRegion
            // 
            this.checkedListBoxRegion.FormattingEnabled = true;
            this.checkedListBoxRegion.Location = new System.Drawing.Point(24, 12);
            this.checkedListBoxRegion.Name = "checkedListBoxRegion";
            this.checkedListBoxRegion.Size = new System.Drawing.Size(116, 79);
            this.checkedListBoxRegion.TabIndex = 1;
            // 
            // threads_trackBar
            // 
            this.threads_trackBar.Location = new System.Drawing.Point(156, 12);
            this.threads_trackBar.Maximum = 40;
            this.threads_trackBar.Minimum = 1;
            this.threads_trackBar.Name = "threads_trackBar";
            this.threads_trackBar.Size = new System.Drawing.Size(97, 45);
            this.threads_trackBar.TabIndex = 2;
            this.threads_trackBar.Value = 1;
            this.threads_trackBar.Scroll += new System.EventHandler(this.threads_trackBar_Scroll);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(24, 211);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(219, 19);
            this.progressBar1.TabIndex = 3;
            this.progressBar1.Visible = false;
            // 
            // Message_richTextBox
            // 
            this.Message_richTextBox.Location = new System.Drawing.Point(24, 97);
            this.Message_richTextBox.Name = "Message_richTextBox";
            this.Message_richTextBox.Size = new System.Drawing.Size(447, 100);
            this.Message_richTextBox.TabIndex = 4;
            this.Message_richTextBox.Text = "";
            // 
            // Mainform
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(499, 306);
            this.Controls.Add(this.Message_richTextBox);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.threads_trackBar);
            this.Controls.Add(this.checkedListBoxRegion);
            this.Controls.Add(this.start_button);
            this.Name = "Mainform";
            this.Text = "WG Clan Icon Downloader";
            ((System.ComponentModel.ISupportInitialize)(this.threads_trackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button start_button;
        private System.Windows.Forms.CheckedListBox checkedListBoxRegion;
        private System.Windows.Forms.TrackBar threads_trackBar;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.RichTextBox Message_richTextBox;
    }
}

