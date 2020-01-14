﻿// -------------------------------------------------------------------------------
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FlashMulti));
            this.buttonUpload = new System.Windows.Forms.Button();
            this.comPortSelector = new System.Windows.Forms.ComboBox();
            this.textFileName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonSerialMonitor = new System.Windows.Forms.Button();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.showVerboseOutput = new System.Windows.Forms.CheckBox();
            this.textVerbose = new System.Windows.Forms.TextBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.textActivity = new System.Windows.Forms.TextBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonUpload
            // 
            this.buttonUpload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonUpload.Location = new System.Drawing.Point(373, 47);
            this.buttonUpload.Name = "buttonUpload";
            this.buttonUpload.Size = new System.Drawing.Size(75, 23);
            this.buttonUpload.TabIndex = 6;
            this.buttonUpload.Text = "Upload";
            this.buttonUpload.TextImageRelation = System.Windows.Forms.TextImageRelation.TextAboveImage;
            this.buttonUpload.UseVisualStyleBackColor = true;
            this.buttonUpload.Click += new System.EventHandler(this.ButtonUpload_Click);
            // 
            // comPortSelector
            // 
            this.comPortSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comPortSelector.DropDownWidth = 90;
            this.comPortSelector.FormattingEnabled = true;
            this.comPortSelector.Location = new System.Drawing.Point(77, 48);
            this.comPortSelector.Name = "comPortSelector";
            this.comPortSelector.Size = new System.Drawing.Size(90, 21);
            this.comPortSelector.TabIndex = 2;
            this.comPortSelector.SelectedIndexChanged += new System.EventHandler(this.ComPortSelector_SelectionChanged);
            // 
            // textFileName
            // 
            this.textFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textFileName.Location = new System.Drawing.Point(77, 20);
            this.textFileName.Name = "textFileName";
            this.textFileName.Size = new System.Drawing.Size(290, 20);
            this.textFileName.TabIndex = 0;
            this.textFileName.DoubleClick += new System.EventHandler(this.ButtonBrowse_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "File Name:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 52);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Serial Port:";
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowse.Location = new System.Drawing.Point(373, 19);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(75, 22);
            this.buttonBrowse.TabIndex = 1;
            this.buttonBrowse.Text = "Browse";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.ButtonBrowse_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.buttonSerialMonitor);
            this.groupBox1.Controls.Add(this.buttonRefresh);
            this.groupBox1.Controls.Add(this.buttonUpload);
            this.groupBox1.Controls.Add(this.buttonBrowse);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.comPortSelector);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textFileName);
            this.groupBox1.Location = new System.Drawing.Point(15, 37);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(454, 84);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Settings";
            // 
            // buttonSerialMonitor
            // 
            this.buttonSerialMonitor.Location = new System.Drawing.Point(259, 47);
            this.buttonSerialMonitor.Name = "buttonSerialMonitor";
            this.buttonSerialMonitor.Size = new System.Drawing.Size(80, 23);
            this.buttonSerialMonitor.TabIndex = 15;
            this.buttonSerialMonitor.Text = "Serial Monitor";
            this.buttonSerialMonitor.UseVisualStyleBackColor = true;
            this.buttonSerialMonitor.Click += new System.EventHandler(this.ButtonSerialMonitor_Click);
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Location = new System.Drawing.Point(173, 47);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(80, 23);
            this.buttonRefresh.TabIndex = 3;
            this.buttonRefresh.Text = "Refresh Ports";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.ButtonRefresh_Click);
            // 
            // showVerboseOutput
            // 
            this.showVerboseOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.showVerboseOutput.AutoSize = true;
            this.showVerboseOutput.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.showVerboseOutput.Location = new System.Drawing.Point(324, 159);
            this.showVerboseOutput.Name = "showVerboseOutput";
            this.showVerboseOutput.Size = new System.Drawing.Size(130, 17);
            this.showVerboseOutput.TabIndex = 8;
            this.showVerboseOutput.Text = "Show Verbose Output";
            this.showVerboseOutput.UseVisualStyleBackColor = true;
            this.showVerboseOutput.CheckedChanged += new System.EventHandler(this.ShowVerboseOutput_OnChange);
            // 
            // textVerbose
            // 
            this.textVerbose.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.tableLayoutPanel1.SetColumnSpan(this.textVerbose, 2);
            this.textVerbose.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textVerbose.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textVerbose.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textVerbose.Location = new System.Drawing.Point(3, 184);
            this.textVerbose.Multiline = true;
            this.textVerbose.Name = "textVerbose";
            this.textVerbose.ReadOnly = true;
            this.textVerbose.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textVerbose.Size = new System.Drawing.Size(451, 1);
            this.textVerbose.TabIndex = 10;
            this.textVerbose.TabStop = false;
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.progressBar1, 2);
            this.progressBar1.Location = new System.Drawing.Point(3, 130);
            this.progressBar1.Maximum = 1000;
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(451, 23);
            this.progressBar1.TabIndex = 12;
            // 
            // textActivity
            // 
            this.textActivity.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.tableLayoutPanel1.SetColumnSpan(this.textActivity, 2);
            this.textActivity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textActivity.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textActivity.Location = new System.Drawing.Point(3, 3);
            this.textActivity.MinimumSize = new System.Drawing.Size(451, 120);
            this.textActivity.Multiline = true;
            this.textActivity.Name = "textActivity";
            this.textActivity.ReadOnly = true;
            this.textActivity.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textActivity.Size = new System.Drawing.Size(451, 120);
            this.textActivity.TabIndex = 7;
            this.textActivity.TabStop = false;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(3, 159);
            this.linkLabel1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
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
            this.linkLabel2.LinkArea = new System.Windows.Forms.LinkArea(65, 4);
            this.linkLabel2.Location = new System.Drawing.Point(14, 16);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(353, 17);
            this.linkLabel2.TabIndex = 14;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "Flash firmware to a MULTI-Module.  Get the latest firmware files here.";
            this.linkLabel2.UseCompatibleTextRendering = true;
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ReleasesLink_LinkClicked);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.textVerbose, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.textActivity, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.linkLabel1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.showVerboseOutput, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.progressBar1, 0, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(14, 127);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(457, 181);
            this.tableLayoutPanel1.TabIndex = 15;
            // 
            // FlashMulti
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 320);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.linkLabel2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 359);
            this.Name = "FlashMulti";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Flash Multi";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
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
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.Button buttonRefresh;
        private System.Windows.Forms.Button buttonSerialMonitor;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}

