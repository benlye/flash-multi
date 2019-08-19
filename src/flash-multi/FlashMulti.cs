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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
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

            // Register a handler to check for a new version when the form is shown the first time
            this.Shown += this.FlashMulti_Shown;

            // Register a handler to run on loading the form
            this.Load += this.FlashMulti_Load;

            // Resgister a handler to be notified when USB devices are added or removed
            UsbNotification.RegisterUsbDeviceNotification(this.Handle);
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

            if (arg)
            {
                // Populate the COM ports
                this.PopulateComPortsAsync();
            }

            // Check if there is a Maple device attached
            MapleDevice mapleCheck = MapleDevice.FindMaple();

            // Toggle the controls
            this.buttonUpload.Enabled = arg;
            this.buttonBrowse.Enabled = arg;
            this.buttonRefresh.Enabled = arg;
            this.textFileName.Enabled = arg;
            this.comPortSelector.Enabled = arg;

            // Keep the Write Bootloader controls disabled if a Maple device is plugged in.
            if (mapleCheck.DeviceFound)
            {
                this.writeBootloader_Yes.Checked = true;
                this.writeBootloader_Yes.Enabled = false;
                this.writeBootloader_No.Enabled = false;
            }
            else
            {
                this.writeBootloader_Yes.Enabled = arg;
                this.writeBootloader_No.Enabled = arg;
            }

            // Check a couple of things if we're re-enabling
            if (arg)
            {
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
                        // Update the COM port list
                        this.BeginInvoke(new InvokeDelegate(this.PopulateComPortsAsync));
                        break;
                    case UsbNotification.DbtDevicearrival:
                        // Update the COM port list
                        this.BeginInvoke(new InvokeDelegate(this.PopulateComPortsAsync));
                        break;
                }
            }
        }

        /// <summary>
        /// Override method to handle the application closing.
        /// Unregisters device change notifications.
        /// </summary>
        /// <param name="e">The event.</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Call the base method
            base.OnFormClosing(e);

            // Unregister for USB notifications
            UsbNotification.UnregisterUsbDeviceNotification();

            // Save the window position
            Properties.Settings.Default.WindowLocation = this.Location;
            Properties.Settings.Default.Save();
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
            if (Properties.Settings.Default.CheckForUpdates == true)
            {
                UpdateCheck.DoCheck(this);
            }
            else
            {
                Debug.WriteLine("Update check is disabled.");
            }
        }

        private void FlashMulti_Load(object sender, EventArgs e)
        {
            // Restore the last window location
            var windowLocation = Properties.Settings.Default.WindowLocation;
            if (windowLocation.X != -1 && windowLocation.Y != -1)
            {
                this.Location = Properties.Settings.Default.WindowLocation;
            }
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

        private async void PopulateComPortsAsync()
        {
            await Task.Run(() => { this.PopulateComPorts(); });
        }

        /// <summary>
        /// Populates the list of COM ports.
        /// </summary>
        private void PopulateComPorts()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(this.PopulateComPorts));
                return;
            }

            // Don't refresh if the control is not enabled
            if (!this.comPortSelector.Enabled)
            {
                return;
            }

            // Get the current list from the combobox so we can auto-select the new device
            var oldPortList = this.comPortSelector.Items;

            // Cache the selected item so we can try to re-select it later
            object selectedValue = null;
            selectedValue = this.comPortSelector.SelectedValue;

            // Enumerate the COM ports and bind the COM port selector
            List<ComPort> comPorts = new List<ComPort>();
            comPorts = ComPort.EnumeratePortList();

            // Check if we have a Maple device
            MapleDevice mapleCheck = MapleDevice.FindMaple();

            this.comPortSelector.DataSource = comPorts;
            this.comPortSelector.DisplayMember = "Name";
            this.comPortSelector.ValueMember = "Name";

            // If we had an old list, compare it to the new one and pick the first item which is new
            if (oldPortList.Count > 0)
            {
                foreach (ComPort newPort in comPorts)
                {
                    bool found = false;
                    foreach (ComPort oldPort in oldPortList)
                    {
                        if (newPort.Name == oldPort.Name)
                        {
                            found = true;
                        }
                    }

                    if (found == false)
                    {
                        Debug.WriteLine($"{newPort.Name} was added.");
                        selectedValue = newPort.Name;
                    }
                }
            }

            // Re-select the previously selected item
            if (selectedValue != null)
            {
                this.comPortSelector.SelectedValue = selectedValue;
            }
            else
            {
                this.comPortSelector.SelectedItem = null;
            }

            // Check if we there's a Maple device plugged in
            if (mapleCheck.DeviceFound)
            {
                // Set the Write Bootloader radio button and disable the controls if a Maple device is present
                // Required so that the firmware size is calculated correctly
                this.writeBootloader_Yes.Checked = true;
                this.writeBootloader_Yes.Enabled = false;
                this.writeBootloader_No.Enabled = false;
            }
            else
            {
                this.writeBootloader_Yes.Enabled = true;
                this.writeBootloader_No.Enabled = true;
            }

            // Set the width of the dropdown
            // this.comPortSelector.DropDownWidth = comPorts.Select(c => c.DisplayName).ToList().Max(x => TextRenderer.MeasureText(x, this.comPortSelector.Font).Width);

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

            // Check the file size
            if (!this.CheckFirmwareFileSize())
            {
                this.EnableControls(true);
                return;
            }

            // Determine if we should use Maple or serial interface
            MapleDevice mapleResult = MapleDevice.FindMaple();

            // Warn if Write Bootloader seems to be incorrect
            bool firmwareSupportsBootloader = this.CheckForBootloaderSupport();
            if (this.writeBootloader_No.Checked && firmwareSupportsBootloader)
            {
                DialogResult response = MessageBox.Show("You have selected not to write the bootloader, but the selected firmware file was compiled with bootloader support.\r\n\r\nThe module will not function if the firmware requires the bootloader but the bootloader is not written.\r\n\r\nAre you sure you want to proceed?", "Write Bootloader", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (response != DialogResult.OK)
                {
                    return;
                }
            }

            if (this.writeBootloader_Yes.Checked && !firmwareSupportsBootloader)
            {
                string msgBoxMessage = string.Empty;
                DialogResult response = DialogResult.None;

                if (mapleResult.DeviceFound)
                {
                    msgBoxMessage = "You are flashing via USB, but the selected firmware file was compiled without USB support.\r\n\r\nContinuing would prevent the module from functioning correctly.\r\n\r\nPlease select a different firmware file.";
                    response = MessageBox.Show(msgBoxMessage, "Write Bootloader", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    msgBoxMessage = "You have selected to write the bootloader, but the selected firmware file was compiled without bootloader support.\r\n\r\nThe module will not function if the bootloader is written but the firmware does not support it.\r\n\r\nAre you sure you want to proceed?";
                    response = MessageBox.Show(msgBoxMessage, "Write Bootloader", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (response != DialogResult.OK)
                    {
                        return;
                    }
                }
            }

            // Get the selected COM port
            string comPort = this.comPortSelector.SelectedValue.ToString();

            // Check if the port can be opened
            if (!ComPort.CheckPort(comPort))
            {
                this.AppendLog(string.Format("Couldn't open port {0}", comPort));
                MessageBox.Show(string.Format("Couldn't open port {0}", comPort), "Write Firmware", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.EnableControls(true);
                return;
            }

            // Disable the buttons until this flash attempt is complete
            Debug.WriteLine("Disabling the controls...");
            this.EnableControls(false);

            // Do the selected flash using the appropriate method
            if (mapleResult.DeviceFound == true)
            {
                Debug.WriteLine($"Maple device found in {mapleResult.Mode} mode\r\n");
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

            // Check the file size
            if (!this.CheckFirmwareFileSize())
            {
                return;
            }

            // Check if there is a Maple device
            bool mapleCheck = MapleDevice.FindMaple().DeviceFound;

            // Check if the binary file contains support for the bootloader
            if (this.CheckForBootloaderSupport())
            {
                Debug.WriteLine("CHECK_FOR_BOOTLOADER is enabled in the firmware file.");
                this.writeBootloader_Yes.Checked = true;
            }
            else
            {
                Debug.WriteLine("CHECK_FOR_BOOTLOADER is not enabled in the firmware file.");
                this.writeBootloader_No.Checked = true;
            }

            // Check if the Upload button should be enabled yet
            this.CheckControls();
        }

        /// <summary>
        /// Parses the binary file looking for a string which indicates that the compiled firmware images contains bootloader support.
        /// The binary firmware file will contain the strings 'Maple' and 'LeafLabs' if it was compiled with CHECK_FOR_BOOTLOADER defined.
        /// </summary>
        /// <returns>A boolean value indicatating whether or not the firmware supports the bootloader.</returns>
        private bool CheckForBootloaderSupport()
        {
            bool bootloaderCheckEnabled = false;
            string fileName = this.textFileName.Text;

            byte[] byteBuffer = File.ReadAllBytes(fileName);
            string byteBufferAsString = System.Text.Encoding.ASCII.GetString(byteBuffer);
            int offset = byteBufferAsString.IndexOf("M\0a\0p\0l\0e\0\u0012\u0003L\0e\0a\0f\0L\0a\0b\0s\0\u0012\u0001");

            if (offset > 0)
            {
                bootloaderCheckEnabled = true;
            }

            return bootloaderCheckEnabled;
        }

        /// <summary>
        /// Checks that the compiled firmware will fit on the module.
        /// </summary>
        /// <returns>Returns a boolean indicating whehter or not the firmware size is OK.</returns>
        private bool CheckFirmwareFileSize()
        {
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
                return false;
            }

            return true;
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
            this.PopulateComPortsAsync();
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
