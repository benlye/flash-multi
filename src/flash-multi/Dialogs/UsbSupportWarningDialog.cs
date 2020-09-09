// -------------------------------------------------------------------------------
// <copyright file="UsbSupportWarningDialog.cs" company="Ben Lye">
// Copyright 2020 Ben Lye
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
    using System;
    using System.Diagnostics;
    using System.Windows.Forms;

    /// <summary>
    /// Class for the USB Support warning dialog.
    /// </summary>
    public partial class UsbSupportWarningDialog : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UsbSupportWarningDialog"/> class.
        /// </summary>
        public UsbSupportWarningDialog()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Handles the Cancel button being clicked.
        /// </summary>
        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            // Return Cancel
            this.DialogResult = DialogResult.Cancel;
            this.Close();
            return;
        }

        /// <summary>
        /// Handles the OK button being clicked.
        /// </summary>
        private void ButtonOK_Click(object sender, EventArgs e)
        {
            // Disable the warning if the box was checked
            if (this.disableUsbWarning.Checked == true)
            {
                Properties.Settings.Default.WarnIfNoUSB = false;
            }

            // Return OK
            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        }

        /// <summary>
        /// Opens a URL in the default browser.
        /// </summary>
        /// <param name="url">The URL to open.</param>
        private void OpenLink(string url)
        {
            try
            {
                System.Diagnostics.Process.Start(url);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void MoreInfoLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.OpenLink("https://github.com/benlye/flash-multi/blob/master/doc/New_Bootloader.md");
        }
    }
}
