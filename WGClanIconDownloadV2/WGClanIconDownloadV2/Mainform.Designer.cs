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
            this.SuspendLayout();
            // 
            // start_button
            // 
            this.start_button.Location = new System.Drawing.Point(293, 12);
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
            this.checkedListBoxRegion.Location = new System.Drawing.Point(32, 27);
            this.checkedListBoxRegion.Name = "checkedListBoxRegion";
            this.checkedListBoxRegion.Size = new System.Drawing.Size(116, 79);
            this.checkedListBoxRegion.TabIndex = 1;
            // 
            // Mainform
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(499, 306);
            this.Controls.Add(this.checkedListBoxRegion);
            this.Controls.Add(this.start_button);
            this.Name = "Mainform";
            this.Text = "WG Clan Icon Downloader";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button start_button;
        private System.Windows.Forms.CheckedListBox checkedListBoxRegion;
    }
}

