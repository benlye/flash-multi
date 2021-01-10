namespace Flash_Multi
{
    partial class UsbSupportWarningDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UsbSupportWarningDialog));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.dialogText = new System.Windows.Forms.Label();
            this.warningIcon = new System.Windows.Forms.PictureBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.disableUsbWarning = new System.Windows.Forms.CheckBox();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.warningIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(322, 121);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(67, 24);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.panel2.Controls.Add(this.dialogText);
            this.panel2.Controls.Add(this.warningIcon);
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(405, 113);
            this.panel2.TabIndex = 6;
            // 
            // dialogText
            // 
            this.dialogText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.dialogText.Location = new System.Drawing.Point(54, 13);
            this.dialogText.Name = "dialogText";
            this.dialogText.Size = new System.Drawing.Size(306, 96);
            this.dialogText.TabIndex = 0;
            this.dialogText.Text = "The selected firmware file was compiled without USB serial support.  The MULTI-Mo" +
    "dule bootloader should be updated before writing this firmware.\r\n\r\nClick OK to w" +
    "rite the firmware.";
            // 
            // warningIcon
            // 
            this.warningIcon.Image = ((System.Drawing.Image)(resources.GetObject("warningIcon.Image")));
            this.warningIcon.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.warningIcon.Location = new System.Drawing.Point(13, 13);
            this.warningIcon.Name = "warningIcon";
            this.warningIcon.Size = new System.Drawing.Size(34, 34);
            this.warningIcon.TabIndex = 2;
            this.warningIcon.TabStop = false;
            // 
            // buttonOK
            // 
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(249, 121);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(67, 24);
            this.buttonOK.TabIndex = 5;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // disableUsbWarning
            // 
            this.disableUsbWarning.AutoSize = true;
            this.disableUsbWarning.Location = new System.Drawing.Point(13, 121);
            this.disableUsbWarning.Name = "disableUsbWarning";
            this.disableUsbWarning.Size = new System.Drawing.Size(196, 19);
            this.disableUsbWarning.TabIndex = 4;
            this.disableUsbWarning.Text = "Do not show this message again";
            this.disableUsbWarning.UseVisualStyleBackColor = true;
            // 
            // UsbSupportWarningDialog
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(404, 152);
            this.ControlBox = false;
            this.Controls.Add(this.disableUsbWarning);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.panel2);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UsbSupportWarningDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "USB Support Warning";
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.warningIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label dialogText;
        private System.Windows.Forms.PictureBox warningIcon;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.CheckBox disableUsbWarning;
    }
}