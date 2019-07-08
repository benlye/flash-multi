// -------------------------------------------------------------------------------
// <copyright file="FlashMulti.cs" company="Ben Lye">
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
    using System.Diagnostics;
    using System.IO;
    using System.IO.Ports;
    using System.Linq;
    using System.Threading;
    using System.Windows.Forms;

    /// <summary>
    /// The FlashMulti Form class.
    /// </summary>
    public partial class FlashMulti : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlashMulti"/> class.
        /// </summary>
        public FlashMulti()
        {
            this.InitializeComponent();

            // Include the version in the window title
            this.Text = string.Format("Flash Multi v{0}", Application.ProductVersion);

            // Set focus away from the textbox
            this.ActiveControl = this.linkLabel2;

            // Populate the list of serial ports
            this.PopulateComPorts();

            // Disable the Upload button until we're ready
            this.buttonUpload.Enabled = false;

            // Register a hendler to check for a new version when the form is shown the first time
            this.Shown += this.FlashMulti_Shown;
        }

        /// <summary>
        /// Delegation method.
        /// </summary>
        public delegate void InvokeDelegate();

        /// <summary>
        /// Handles the standard and error output from a running command.
        /// Updates the verbose output text box.
        /// </summary>
        /// <param name="sendingProcess">The process sending the output.</param>
        /// <param name="eventArgs">The data from the event.</param>
        public void OutputHandler(object sendingProcess, DataReceivedEventArgs eventArgs)
        {
            // Append to the verbose log box
            this.AppendVerbose(eventArgs.Data);
            Debug.WriteLine(eventArgs.Data);
        }

        /// <summary>
        /// Appends a string to the verbose output text box.
        /// </summary>
        /// <param name="text">String to append.</param>
        public void AppendVerbose(string text)
        {
            // Check if we're called from another thread
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(this.AppendVerbose), new object[] { text });
                return;
            }

            // Append the text
            this.textVerbose.AppendText(text + "\r\n");
        }

        /// <summary>
        /// Appends a string to the output text box.
        /// </summary>
        /// <param name="text">String to append.</param>
        public void AppendLog(string text)
        {
            // Check if we're called from another thread
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(this.AppendLog), new object[] { text });
                return;
            }

            // Append the text
            this.textActivity.AppendText(text);
        }

        /// <summary>
        /// Enable or disable the controls.
        /// </summary>
        /// <param name="arg">True to enable, False to disable.</param>
        public void EnableControls(bool arg)
        {
            // Enable the buttons
            if (arg)
            {
                Debug.WriteLine("Re-enabling the controls...");
            }
            else
            {
                Debug.WriteLine("Disabling the controls...");
            }

            // Toggle the controls
            this.buttonUpload.Enabled = arg;
            this.buttonBrowse.Enabled = arg;
            this.buttonRefresh.Enabled = arg;
            this.textFileName.Enabled = arg;
            this.comPortSelector.Enabled = arg;
            this.writeBootloader_Yes.Enabled = arg;
            this.writeBootloader_No.Enabled = arg;

            // Check a couple of things if we're re-enabling
            if (arg)
            {
                // Keep the Write Bootloader controls disabled if a Maple device is plugged in.
                if (MapleDevice.FindMaple().DeviceFound)
                {
                    this.writeBootloader_Yes.Checked = true;
                    this.writeBootloader_Yes.Enabled = false;
                    this.writeBootloader_No.Enabled = false;
                }

                // Check if the Upload button can be enabled
                this.CheckControls();
            }
        }

        /// <summary>
        /// Opens a URL in the default browser.
        /// </summary>
        /// <param name="url">The URL to open.</param>
        public void OpenLink(string url)
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

        /// <summary>
        /// Re-populate the COM port list when a USB device is plugged or unplugged.
        /// </summary>
        /// <param name="m">The message.</param>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == UsbNotification.WmDevicechange)
            {
                switch ((int)m.WParam)
                {
                    case UsbNotification.DbtDeviceremovecomplete:
                        this.BeginInvoke(new InvokeDelegate(this.PopulateComPorts));
                        break;
                    case UsbNotification.DbtDevicearrival:
                        this.BeginInvoke(new InvokeDelegate(this.PopulateComPorts));
                        break;
                }
            }
        }

        /// <summary>
        /// Called when the form has finished loading for the first time.
        /// Checks Github for a newer version.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event data.</param>
        private void FlashMulti_Shown(object sender, EventArgs e)
        {
            // Check for a new version
            UpdateCheck.DoCheck(this);
        }

        /// <summary>
        /// Checks if the Upload button should be enabled or not.
        /// Called by changes to the file name, COM port selector, or bootloader radio buttons.
        /// </summary>
        private void CheckControls()
        {
            if (this.textFileName.Text != string.Empty && this.comPortSelector.SelectedItem != null && (this.writeBootloader_No.Checked || this.writeBootloader_Yes.Checked))
            {
                this.buttonUpload.Enabled = true;
            }
            else
            {
                this.buttonUpload.Enabled = false;
            }
        }

        /// <summary>
        /// Checks if the COM port can be opened.
        /// </summary>
        /// <returns>
        /// Boolean indicating whether the port could be opened.
        /// </returns>
        private bool PortCheck(string port)
        {
            // Skip the check and return true if the selected port is 'DFU Device'
            if (port == "DFU Device")
            {
                return true;
            }

            bool result = false;

            // Try to open the serial port, catch an exception if we fail
            SerialPort serialPort = new SerialPort(port);
            try
            {
                serialPort.Open();
                if (serialPort.IsOpen)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                Thread.Sleep(50);
                serialPort.Close();
            }

            return result;
        }

        /// <summary>
        /// Populates the list of COM ports.
        /// </summary>
        private void PopulateComPorts()
        {
            // Cache the selected item so we can try to re-select it later
            object selectedItem = null;
            selectedItem = this.comPortSelector.SelectedItem;

            // Get the list of COM port names
            string[] comPorts = SerialPort.GetPortNames();

            // Sort the list of ports
            var orderedComPorts = comPorts.OrderBy(c => c.Length).ThenBy(c => c).ToList();

            // Clear the existing list
            this.comPortSelector.Items.Clear();

            // Add the ports one by one
            foreach (string port in orderedComPorts)
            {
                this.comPortSelector.Items.Add(port);
            }

            // Short pause to give a DFU device time to show up
            Thread.Sleep(50);

            // Check if we there's a Maple device plugged in
            MapleDevice mapleCheck = MapleDevice.FindMaple();

            if (mapleCheck.DeviceFound)
            {
                // Set the Write Bootloader radio button and disable the controls if a Maple device is present
                // Required so that the firmware size is calculated correctly
                this.writeBootloader_Yes.Checked = true;
                this.writeBootloader_Yes.Enabled = false;
                this.writeBootloader_No.Enabled = false;

                // If the Maple device is in DFU mode add a DFU device to the list
                // Required in case there are no other serial devices present as the user need to select something from the list
                if (mapleCheck.DfuMode)
                {
                    this.comPortSelector.Items.Add("DFU Device");
                }
            }
            else
            {
                this.writeBootloader_Yes.Enabled = true;
                this.writeBootloader_No.Enabled = true;
            }

            // Re-select the previously selected item
            this.comPortSelector.SelectedItem = selectedItem;

            // Make sure the Update button is disabled if there is no port selected
            this.CheckControls();
        }

        /// <summary>
        /// Main method where all the action happens.
        /// Called by the Upload button.
        /// </summary>
        private void ButtonUpload_Click(object sender, EventArgs e)
        {
            // Clear the output box
            Debug.WriteLine("Clearing the output textboxes...");
            this.textActivity.Clear();
            this.textVerbose.Clear();
            this.progressBar1.Value = 0;

            // Check if the file exists
            if (!File.Exists(this.textFileName.Text))
            {
                this.AppendLog(string.Format("File {0} does not exist", this.textFileName.Text));
                MessageBox.Show("Firmware file does not exist.", "Write Firmware", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.EnableControls(true);
                return;
            }

            // Check that the file size is OK
            // Max size is 120,832B (118KB) with bootloader, 129,024B (126KB) without
            int maxFileSize = 129024;
            if (this.writeBootloader_Yes.Checked)
            {
                maxFileSize = 120832;
            }

            long length = new System.IO.FileInfo(this.textFileName.Text).Length;

            if (length > maxFileSize)
            {
                this.AppendLog(string.Format("Firmware file is too large.\r\nFile is {1:n0} KB, maximum size is {2:n0} KB.", this.textFileName.Text, length / 1024, maxFileSize / 1024));
                MessageBox.Show("Firmware file is too large.", "Write Firmware", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.EnableControls(true);
                return;
            }

            // Get the selected COM port
            string comPort = this.comPortSelector.SelectedItem.ToString();

            // Check if the port can be opened
            if (!this.PortCheck(comPort))
            {
                this.AppendLog(string.Format("Couldn't open port {0}", comPort));
                MessageBox.Show(string.Format("Couldn't open port {0}", comPort), "Write Firmware", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.EnableControls(true);
                return;
            }

            // Disable the buttons until this flash attempt is complete
            Debug.WriteLine("Disabling the controls...");
            this.EnableControls(false);

            // Determine if we should use Maple or serial interface
            MapleDevice mapleResult = MapleDevice.FindMaple();

            if (mapleResult.DeviceFound == true)
            {
                this.AppendLog(string.Format("Maple device found in {0} mode\r\n", mapleResult.Mode));
            }

            // Do the selected flash using the appropriate method
            if (mapleResult.DeviceFound == true)
            {
                // MapleFlashWrite(textFileName.Text, comPort);
                MapleDevice.WriteFlash(this, this.textFileName.Text, comPort);
            }
            else
            {
                SerialDevice.WriteFlash(this, this.textFileName.Text, comPort);
            }
        }

        /// <summary>
        /// Selects a firmware file to flash.
        /// </summary>
        private void ButtonBrowse_Click(object sender, EventArgs e)
        {
            // Create the file open dialog
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // Title for the dialog
                openFileDialog.Title = "Choose file to flash";

                // Filter for .bin files
                openFileDialog.Filter = ".bin File|*.bin";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Set the text box to the selected file name
                    this.textFileName.Text = openFileDialog.FileName;
                }
            }

            // Check the file name and pre-set the Write Bootloader option
            if (this.textFileName.Text.IndexOf("_FTDI_") > -1)
            {
                this.writeBootloader_No.Checked = true;
            }
            else if (this.textFileName.Text.IndexOf("_TXFLASH_") > -1)
            {
                this.writeBootloader_Yes.Checked = true;
            }

            // Check if the Upload button should be enabled yet
            this.CheckControls();
        }

        /// <summary>
        /// Handles a change in the COM port selection dropdown.
        /// </summary>
        private void ComPortSelector_SelectionChanged(object sender, EventArgs e)
        {
            // Check if the Upload button should be enabled yet
            this.CheckControls();
        }

        /// <summary>
        /// Handles input in the firmware file name text box.
        /// </summary>
        private void TextFileName_OnChange(object sender, EventArgs e)
        {
            // Check if the Upload button should be enabled yet
            this.CheckControls();
        }

        private void WriteBootloader_OnChange(object sender, EventArgs e)
        {
            // Check if the Upload button should be enabled yet
            this.CheckControls();
        }

        /// <summary>
        /// Updates the progress bar.
        /// </summary>
        private void UpdateProgress(int value)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<int>(this.UpdateProgress), new object[] { value });
                return;
            }

            this.progressBar1.Value = value;
        }

        /// <summary>
        /// Handles the show verbose output text box being checked or unchecked.
        /// Shows or hides the verbose output text box.
        /// </summary>
        private void ShowVerboseOutput_OnChange(object sender, EventArgs e)
        {
            if (this.showVerboseOutput.Checked == true)
            {
                this.Height = 520;
            }
            else
            {
                this.Height = 330;
            }
        }

        /// <summary>
        /// Handles the refresh button being clicked.
        /// Updates the list of COM ports in the drop down.
        /// </summary>
        private void ButtonRefresh_Click(object sender, EventArgs e)
        {
            this.PopulateComPorts();
        }

        /// <summary>
        /// Handles the Github repo link being clicked.
        /// </summary>
        private void RepoLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.OpenLink("https://github.com/benlye/flash-multi");
        }

        /// <summary>
        /// Handles the Multi firmware repo releases link being clicked.
        /// </summary>
        private void ReleasesLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.OpenLink("https://github.com/pascallanger/DIY-Multiprotocol-TX-Module/releases");
        }
    }
}
