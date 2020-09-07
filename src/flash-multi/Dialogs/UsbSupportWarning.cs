// -------------------------------------------------------------------------------
// <copyright file="UsbSupportWarning.cs" company="Ben Lye">
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
    using System.Windows.Forms;

    public partial class UsbSupportWarning : Form
    {
        public UsbSupportWarning()
        {
            this.InitializeComponent();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
            return;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            // Disable the warning
            if (this.disableUsbWarning.Checked == true) {
                Properties.Settings.Default.WarnIfNoUSB = false;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        }
    }
}
