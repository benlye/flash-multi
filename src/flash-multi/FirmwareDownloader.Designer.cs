namespace Flash_Multi
{
    partial class FirmwareDownloader
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupboxRelease = new System.Windows.Forms.GroupBox();
            this.releaseSelector = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.releaseNotesLink = new System.Windows.Forms.LinkLabel();
            this.label6 = new System.Windows.Forms.Label();
            this.releaseDate = new System.Windows.Forms.Label();
            this.groupboxRelease.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 130);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Module Type:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 175);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Telemetry Type:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 197);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(102, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Telemetry Inversion:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 152);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(78, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Channel Order:";
            // 
            // groupboxRelease
            // 
            this.groupboxRelease.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupboxRelease.Controls.Add(this.releaseDate);
            this.groupboxRelease.Controls.Add(this.label6);
            this.groupboxRelease.Controls.Add(this.releaseNotesLink);
            this.groupboxRelease.Controls.Add(this.label5);
            this.groupboxRelease.Controls.Add(this.releaseSelector);
            this.groupboxRelease.Location = new System.Drawing.Point(15, 13);
            this.groupboxRelease.Name = "groupboxRelease";
            this.groupboxRelease.Size = new System.Drawing.Size(339, 87);
            this.groupboxRelease.TabIndex = 4;
            this.groupboxRelease.TabStop = false;
            this.groupboxRelease.Text = "Firmware Release";
            // 
            // releaseSelector
            // 
            this.releaseSelector.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.releaseSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.releaseSelector.FormattingEnabled = true;
            this.releaseSelector.Location = new System.Drawing.Point(7, 20);
            this.releaseSelector.Name = "releaseSelector";
            this.releaseSelector.Size = new System.Drawing.Size(323, 21);
            this.releaseSelector.TabIndex = 0;
            this.releaseSelector.SelectedIndexChanged += new System.EventHandler(this.releaseSelector_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(75, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Release Date:";
            // 
            // releaseNotesLink
            // 
            this.releaseNotesLink.AutoSize = true;
            this.releaseNotesLink.Location = new System.Drawing.Point(93, 65);
            this.releaseNotesLink.Name = "releaseNotesLink";
            this.releaseNotesLink.Size = new System.Drawing.Size(55, 13);
            this.releaseNotesLink.TabIndex = 2;
            this.releaseNotesLink.TabStop = true;
            this.releaseNotesLink.Text = "linkLabel1";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 65);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(80, 13);
            this.label6.TabIndex = 3;
            this.label6.Text = "Release Notes:";
            // 
            // releaseDate
            // 
            this.releaseDate.AutoSize = true;
            this.releaseDate.Location = new System.Drawing.Point(93, 48);
            this.releaseDate.Name = "releaseDate";
            this.releaseDate.Size = new System.Drawing.Size(64, 13);
            this.releaseDate.TabIndex = 4;
            this.releaseDate.Text = "releaseDate";
            // 
            // FirmwareDownloader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(366, 260);
            this.Controls.Add(this.groupboxRelease);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FirmwareDownloader";
            this.Text = "Download Firmware File";
            this.groupboxRelease.ResumeLayout(false);
            this.groupboxRelease.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupboxRelease;
        private System.Windows.Forms.ComboBox releaseSelector;
        private System.Windows.Forms.Label releaseDate;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.LinkLabel releaseNotesLink;
        private System.Windows.Forms.Label label5;
    }
}