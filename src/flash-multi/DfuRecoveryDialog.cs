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
    using System;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    /// <summary>
    /// Class for the DFU recovery dialog form.
    /// </summary>
    public partial class DfuRecoveryDialog : Form
    {
        private Timer timer1 = new Timer();
        private FlashMulti flashMulti;

        /// <summary>
        /// Initializes a new instance of the <see cref="DfuRecoveryDialog"/> class.
        /// </summary>
        public DfuRecoveryDialog(FlashMulti flashMulti)
        {
            this.InitializeComponent();
            this.flashMulti = flashMulti;
            this.Load += this.DfuRecoveryDialog_Load;
            this.Shown += this.DfuRecoveryDialog_Shown;
        }

        private async void DfuRecoveryDialog_Shown(object sender, EventArgs e)
        {
            this.flashMulti.AppendLog("Waiting up to 30s for DFU device to disappear ...");

            // Wait for the DFU device to disappear
            bool dfuCheck = false;
            await Task.Run(() => { dfuCheck = MapleDevice.WaitForDFU(30000, true); });

            if (dfuCheck)
            {
                this.flashMulti.AppendLog(" gone.\r\n");
            }
            else
            {
                this.flashMulti.AppendLog(" timed out!\r\n");
                MessageBox.Show("DFU device was not unplugged in time.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return;
            }

            this.flashMulti.AppendLog("Waiting up to 30s for DFU device to appear ...");

            // Reset the progress bar
            this.progressBar1.Value = 0;

            // Wait for the DFU device to appear
            await Task.Run(() => { dfuCheck = MapleDevice.WaitForDFU(30000); });

            if (dfuCheck)
            {
                this.flashMulti.AppendLog(" got it.\r\n");
                this.DialogResult = DialogResult.OK;
                this.Close();
                return;
            }
            else
            {
                this.flashMulti.AppendLog(" timed out!\r\n");
                MessageBox.Show("DFU device was not plugged in in time.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return;
            }
        }

        private void ButtonCancel_Click(object sender, System.EventArgs e)
        {
            this.flashMulti.AppendLog("\r\nDFU Recovery cancelled.\r\n");
            this.DialogResult = DialogResult.Cancel;
            this.Close();
            return;
        }

        private void DfuRecoveryDialog_Load(object sender, EventArgs e)
        {
            this.timer1.Enabled = true;
            this.timer1.Start();
            this.timer1.Interval = 1000;
            this.progressBar1.Maximum = 30;
            this.timer1.Tick += new EventHandler(this.Timer1_Tick);
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (this.progressBar1.Value != this.progressBar1.Maximum)
            {
                this.progressBar1.Value++;
            }
            else
            {
                this.timer1.Stop();
            }
        }
    }
}
