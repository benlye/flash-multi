// -------------------------------------------------------------------------------
// <copyright file="DfuRecoveryDialog.cs" company="Ben Lye">
// Copyright 2019 Ben Lye
//
// This file is part of Flash Multi.
//
// Flash Multi is free software: you can redistribute it and/or modify it under
// the terms of the GNU General Public License as published by the Free Software
// Foundation, either version 3 of the License, or(at your option) any later
// version.
//
// Flash Multi is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with
// Flash Multi. If not, see http://www.gnu.org/licenses/.
// </copyright>
// -------------------------------------------------------------------------------

namespace Flash_Multi
{
    partial class DfuRecoveryDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DfuRecoveryDialog));
            this.dialogText = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.warningIcon = new System.Windows.Forms.PictureBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.timerProgressBar = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.warningIcon)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // dialogText
            // 
            this.dialogText.Location = new System.Drawing.Point(54, 13);
            this.dialogText.Name = "dialogText";
            this.dialogText.Size = new System.Drawing.Size(306, 84);
            this.dialogText.TabIndex = 0;
            this.dialogText.Text = "Could not find a MULTI-Module in DFU mode.  \r\n\r\nPlease unplug and then re" +
    "-plug your module.\r\n\r\nFlashing will continue automatically when the module is pl" +
    "ugged back in.";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(320, 154);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(67, 24);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.TabStop = false;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // warningIcon
            // 
            this.warningIcon.Image = ((System.Drawing.Image)(resources.GetObject("warningIcon.Image")));
            this.warningIcon.Location = new System.Drawing.Point(19, 13);
            this.warningIcon.Name = "warningIcon";
            this.warningIcon.Size = new System.Drawing.Size(30, 34);
            this.warningIcon.TabIndex = 2;
            this.warningIcon.TabStop = false;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.panel2.Controls.Add(this.timerProgressBar);
            this.panel2.Controls.Add(this.dialogText);
            this.panel2.Controls.Add(this.warningIcon);
            this.panel2.Location = new System.Drawing.Point(-2, -1);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(405, 147);
            this.panel2.TabIndex = 4;
            // 
            // timerProgressBar
            // 
            this.timerProgressBar.Location = new System.Drawing.Point(58, 100);
            this.timerProgressBar.Maximum = 30;
            this.timerProgressBar.Name = "timerProgressBar";
            this.timerProgressBar.Size = new System.Drawing.Size(302, 23);
            this.timerProgressBar.Step = 1;
            this.timerProgressBar.TabIndex = 3;
            this.timerProgressBar.Value = 30;
            // 
            // DfuRecoveryDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(404, 187);
            this.ControlBox = false;
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.panel2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DfuRecoveryDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DFU Recovery Mode";
            ((System.ComponentModel.ISupportInitialize)(this.warningIcon)).EndInit();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label dialogText;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.PictureBox warningIcon;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ProgressBar timerProgressBar;
    }
}
