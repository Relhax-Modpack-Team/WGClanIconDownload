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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Mainform));
            this.start_button = new System.Windows.Forms.Button();
            this.checkedListBoxRegion = new System.Windows.Forms.CheckedListBox();
            this.threads_trackBar = new System.Windows.Forms.TrackBar();
            this.Message_richTextBox = new System.Windows.Forms.RichTextBox();
            this.avgOverTimeTicksLabel = new System.Windows.Forms.Label();
            this.UiThreadsAllowed_label = new System.Windows.Forms.Label();
            this.avgOverTimeTicksLabel_toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.overallTickLabel = new System.Windows.Forms.Label();
            this.separatorBevelLineLabel = new System.Windows.Forms.Label();
            this.overallTickLabel_toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.threads_trackBar_toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.cancel_button = new System.Windows.Forms.Button();
            this.regionThreadsLabel_toolTip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.threads_trackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // start_button
            // 
            this.start_button.Enabled = false;
            this.start_button.Location = new System.Drawing.Point(374, 12);
            this.start_button.Name = "start_button";
            this.start_button.Size = new System.Drawing.Size(97, 26);
            this.start_button.TabIndex = 0;
            this.start_button.Text = "Start";
            this.start_button.UseVisualStyleBackColor = true;
            this.start_button.Click += new System.EventHandler(this.start_button_Click);
            // 
            // checkedListBoxRegion
            // 
            this.checkedListBoxRegion.BackColor = System.Drawing.SystemColors.Control;
            this.checkedListBoxRegion.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.checkedListBoxRegion.CheckOnClick = true;
            this.checkedListBoxRegion.FormattingEnabled = true;
            this.checkedListBoxRegion.Location = new System.Drawing.Point(24, 12);
            this.checkedListBoxRegion.Name = "checkedListBoxRegion";
            this.checkedListBoxRegion.Size = new System.Drawing.Size(116, 75);
            this.checkedListBoxRegion.Sorted = true;
            this.checkedListBoxRegion.TabIndex = 1;
            this.checkedListBoxRegion.ThreeDCheckBoxes = true;
            this.checkedListBoxRegion.SelectedValueChanged += new System.EventHandler(this.checkedListBoxRegion_SelectedValueChanged);
            // 
            // threads_trackBar
            // 
            this.threads_trackBar.Location = new System.Drawing.Point(146, 12);
            this.threads_trackBar.Maximum = 100;
            this.threads_trackBar.Minimum = 1;
            this.threads_trackBar.Name = "threads_trackBar";
            this.threads_trackBar.Size = new System.Drawing.Size(97, 45);
            this.threads_trackBar.TabIndex = 2;
            this.threads_trackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.threads_trackBar_toolTip.SetToolTip(this.threads_trackBar, resources.GetString("threads_trackBar.ToolTip"));
            this.threads_trackBar.Value = 1;
            this.threads_trackBar.Scroll += new System.EventHandler(this.threads_trackBar_Scroll);
            // 
            // Message_richTextBox
            // 
            this.Message_richTextBox.Location = new System.Drawing.Point(24, 97);
            this.Message_richTextBox.Name = "Message_richTextBox";
            this.Message_richTextBox.Size = new System.Drawing.Size(447, 114);
            this.Message_richTextBox.TabIndex = 4;
            this.Message_richTextBox.Text = "";
            this.Message_richTextBox.TextChanged += new System.EventHandler(this.Message_richTextBox_TextChanged);
            // 
            // avgOverTimeTicksLabel
            // 
            this.avgOverTimeTicksLabel.AutoSize = true;
            this.avgOverTimeTicksLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.avgOverTimeTicksLabel.Location = new System.Drawing.Point(281, 75);
            this.avgOverTimeTicksLabel.Name = "avgOverTimeTicksLabel";
            this.avgOverTimeTicksLabel.Size = new System.Drawing.Size(73, 17);
            this.avgOverTimeTicksLabel.TabIndex = 6;
            this.avgOverTimeTicksLabel.Text = "ø dl/sec: 0";
            this.avgOverTimeTicksLabel_toolTip.SetToolTip(this.avgOverTimeTicksLabel, "This is the average download of Clan icons at the last XX seconds.");
            // 
            // UiThreadsAllowed_label
            // 
            this.UiThreadsAllowed_label.AutoSize = true;
            this.UiThreadsAllowed_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UiThreadsAllowed_label.Location = new System.Drawing.Point(158, 40);
            this.UiThreadsAllowed_label.Name = "UiThreadsAllowed_label";
            this.UiThreadsAllowed_label.Size = new System.Drawing.Size(30, 17);
            this.UiThreadsAllowed_label.TabIndex = 7;
            this.UiThreadsAllowed_label.Text = "x10";
            this.threads_trackBar_toolTip.SetToolTip(this.UiThreadsAllowed_label, resources.GetString("UiThreadsAllowed_label.ToolTip"));
            // 
            // avgOverTimeTicksLabel_toolTip
            // 
            this.avgOverTimeTicksLabel_toolTip.AutoPopDelay = 10000;
            this.avgOverTimeTicksLabel_toolTip.InitialDelay = 500;
            this.avgOverTimeTicksLabel_toolTip.ReshowDelay = 100;
            this.avgOverTimeTicksLabel_toolTip.ShowAlways = true;
            this.avgOverTimeTicksLabel_toolTip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.avgOverTimeTicksLabel_toolTip.ToolTipTitle = "average download over time";
            // 
            // overallTickLabel
            // 
            this.overallTickLabel.AutoSize = true;
            this.overallTickLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.overallTickLabel.Location = new System.Drawing.Point(281, 215);
            this.overallTickLabel.Name = "overallTickLabel";
            this.overallTickLabel.Size = new System.Drawing.Size(75, 17);
            this.overallTickLabel.TabIndex = 8;
            this.overallTickLabel.Text = "∑ dl/sec: 0";
            this.overallTickLabel_toolTip.SetToolTip(this.overallTickLabel, "this is the sum of all downloaded icons in a second");
            this.overallTickLabel.Visible = false;
            // 
            // separatorBevelLineLabel
            // 
            this.separatorBevelLineLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.separatorBevelLineLabel.Location = new System.Drawing.Point(281, 210);
            this.separatorBevelLineLabel.Name = "separatorBevelLineLabel";
            this.separatorBevelLineLabel.Size = new System.Drawing.Size(95, 2);
            this.separatorBevelLineLabel.TabIndex = 9;
            this.separatorBevelLineLabel.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.separatorBevelLineLabel.Visible = false;
            // 
            // overallTickLabel_toolTip
            // 
            this.overallTickLabel_toolTip.AutoPopDelay = 10000;
            this.overallTickLabel_toolTip.InitialDelay = 500;
            this.overallTickLabel_toolTip.ReshowDelay = 100;
            this.overallTickLabel_toolTip.ShowAlways = true;
            this.overallTickLabel_toolTip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.overallTickLabel_toolTip.ToolTipTitle = "sum of current downloads";
            // 
            // threads_trackBar_toolTip
            // 
            this.threads_trackBar_toolTip.AutoPopDelay = 10000;
            this.threads_trackBar_toolTip.InitialDelay = 500;
            this.threads_trackBar_toolTip.IsBalloon = true;
            this.threads_trackBar_toolTip.ReshowDelay = 100;
            this.threads_trackBar_toolTip.ShowAlways = true;
            this.threads_trackBar_toolTip.StripAmpersands = true;
            this.threads_trackBar_toolTip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.threads_trackBar_toolTip.ToolTipTitle = "simultaneous downloads";
            // 
            // cancel_button
            // 
            this.cancel_button.Location = new System.Drawing.Point(374, 44);
            this.cancel_button.Name = "cancel_button";
            this.cancel_button.Size = new System.Drawing.Size(97, 26);
            this.cancel_button.TabIndex = 10;
            this.cancel_button.Text = "Quit";
            this.cancel_button.UseVisualStyleBackColor = true;
            this.cancel_button.Click += new System.EventHandler(this.cancel_button_Click);
            // 
            // regionThreadsLabel_toolTip
            // 
            this.regionThreadsLabel_toolTip.AutoPopDelay = 10000;
            this.regionThreadsLabel_toolTip.InitialDelay = 500;
            this.regionThreadsLabel_toolTip.ReshowDelay = 100;
            this.regionThreadsLabel_toolTip.ShowAlways = true;
            this.regionThreadsLabel_toolTip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.regionThreadsLabel_toolTip.ToolTipTitle = "simultaneous downloads";
            // 
            // Mainform
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(499, 285);
            this.Controls.Add(this.cancel_button);
            this.Controls.Add(this.separatorBevelLineLabel);
            this.Controls.Add(this.overallTickLabel);
            this.Controls.Add(this.UiThreadsAllowed_label);
            this.Controls.Add(this.avgOverTimeTicksLabel);
            this.Controls.Add(this.Message_richTextBox);
            this.Controls.Add(this.threads_trackBar);
            this.Controls.Add(this.checkedListBoxRegion);
            this.Controls.Add(this.start_button);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
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
        private System.Windows.Forms.RichTextBox Message_richTextBox;
        private System.Windows.Forms.Label avgOverTimeTicksLabel;
        private System.Windows.Forms.Label UiThreadsAllowed_label;
        private System.Windows.Forms.ToolTip avgOverTimeTicksLabel_toolTip;
        private System.Windows.Forms.Label overallTickLabel;
        private System.Windows.Forms.Label separatorBevelLineLabel;
        private System.Windows.Forms.ToolTip overallTickLabel_toolTip;
        private System.Windows.Forms.ToolTip threads_trackBar_toolTip;
        private System.Windows.Forms.Button cancel_button;
        private System.Windows.Forms.ToolTip regionThreadsLabel_toolTip;
    }
}

