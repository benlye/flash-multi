namespace flash_multi
{
    partial class FlashMulti
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FlashMulti));
            this.buttonUpload = new System.Windows.Forms.Button();
            this.comPortSelector = new System.Windows.Forms.ComboBox();
            this.textFileName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.showVerboseOutput = new System.Windows.Forms.CheckBox();
            this.textVerbose = new System.Windows.Forms.TextBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.textActivity = new System.Windows.Forms.TextBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonGo
            // 
            this.buttonUpload.Location = new System.Drawing.Point(373, 71);
            this.buttonUpload.Name = "buttonGo";
            this.buttonUpload.Size = new System.Drawing.Size(75, 23);
            this.buttonUpload.TabIndex = 5;
            this.buttonUpload.Text = "Upload";
            this.buttonUpload.TextImageRelation = System.Windows.Forms.TextImageRelation.TextAboveImage;
            this.buttonUpload.UseVisualStyleBackColor = true;
            this.buttonUpload.Click += new System.EventHandler(this.ButtonUpload_Click);
            // 
            // comPortSelector
            // 
            this.comPortSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comPortSelector.FormattingEnabled = true;
            this.comPortSelector.Location = new System.Drawing.Point(101, 48);
            this.comPortSelector.Name = "comPortSelector";
            this.comPortSelector.Size = new System.Drawing.Size(98, 21);
            this.comPortSelector.TabIndex = 2;
            this.comPortSelector.SelectedIndexChanged += new System.EventHandler(this.ComPortSelector_SelectionChanged);
            // 
            // textFileName
            // 
            this.textFileName.Location = new System.Drawing.Point(101, 21);
            this.textFileName.Name = "textFileName";
            this.textFileName.Size = new System.Drawing.Size(266, 20);
            this.textFileName.TabIndex = 0;
            this.textFileName.DoubleClick += new System.EventHandler(this.ButtonBrowse_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(28, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "File Name:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(28, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Serial Port:";
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Location = new System.Drawing.Point(373, 20);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowse.TabIndex = 1;
            this.buttonBrowse.Text = "Browse";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.ButtonBrowse_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.buttonRefresh);
            this.groupBox1.Controls.Add(this.buttonUpload);
            this.groupBox1.Controls.Add(this.buttonBrowse);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.comPortSelector);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textFileName);
            this.groupBox1.Location = new System.Drawing.Point(15, 37);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(454, 106);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Settings";
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Location = new System.Drawing.Point(205, 47);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(75, 23);
            this.buttonRefresh.TabIndex = 3;
            this.buttonRefresh.Text = "Refresh";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.ButtonRefresh_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(339, 268);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(114, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Show Verbose Output:";
            // 
            // showVerboseOutput
            // 
            this.showVerboseOutput.AutoSize = true;
            this.showVerboseOutput.Location = new System.Drawing.Point(454, 269);
            this.showVerboseOutput.Name = "showVerboseOutput";
            this.showVerboseOutput.Size = new System.Drawing.Size(15, 14);
            this.showVerboseOutput.TabIndex = 6;
            this.showVerboseOutput.UseVisualStyleBackColor = true;
            this.showVerboseOutput.CheckedChanged += new System.EventHandler(this.ShowVerboseOutput_OnChange);
            // 
            // textVerbose
            // 
            this.textVerbose.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.textVerbose.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textVerbose.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textVerbose.Location = new System.Drawing.Point(15, 292);
            this.textVerbose.Multiline = true;
            this.textVerbose.Name = "textVerbose";
            this.textVerbose.ReadOnly = true;
            this.textVerbose.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textVerbose.Size = new System.Drawing.Size(457, 172);
            this.textVerbose.TabIndex = 10;
            this.textVerbose.TabStop = false;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(15, 237);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(454, 23);
            this.progressBar1.TabIndex = 12;
            this.progressBar1.Visible = false;
            // 
            // textActivity
            // 
            this.textActivity.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.textActivity.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textActivity.Location = new System.Drawing.Point(15, 149);
            this.textActivity.Multiline = true;
            this.textActivity.Name = "textActivity";
            this.textActivity.ReadOnly = true;
            this.textActivity.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textActivity.Size = new System.Drawing.Size(454, 111);
            this.textActivity.TabIndex = 13;
            this.textActivity.TabStop = false;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(15, 269);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(182, 13);
            this.linkLabel1.TabIndex = 7;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "https://github.com/benlye/flash-multi";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.RepoLink_LinkClicked);
            // 
            // linkLabel2
            // 
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.LinkArea = new System.Windows.Forms.LinkArea(77, 81);
            this.linkLabel2.Location = new System.Drawing.Point(14, 16);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(408, 17);
            this.linkLabel2.TabIndex = 14;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "Upload firmware to a Multiprotocol TX module.  Get the latest firmware files here" +
    ".";
            this.linkLabel2.UseCompatibleTextRendering = true;
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ReleasesLink_LinkClicked);
            // 
            // FlashMulti
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 291);
            this.Controls.Add(this.linkLabel2);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textActivity);
            this.Controls.Add(this.showVerboseOutput);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.textVerbose);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FlashMulti";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Flash Multi";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonUpload;
        private System.Windows.Forms.ComboBox comPortSelector;
        private System.Windows.Forms.TextBox textFileName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textVerbose;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox showVerboseOutput;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.TextBox textActivity;
        private System.Windows.Forms.Button buttonRefresh;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.LinkLabel linkLabel2;
    }
}

