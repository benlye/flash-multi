// -------------------------------------------------------------------------------
// <copyright file="DfuRecoveryDialog.cs" company="Ben Lye">
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
    using System.Threading.Tasks;
    using System.Windows.Forms;

    /// <summary>
    /// Class for the DFU recovery dialog form.
    /// </summary>
    public partial class DfuRecoveryDialog : Form
    {
        private Timer progressTimer = new Timer();
        private FlashMulti flashMulti;

        /// <summary>
        /// Initializes a new instance of the <see cref="DfuRecoveryDialog"/> class.
        /// </summary>
        /// <param name="flashMulti">An instance of the <see cref="FlashMulti"/> form.</param>
        public DfuRecoveryDialog(FlashMulti flashMulti)
        {
            this.InitializeComponent();
            this.flashMulti = flashMulti;
            this.Load += this.DfuRecoveryDialog_Load;
            this.Shown += this.DfuRecoveryDialog_Shown;
        }

        /// <summary>
        /// Prompts the user to unplug / replug the module to get it into recovery mode.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private async void DfuRecoveryDialog_Shown(object sender, EventArgs e)
        {
            // Check if we have a DFU device we need to see unplugged
            bool dfuCheck = MapleDevice.FindMaple().DfuMode;

            if (dfuCheck)
            {
                this.flashMulti.AppendLog("Waiting for DFU device to disappear ...");

                // Wait 30s for the DFU device to disappear and reappear
                await Task.Run(() => { dfuCheck = MapleDevice.WaitForDFU(30000, true); });

                // Handle user cancelling DFU recovery
                if (this.DialogResult == DialogResult.Cancel)
                {
                    return;
                }

                if (dfuCheck)
                {
                    // Wait 30s for the DFU device to disappear
                    await Task.Run(() => { dfuCheck = MapleDevice.WaitForDFU(30000, true); });

                    if (dfuCheck)
                    {
                        // The module was unplugged
                        this.flashMulti.AppendLog(" gone.\r\n");
                    }
                    else
                    {
                        // The module wasn't unplugged when the timer expired.
                        this.flashMulti.AppendLog(" timed out!\r\n");
                        using (new CenterWinDialog(this.flashMulti))
                        {
                            MessageBox.Show("DFU device was not unplugged in time.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                        this.DialogResult = DialogResult.Cancel;
                        this.Close();
                        return;
                    }
                }
            }

            this.flashMulti.AppendLog("Waiting for DFU device to appear ...");

            // Wait for the DFU device to appear
            await Task.Run(() => { dfuCheck = MapleDevice.WaitForDFU(30000 - (this.timerProgressBar.Value * 1000)); });

            // Handle user cancelling DFU recovery
            if (this.DialogResult == DialogResult.Cancel)
            {
                return;
            }

            if (dfuCheck)
            {
                // The module was plugged in
                this.flashMulti.AppendLog(" got it.\r\n");
                this.DialogResult = DialogResult.OK;
                this.Close();
                return;
            }
            else
            {
                // The module wasn't plugged in when the timer expired
                this.flashMulti.AppendLog(" timed out!\r\n");
                using (new CenterWinDialog(this))
                {
                    MessageBox.Show("DFU device was not plugged in in time.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                this.DialogResult = DialogResult.Abort;
                this.Close();
                return;
            }
        }

        /// <summary>
        /// Handle the Cancel button being clicked.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event argument.</param>
        private void ButtonCancel_Click(object sender, System.EventArgs e)
        {
            this.flashMulti.AppendLog(" cancelled.\r\n");
            this.DialogResult = DialogResult.Cancel;
            this.Close();
            return;
        }

        /// <summary>
        /// Configure the timer when the form is loaded.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void DfuRecoveryDialog_Load(object sender, EventArgs e)
        {
            // Configures the timer used to count the progress bar down
            this.progressTimer.Enabled = true;
            this.progressTimer.Interval = 1000;

            // Configure the progress bar
            this.timerProgressBar.Minimum = 0;
            this.timerProgressBar.Maximum = 30;
            this.timerProgressBar.Value = 0;
            this.progressTimer.Tick += new EventHandler(this.ProgressBarTimer_TickUp);

            // Start the timer to count the bar down
            this.progressTimer.Start();
        }

        /// <summary>
        /// Increment the progress bar when the timer ticks.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void ProgressBarTimer_TickUp(object sender, EventArgs e)
        {
            if (this.timerProgressBar.Value != this.timerProgressBar.Maximum)
            {
                this.timerProgressBar.Value++;
            }
            else
            {
                this.progressTimer.Stop();
            }
        }

        /// <summary>
        /// Increment the progress bar when the timer ticks.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void ProgressBarTimer_TickDown(object sender, EventArgs e)
        {
            if (this.timerProgressBar.Value != this.timerProgressBar.Minimum)
            {
                this.timerProgressBar.Value--;
            }
            else
            {
                this.progressTimer.Stop();
            }
        }
    }
}
