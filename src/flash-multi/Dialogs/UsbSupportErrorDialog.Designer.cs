namespace Flash_Multi
{
    partial class UsbSupportErrorDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UsbSupportErrorDialog));
            this.panel2 = new System.Windows.Forms.Panel();
            this.moreInfoLinkLabel = new System.Windows.Forms.LinkLabel();
            this.dialogText = new System.Windows.Forms.Label();
            this.errorIcon = new System.Windows.Forms.PictureBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.panel2.Controls.Add(this.moreInfoLinkLabel);
            this.panel2.Controls.Add(this.dialogText);
            this.panel2.Controls.Add(this.errorIcon);
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(405, 147);
            this.panel2.TabIndex = 6;
            // 
            // moreInfoLinkLabel
            // 
            this.moreInfoLinkLabel.AutoSize = true;
            this.moreInfoLinkLabel.LinkArea = new System.Windows.Forms.LinkArea(6, 4);
            this.moreInfoLinkLabel.Location = new System.Drawing.Point(57, 114);
            this.moreInfoLinkLabel.Name = "moreInfoLinkLabel";
            this.moreInfoLinkLabel.Size = new System.Drawing.Size(178, 21);
            this.moreInfoLinkLabel.TabIndex = 3;
            this.moreInfoLinkLabel.TabStop = true;
            this.moreInfoLinkLabel.Text = "Click here for more information.";
            this.moreInfoLinkLabel.UseCompatibleTextRendering = true;
            this.moreInfoLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.MoreInfoLinkLabel_LinkClicked);
            // 
            // dialogText
            // 
            this.dialogText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.dialogText.Location = new System.Drawing.Point(54, 13);
            this.dialogText.Name = "dialogText";
            this.dialogText.Size = new System.Drawing.Size(306, 101);
            this.dialogText.TabIndex = 0;
            this.dialogText.Text = resources.GetString("dialogText.Text");
            // 
            // errorIcon
            // 
            this.errorIcon.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.errorIcon.Location = new System.Drawing.Point(13, 13);
            this.errorIcon.Name = "errorIcon";
            this.errorIcon.Size = new System.Drawing.Size(34, 34);
            this.errorIcon.TabIndex = 2;
            this.errorIcon.TabStop = false;
            // 
            // buttonOK
            // 
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(325, 153);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(67, 24);
            this.buttonOK.TabIndex = 7;
            this.buttonOK.TabStop = false;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // UsbSupportErrorDialog
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(404, 187);
            this.ControlBox = false;
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.panel2);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UsbSupportErrorDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "USB Support Error";
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorIcon)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label dialogText;
        private System.Windows.Forms.PictureBox errorIcon;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.LinkLabel moreInfoLinkLabel;
    }
}