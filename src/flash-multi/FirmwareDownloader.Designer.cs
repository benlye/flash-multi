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
            this.releaseDate = new System.Windows.Forms.Label();
            this.releaseNotesLink = new System.Windows.Forms.LinkLabel();
            this.label5 = new System.Windows.Forms.Label();
            this.releaseSelector = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxTelemetryInversion = new System.Windows.Forms.CheckBox();
            this.radioTypeSelector = new System.Windows.Forms.ComboBox();
            this.channelOrderSelector = new System.Windows.Forms.ComboBox();
            this.moduleTypeSelector = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.groupboxRelease.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(47, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Module Type:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(54, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Radio Type:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(17, 107);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(102, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Telemetry Inversion:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(41, 80);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(78, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Channel Order:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // groupboxRelease
            // 
            this.groupboxRelease.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupboxRelease.Controls.Add(this.releaseDate);
            this.groupboxRelease.Controls.Add(this.releaseNotesLink);
            this.groupboxRelease.Controls.Add(this.label5);
            this.groupboxRelease.Controls.Add(this.releaseSelector);
            this.groupboxRelease.Location = new System.Drawing.Point(12, 12);
            this.groupboxRelease.Name = "groupboxRelease";
            this.groupboxRelease.Size = new System.Drawing.Size(282, 70);
            this.groupboxRelease.TabIndex = 4;
            this.groupboxRelease.TabStop = false;
            this.groupboxRelease.Text = "Firmware Release";
            // 
            // releaseDate
            // 
            this.releaseDate.AutoSize = true;
            this.releaseDate.Location = new System.Drawing.Point(89, 48);
            this.releaseDate.Name = "releaseDate";
            this.releaseDate.Size = new System.Drawing.Size(64, 13);
            this.releaseDate.TabIndex = 4;
            this.releaseDate.Text = "releaseDate";
            // 
            // releaseNotesLink
            // 
            this.releaseNotesLink.AutoSize = true;
            this.releaseNotesLink.Location = new System.Drawing.Point(197, 48);
            this.releaseNotesLink.Name = "releaseNotesLink";
            this.releaseNotesLink.Size = new System.Drawing.Size(77, 13);
            this.releaseNotesLink.TabIndex = 2;
            this.releaseNotesLink.TabStop = true;
            this.releaseNotesLink.Text = "Release Notes";
            this.releaseNotesLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ReleaseNotesLink_LinkClicked);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(75, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Release Date:";
            // 
            // releaseSelector
            // 
            this.releaseSelector.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.releaseSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.releaseSelector.FormattingEnabled = true;
            this.releaseSelector.Location = new System.Drawing.Point(14, 20);
            this.releaseSelector.Name = "releaseSelector";
            this.releaseSelector.Size = new System.Drawing.Size(259, 21);
            this.releaseSelector.TabIndex = 0;
            this.releaseSelector.SelectedIndexChanged += new System.EventHandler(this.releaseSelector_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.checkBoxTelemetryInversion);
            this.groupBox1.Controls.Add(this.radioTypeSelector);
            this.groupBox1.Controls.Add(this.channelOrderSelector);
            this.groupBox1.Controls.Add(this.moduleTypeSelector);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(12, 88);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(282, 139);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Firmware Attributes";
            // 
            // checkBoxTelemetryInversion
            // 
            this.checkBoxTelemetryInversion.AutoSize = true;
            this.checkBoxTelemetryInversion.Location = new System.Drawing.Point(126, 106);
            this.checkBoxTelemetryInversion.Name = "checkBoxTelemetryInversion";
            this.checkBoxTelemetryInversion.Size = new System.Drawing.Size(15, 14);
            this.checkBoxTelemetryInversion.TabIndex = 7;
            this.checkBoxTelemetryInversion.UseVisualStyleBackColor = true;
            this.checkBoxTelemetryInversion.CheckedChanged += new System.EventHandler(this.checkBoxTelemetryInversion_CheckedChanged);
            // 
            // radioTypeSelector
            // 
            this.radioTypeSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.radioTypeSelector.FormattingEnabled = true;
            this.radioTypeSelector.Items.AddRange(new object[] {
            "erSkyTX",
            "OpenTX",
            "PPM"});
            this.radioTypeSelector.Location = new System.Drawing.Point(126, 49);
            this.radioTypeSelector.Name = "radioTypeSelector";
            this.radioTypeSelector.Size = new System.Drawing.Size(121, 21);
            this.radioTypeSelector.TabIndex = 6;
            this.radioTypeSelector.SelectedIndexChanged += new System.EventHandler(this.multiTelemetrySelector_SelectedIndexChanged);
            // 
            // channelOrderSelector
            // 
            this.channelOrderSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.channelOrderSelector.FormattingEnabled = true;
            this.channelOrderSelector.Items.AddRange(new object[] {
            "AETR",
            "TAER",
            "RETA"});
            this.channelOrderSelector.Location = new System.Drawing.Point(126, 76);
            this.channelOrderSelector.Name = "channelOrderSelector";
            this.channelOrderSelector.Size = new System.Drawing.Size(121, 21);
            this.channelOrderSelector.TabIndex = 5;
            this.channelOrderSelector.SelectedIndexChanged += new System.EventHandler(this.channelOrderSelector_SelectedIndexChanged);
            // 
            // moduleTypeSelector
            // 
            this.moduleTypeSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.moduleTypeSelector.FormattingEnabled = true;
            this.moduleTypeSelector.Items.AddRange(new object[] {
            "STM32"});
            this.moduleTypeSelector.Location = new System.Drawing.Point(126, 22);
            this.moduleTypeSelector.Name = "moduleTypeSelector";
            this.moduleTypeSelector.Size = new System.Drawing.Size(121, 21);
            this.moduleTypeSelector.TabIndex = 4;
            this.moduleTypeSelector.SelectedIndexChanged += new System.EventHandler(this.moduleTypeSelector_SelectedIndexChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.comboBox1);
            this.groupBox2.Location = new System.Drawing.Point(12, 233);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(282, 56);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Firmware File";
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(14, 19);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(258, 21);
            this.comboBox1.TabIndex = 5;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(219, 299);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "Download";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // FirmwareDownloader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(309, 334);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupboxRelease);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FirmwareDownloader";
            this.Text = "Download Firmware File";
            this.groupboxRelease.ResumeLayout(false);
            this.groupboxRelease.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupboxRelease;
        private System.Windows.Forms.ComboBox releaseSelector;
        private System.Windows.Forms.Label releaseDate;
        private System.Windows.Forms.LinkLabel releaseNotesLink;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox checkBoxTelemetryInversion;
        private System.Windows.Forms.ComboBox radioTypeSelector;
        private System.Windows.Forms.ComboBox channelOrderSelector;
        private System.Windows.Forms.ComboBox moduleTypeSelector;
    }
}