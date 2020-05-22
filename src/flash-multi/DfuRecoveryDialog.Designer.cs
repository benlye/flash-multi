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
            resources.ApplyResources(this.dialogText, "dialogText");
            this.dialogText.Name = "dialogText";
            // 
            // buttonCancel
            // 
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.TabStop = false;
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // warningIcon
            // 
            resources.ApplyResources(this.warningIcon, "warningIcon");
            this.warningIcon.Name = "warningIcon";
            this.warningIcon.TabStop = false;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.panel2.Controls.Add(this.timerProgressBar);
            this.panel2.Controls.Add(this.dialogText);
            this.panel2.Controls.Add(this.warningIcon);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // timerProgressBar
            // 
            resources.ApplyResources(this.timerProgressBar, "timerProgressBar");
            this.timerProgressBar.Maximum = 30;
            this.timerProgressBar.Name = "timerProgressBar";
            this.timerProgressBar.Step = 1;
            this.timerProgressBar.Value = 30;
            // 
            // DfuRecoveryDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ControlBox = false;
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.panel2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DfuRecoveryDialog";
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
