namespace Flash_Multi
{
    partial class UsbSupportWarning
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UsbSupportWarning));
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
            this.buttonCancel.Location = new System.Drawing.Point(322, 157);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(67, 24);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.TabStop = false;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.panel2.Controls.Add(this.dialogText);
            this.panel2.Controls.Add(this.warningIcon);
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(405, 147);
            this.panel2.TabIndex = 6;
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
            // warningIcon
            // 
            this.warningIcon.Image = ((System.Drawing.Image)(resources.GetObject("warningIcon.Image")));
            this.warningIcon.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.warningIcon.Location = new System.Drawing.Point(19, 13);
            this.warningIcon.Name = "warningIcon";
            this.warningIcon.Size = new System.Drawing.Size(30, 34);
            this.warningIcon.TabIndex = 2;
            this.warningIcon.TabStop = false;
            // 
            // buttonOK
            // 
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(249, 157);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(67, 24);
            this.buttonOK.TabIndex = 7;
            this.buttonOK.TabStop = false;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // disableUsbWarning
            // 
            this.disableUsbWarning.AutoSize = true;
            this.disableUsbWarning.Location = new System.Drawing.Point(57, 160);
            this.disableUsbWarning.Name = "disableUsbWarning";
            this.disableUsbWarning.Size = new System.Drawing.Size(179, 17);
            this.disableUsbWarning.TabIndex = 3;
            this.disableUsbWarning.Text = "Do not show this message again";
            this.disableUsbWarning.UseVisualStyleBackColor = true;
            // 
            // UsbSupportWarning
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(404, 187);
            this.ControlBox = false;
            this.Controls.Add(this.disableUsbWarning);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.panel2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UsbSupportWarning";
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