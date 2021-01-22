// -------------------------------------------------------------------------------
// <copyright file="FlashMulti.Designer.cs" company="Ben Lye">
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FlashMulti));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.textActivity = new System.Windows.Forms.TextBox();
            this.showVerboseOutput = new System.Windows.Forms.CheckBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.textVerbose = new System.Windows.Forms.TextBox();
            this.buttonUpload = new System.Windows.Forms.Button();
            this.comPortSelector = new System.Windows.Forms.ComboBox();
            this.textFileName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.buttonSaveBackup = new System.Windows.Forms.Button();
            this.buttonRead = new System.Windows.Forms.Button();
            this.buttonSerialMonitor = new System.Windows.Forms.Button();
            this.buttonErase = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.menuStrip2 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.advancedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.baudRateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemBaudRate57600 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemBaudRate115200 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemBaudRate500000 = new System.Windows.Forms.ToolStripMenuItem();
            this.bootloaderTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stickyDfuUsbModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.comPortUsbModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disableCompatibilityCheckToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableDeviceDetectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runAfterUploadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.actionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.installUSBDriversToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetToDFUModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.upgradeBootloaderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runFirmwareToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkForUpdateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.documentationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.downloadFirmwareToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.menuStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer1.Panel2.Controls.Add(this.textVerbose);
            resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.textActivity, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.showVerboseOutput, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.progressBar1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.linkLabel1, 0, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // textActivity
            // 
            this.textActivity.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.tableLayoutPanel1.SetColumnSpan(this.textActivity, 2);
            resources.ApplyResources(this.textActivity, "textActivity");
            this.textActivity.Name = "textActivity";
            this.textActivity.ReadOnly = true;
            this.textActivity.TabStop = false;
            // 
            // showVerboseOutput
            // 
            resources.ApplyResources(this.showVerboseOutput, "showVerboseOutput");
            this.showVerboseOutput.Name = "showVerboseOutput";
            this.showVerboseOutput.UseVisualStyleBackColor = true;
            this.showVerboseOutput.CheckedChanged += new System.EventHandler(this.ShowVerboseOutput_OnChange);
            // 
            // progressBar1
            // 
            resources.ApplyResources(this.progressBar1, "progressBar1");
            this.tableLayoutPanel1.SetColumnSpan(this.progressBar1, 2);
            this.progressBar1.Maximum = 1000;
            this.progressBar1.Name = "progressBar1";
            // 
            // linkLabel1
            // 
            resources.ApplyResources(this.linkLabel1, "linkLabel1");
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.TabStop = true;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.RepoLink_LinkClicked);
            // 
            // textVerbose
            // 
            this.textVerbose.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.textVerbose.Cursor = System.Windows.Forms.Cursors.IBeam;
            resources.ApplyResources(this.textVerbose, "textVerbose");
            this.textVerbose.Name = "textVerbose";
            this.textVerbose.ReadOnly = true;
            this.textVerbose.TabStop = false;
            // 
            // buttonUpload
            // 
            resources.ApplyResources(this.buttonUpload, "buttonUpload");
            this.buttonUpload.Name = "buttonUpload";
            this.buttonUpload.UseVisualStyleBackColor = true;
            this.buttonUpload.Click += new System.EventHandler(this.ButtonWrite_Click);
            // 
            // comPortSelector
            // 
            this.comPortSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comPortSelector.DropDownWidth = 90;
            this.comPortSelector.FormattingEnabled = true;
            resources.ApplyResources(this.comPortSelector, "comPortSelector");
            this.comPortSelector.Name = "comPortSelector";
            this.comPortSelector.SelectedIndexChanged += new System.EventHandler(this.ComPortSelector_SelectionChanged);
            // 
            // textFileName
            // 
            resources.ApplyResources(this.textFileName, "textFileName");
            this.textFileName.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.textFileName.Name = "textFileName";
            this.textFileName.ReadOnly = true;
            this.textFileName.DoubleClick += new System.EventHandler(this.ButtonBrowse_Click);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // buttonBrowse
            // 
            resources.ApplyResources(this.buttonBrowse, "buttonBrowse");
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.ButtonBrowse_Click);
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.buttonRefresh);
            this.groupBox1.Controls.Add(this.buttonBrowse);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.comPortSelector);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textFileName);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // buttonRefresh
            // 
            resources.ApplyResources(this.buttonRefresh, "buttonRefresh");
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.ButtonRefresh_Click);
            // 
            // buttonSaveBackup
            // 
            resources.ApplyResources(this.buttonSaveBackup, "buttonSaveBackup");
            this.buttonSaveBackup.Name = "buttonSaveBackup";
            this.buttonSaveBackup.UseVisualStyleBackColor = true;
            this.buttonSaveBackup.Click += new System.EventHandler(this.ButtonSaveBackup_Click);
            // 
            // buttonRead
            // 
            resources.ApplyResources(this.buttonRead, "buttonRead");
            this.buttonRead.Name = "buttonRead";
            this.buttonRead.UseVisualStyleBackColor = true;
            this.buttonRead.Click += new System.EventHandler(this.ButtonRead_Click);
            // 
            // buttonSerialMonitor
            // 
            resources.ApplyResources(this.buttonSerialMonitor, "buttonSerialMonitor");
            this.buttonSerialMonitor.Name = "buttonSerialMonitor";
            this.buttonSerialMonitor.UseVisualStyleBackColor = true;
            this.buttonSerialMonitor.Click += new System.EventHandler(this.ButtonSerialMonitor_Click);
            // 
            // buttonErase
            // 
            resources.ApplyResources(this.buttonErase, "buttonErase");
            this.buttonErase.Name = "buttonErase";
            this.buttonErase.UseVisualStyleBackColor = true;
            this.buttonErase.Click += new System.EventHandler(this.ButtonErase_Click);
            // 
            // menuStrip2
            // 
            this.menuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.advancedToolStripMenuItem,
            this.helpToolStripMenuItem});
            resources.ApplyResources(this.menuStrip2, "menuStrip2");
            this.menuStrip2.Name = "menuStrip2";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // advancedToolStripMenuItem
            // 
            this.advancedToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem,
            this.actionsToolStripMenuItem});
            this.advancedToolStripMenuItem.Name = "advancedToolStripMenuItem";
            resources.ApplyResources(this.advancedToolStripMenuItem, "advancedToolStripMenuItem");
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.baudRateToolStripMenuItem,
            this.bootloaderTypeToolStripMenuItem,
            this.disableCompatibilityCheckToolStripMenuItem,
            this.enableDeviceDetectionToolStripMenuItem,
            this.runAfterUploadToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            resources.ApplyResources(this.settingsToolStripMenuItem, "settingsToolStripMenuItem");
            // 
            // baudRateToolStripMenuItem
            // 
            this.baudRateToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemBaudRate57600,
            this.toolStripMenuItemBaudRate115200,
            this.toolStripMenuItemBaudRate500000});
            this.baudRateToolStripMenuItem.Name = "baudRateToolStripMenuItem";
            resources.ApplyResources(this.baudRateToolStripMenuItem, "baudRateToolStripMenuItem");
            // 
            // toolStripMenuItemBaudRate57600
            // 
            this.toolStripMenuItemBaudRate57600.Name = "toolStripMenuItemBaudRate57600";
            resources.ApplyResources(this.toolStripMenuItemBaudRate57600, "toolStripMenuItemBaudRate57600");
            this.toolStripMenuItemBaudRate57600.Click += new System.EventHandler(this.ToolStripMenuItemBaudRate57600_Click);
            // 
            // toolStripMenuItemBaudRate115200
            // 
            this.toolStripMenuItemBaudRate115200.Name = "toolStripMenuItemBaudRate115200";
            resources.ApplyResources(this.toolStripMenuItemBaudRate115200, "toolStripMenuItemBaudRate115200");
            this.toolStripMenuItemBaudRate115200.Click += new System.EventHandler(this.ToolStripMenuItemBaudRate115200_Click);
            // 
            // toolStripMenuItemBaudRate500000
            // 
            this.toolStripMenuItemBaudRate500000.Name = "toolStripMenuItemBaudRate500000";
            resources.ApplyResources(this.toolStripMenuItemBaudRate500000, "toolStripMenuItemBaudRate500000");
            this.toolStripMenuItemBaudRate500000.Click += new System.EventHandler(this.ToolStripMenuItemBaudRate500000_Click);
            // 
            // bootloaderTypeToolStripMenuItem
            // 
            this.bootloaderTypeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stickyDfuUsbModeToolStripMenuItem,
            this.comPortUsbModeToolStripMenuItem});
            this.bootloaderTypeToolStripMenuItem.Name = "bootloaderTypeToolStripMenuItem";
            resources.ApplyResources(this.bootloaderTypeToolStripMenuItem, "bootloaderTypeToolStripMenuItem");
            // 
            // stickyDfuUsbModeToolStripMenuItem
            // 
            this.stickyDfuUsbModeToolStripMenuItem.Name = "stickyDfuUsbModeToolStripMenuItem";
            resources.ApplyResources(this.stickyDfuUsbModeToolStripMenuItem, "stickyDfuUsbModeToolStripMenuItem");
            this.stickyDfuUsbModeToolStripMenuItem.Click += new System.EventHandler(this.StickyDfuUsbModeToolStripMenuItem_Click);
            // 
            // comPortUsbModeToolStripMenuItem
            // 
            this.comPortUsbModeToolStripMenuItem.Name = "comPortUsbModeToolStripMenuItem";
            resources.ApplyResources(this.comPortUsbModeToolStripMenuItem, "comPortUsbModeToolStripMenuItem");
            this.comPortUsbModeToolStripMenuItem.Click += new System.EventHandler(this.ComPortUsbModeToolStripMenuItem_Click);
            // 
            // disableCompatibilityCheckToolStripMenuItem
            // 
            this.disableCompatibilityCheckToolStripMenuItem.CheckOnClick = true;
            this.disableCompatibilityCheckToolStripMenuItem.Name = "disableCompatibilityCheckToolStripMenuItem";
            resources.ApplyResources(this.disableCompatibilityCheckToolStripMenuItem, "disableCompatibilityCheckToolStripMenuItem");
            this.disableCompatibilityCheckToolStripMenuItem.Click += new System.EventHandler(this.DisableCompatibilityCheckToolStripMenuItem_Click);
            // 
            // enableDeviceDetectionToolStripMenuItem
            // 
            this.enableDeviceDetectionToolStripMenuItem.CheckOnClick = true;
            this.enableDeviceDetectionToolStripMenuItem.Name = "enableDeviceDetectionToolStripMenuItem";
            resources.ApplyResources(this.enableDeviceDetectionToolStripMenuItem, "enableDeviceDetectionToolStripMenuItem");
            this.enableDeviceDetectionToolStripMenuItem.Click += new System.EventHandler(this.EnableDeviceDetectionToolStripMenuItem_Click);
            // 
            // runAfterUploadToolStripMenuItem
            // 
            this.runAfterUploadToolStripMenuItem.CheckOnClick = true;
            this.runAfterUploadToolStripMenuItem.Name = "runAfterUploadToolStripMenuItem";
            resources.ApplyResources(this.runAfterUploadToolStripMenuItem, "runAfterUploadToolStripMenuItem");
            this.runAfterUploadToolStripMenuItem.Click += new System.EventHandler(this.RunAfterUploadToolStripMenuItem_Click);
            // 
            // actionsToolStripMenuItem
            // 
            this.actionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.installUSBDriversToolStripMenuItem,
            this.resetToDFUModeToolStripMenuItem,
            this.upgradeBootloaderToolStripMenuItem,
            this.runFirmwareToolStripMenuItem});
            this.actionsToolStripMenuItem.Name = "actionsToolStripMenuItem";
            resources.ApplyResources(this.actionsToolStripMenuItem, "actionsToolStripMenuItem");
            // 
            // installUSBDriversToolStripMenuItem
            // 
            this.installUSBDriversToolStripMenuItem.Name = "installUSBDriversToolStripMenuItem";
            resources.ApplyResources(this.installUSBDriversToolStripMenuItem, "installUSBDriversToolStripMenuItem");
            this.installUSBDriversToolStripMenuItem.Click += new System.EventHandler(this.InstallUSBDriversToolStripMenuItem_Click);
            // 
            // resetToDFUModeToolStripMenuItem
            // 
            this.resetToDFUModeToolStripMenuItem.Name = "resetToDFUModeToolStripMenuItem";
            resources.ApplyResources(this.resetToDFUModeToolStripMenuItem, "resetToDFUModeToolStripMenuItem");
            this.resetToDFUModeToolStripMenuItem.Click += new System.EventHandler(this.ResetToDFUModeToolStripMenuItem_Click);
            // 
            // upgradeBootloaderToolStripMenuItem
            // 
            this.upgradeBootloaderToolStripMenuItem.Name = "upgradeBootloaderToolStripMenuItem";
            resources.ApplyResources(this.upgradeBootloaderToolStripMenuItem, "upgradeBootloaderToolStripMenuItem");
            this.upgradeBootloaderToolStripMenuItem.Click += new System.EventHandler(this.UpgradeBootloaderToolStripMenuItem_Click);
            // 
            // runFirmwareToolStripMenuItem
            // 
            this.runFirmwareToolStripMenuItem.Name = "runFirmwareToolStripMenuItem";
            resources.ApplyResources(this.runFirmwareToolStripMenuItem, "runFirmwareToolStripMenuItem");
            this.runFirmwareToolStripMenuItem.Click += new System.EventHandler(this.RunFirmwareToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkForUpdateToolStripMenuItem,
            this.documentationToolStripMenuItem,
            this.downloadFirmwareToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            resources.ApplyResources(this.helpToolStripMenuItem, "helpToolStripMenuItem");
            // 
            // checkForUpdateToolStripMenuItem
            // 
            this.checkForUpdateToolStripMenuItem.Name = "checkForUpdateToolStripMenuItem";
            resources.ApplyResources(this.checkForUpdateToolStripMenuItem, "checkForUpdateToolStripMenuItem");
            this.checkForUpdateToolStripMenuItem.Click += new System.EventHandler(this.CheckForUpdateToolStripMenuItem_Click);
            // 
            // documentationToolStripMenuItem
            // 
            this.documentationToolStripMenuItem.Name = "documentationToolStripMenuItem";
            resources.ApplyResources(this.documentationToolStripMenuItem, "documentationToolStripMenuItem");
            this.documentationToolStripMenuItem.Click += new System.EventHandler(this.DocumentationToolStripMenuItem_Click);
            // 
            // downloadFirmwareToolStripMenuItem
            // 
            this.downloadFirmwareToolStripMenuItem.Name = "downloadFirmwareToolStripMenuItem";
            resources.ApplyResources(this.downloadFirmwareToolStripMenuItem, "downloadFirmwareToolStripMenuItem");
            this.downloadFirmwareToolStripMenuItem.Click += new System.EventHandler(this.DownloadFirmwareToolStripMenuItem_Click);
            // 
            // FlashMulti
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.buttonErase);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.buttonSaveBackup);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonSerialMonitor);
            this.Controls.Add(this.buttonUpload);
            this.Controls.Add(this.buttonRead);
            this.Controls.Add(this.menuStrip2);
            this.MaximizeBox = false;
            this.Name = "FlashMulti";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.menuStrip2.ResumeLayout(false);
            this.menuStrip2.PerformLayout();
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
        private System.Windows.Forms.CheckBox showVerboseOutput;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.TextBox textActivity;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Button buttonRefresh;
        private System.Windows.Forms.Button buttonSerialMonitor;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button buttonRead;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button buttonSaveBackup;
        private System.Windows.Forms.Button buttonErase;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.MenuStrip menuStrip2;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem advancedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkForUpdateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem documentationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem downloadFirmwareToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem actionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetToDFUModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem upgradeBootloaderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem baudRateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemBaudRate57600;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemBaudRate115200;
        private System.Windows.Forms.ToolStripMenuItem runAfterUploadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem installUSBDriversToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enableDeviceDetectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bootloaderTypeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stickyDfuUsbModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem comPortUsbModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disableCompatibilityCheckToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemBaudRate500000;
        private System.Windows.Forms.ToolStripMenuItem runFirmwareToolStripMenuItem;
    }
}

