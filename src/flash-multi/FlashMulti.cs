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
    using System.IO.Ports;
    using System.Linq;
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

            // Register a handler to be notified when USB devices are added or removed
            UsbNotification.RegisterUsbDeviceNotification(this.Handle);
        }

        /// <summary>
        /// General purpose delegation method.
        /// </summary>
        public delegate void InvokeDelegate();

        /// <summary>
        /// Delegation method for selecing a COM port in the dropdown list.
        /// </summary>
        /// <param name="port">The port to select.</param>
        private delegate void ComPortSelectorDelegate(object port);

        /// <summary>
        /// Delegation method to get the currently selected COM port.
        /// </summary>
        /// <returns>A <see cref="ComPort"/> object.</returns>
        private delegate object SelectedComPortDelegate();

        /// <summary>
        /// Delegation method to populate the COM port dropdown list.
        /// </summary>
        /// <param name="ports">A list of <see cref="ComPort"/> objects.</param>
        private delegate void PopulateComPortSelectorDelegate(List<ComPort> ports);

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
                _ = this.PopulateComPortsAsync();
            }

            // Check if there is a Maple device attached
            MapleDevice mapleCheck = MapleDevice.FindMaple();

            // Toggle the controls
            this.buttonUpload.Enabled = arg;
            this.buttonBrowse.Enabled = arg;
            this.buttonRefresh.Enabled = arg;
            this.buttonSerialMonitor.Enabled = arg;
            this.textFileName.Enabled = arg;
            this.comPortSelector.Enabled = arg;

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
                        Debug.WriteLine($"Flash multi saw device removal");
                        _ = this.PopulateComPortsAsync();
                        break;
                    case UsbNotification.DbtDevicearrival:
                        // Update the COM port list
                        Debug.WriteLine($"Flash multi saw device arrival");
                        _ = this.PopulateComPortsAsync();
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

        /// <summary>
        /// Event handler for the application window loading.
        /// </summary>
        /// <param name="e">The event.</param>
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
        /// Called by changes to the file name or COM port selector.
        /// </summary>
        private void CheckControls()
        {
            if (this.InvokeRequired)
            {
               this.Invoke(new Action(this.CheckControls));
               return;
            }

            if (this.textFileName.Text != string.Empty && this.comPortSelector.SelectedItem != null)
            {
                this.buttonUpload.Enabled = true;
            }
            else
            {
                this.buttonUpload.Enabled = false;
            }

            if (this.comPortSelector.SelectedItem != null)
            {
                this.buttonSerialMonitor.Enabled = true;
            }
            else
            {
                this.buttonSerialMonitor.Enabled = false;
            }
        }

        private async Task PopulateComPortsAsync()
        {
            await Task.Run(() => { this.PopulateComPorts(); });
        }

        /// <summary>
        /// Populates the list of COM ports.
        /// </summary>
        private void PopulateComPorts()
        {
            // Don't refresh if the control is not enabled
            if (!this.comPortSelector.Enabled)
            {
                return;
            }

            // Get the current list from the combobox so we can auto-select the new device
            var oldPortList = this.comPortSelector.Items;

            // Cache the selected item so we can try to re-select it later
            object selectedValue = null;
            selectedValue = this.GetSelectedPort();

            // Enumerate the COM ports and bind the COM port selector
            List<ComPort> comPorts = new List<ComPort>();
            comPorts = ComPort.EnumeratePortList();

            // Check if we have a Maple device
            MapleDevice mapleCheck = MapleDevice.FindMaple();

            // Populate the COM port selector
            this.PopulatePortSelector(comPorts);

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
            this.SelectPort(selectedValue);

            // Set the width of the dropdown
            // this.comPortSelector.DropDownWidth = comPorts.Select(c => c.DisplayName).ToList().Max(x => TextRenderer.MeasureText(x, this.comPortSelector.Font).Width);

            // Make sure the Update button is disabled if there is no port selected
            this.CheckControls();
        }

        private void SelectPort(object selectedPort)
        {
            if (this.comPortSelector.InvokeRequired)
            {
                this.comPortSelector.Invoke(new ComPortSelectorDelegate(this.SelectPort), new object[] { selectedPort });
            }
            else
            {
                if (selectedPort != null)
                {
                    this.comPortSelector.SelectedValue = selectedPort;
                }
                else
                {
                    this.comPortSelector.SelectedItem = null;
                }
            }
        }

        private object GetSelectedPort()
        {
            object selectedValue = null;
            if (this.comPortSelector.InvokeRequired)
            {
                selectedValue = this.comPortSelector.Invoke(new SelectedComPortDelegate(this.GetSelectedPort));
            }
            else
            {
                selectedValue = this.comPortSelector.SelectedValue;
            }

            return selectedValue;
        }

        private void PopulatePortSelector(List<ComPort> comPorts)
        {
            if (this.comPortSelector.InvokeRequired)
            {
                this.comPortSelector.Invoke(new PopulateComPortSelectorDelegate(this.PopulatePortSelector), new object[] { comPorts });
            }
            else
            {
                this.comPortSelector.DataSource = comPorts;
                this.comPortSelector.DisplayMember = "Name";
                this.comPortSelector.ValueMember = "Name";
            }
        }

        /// <summary>
        /// Main method where all the action happens.
        /// Called by the Upload button.
        /// </summary>
        private async void ButtonUpload_Click(object sender, EventArgs e)
        {
            // Disable the buttons until this flash attempt is complete
            Debug.WriteLine("Disabling the controls...");
            this.EnableControls(false);

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

            // Determine if the selected file contains USB / bootloader support
            bool firmwareSupportsUsb = this.CheckForUsbSupport();

            // Error if flashing non-USB firmware via native USB port
            if (mapleResult.DeviceFound && !firmwareSupportsUsb)
            {
                string msgBoxMessage = "The selected firmware file was compiled without USB support.\r\n\r\nFlashing this firmware would prevent the Multiprotocol module from functioning correctly.\r\n\r\nPlease select a different firmware file.";
                MessageBox.Show(msgBoxMessage, "Incompatible Firmware", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Get the selected COM port
            string comPort = this.comPortSelector.SelectedValue.ToString();

            // Do the selected flash using the appropriate method
            if (mapleResult.DeviceFound == true)
            {
                Debug.WriteLine($"Maple device found in {mapleResult.Mode} mode\r\n");
                await MapleDevice.WriteFlash(this, this.textFileName.Text, comPort);
            }
            else
            {
                await SerialDevice.WriteFlash(this, this.textFileName.Text, comPort, firmwareSupportsUsb);
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

                    // Check the file size
                    if (!this.CheckFirmwareFileSize())
                    {
                        return;
                    }

                    // Check if the binary file contains USB / bootloader support
                    if (this.CheckForUsbSupport())
                    {
                        Debug.WriteLine("Firmware file compiled with USB support.");
                    }
                    else
                    {
                        Debug.WriteLine("Firmware file was not compiled with USB support.");
                    }
                }
            }

            // Check if the Upload button should be enabled yet
            this.CheckControls();
        }

        /// <summary>
        /// Parses the binary file looking for a string which indicates that the compiled firmware images contains USB support.
        /// The binary firmware file will contain the strings 'Maple' and 'LeafLabs' if it was compiled with support for the USB / Flash from TX bootloader.
        /// </summary>
        /// <returns>A boolean value indicatating whether or not the firmware supports USB.</returns>
        private bool CheckForUsbSupport()
        {
            bool usbSupportEnabled = false;
            string fileName = this.textFileName.Text;

            byte[] byteBuffer = File.ReadAllBytes(fileName);
            string byteBufferAsString = System.Text.Encoding.ASCII.GetString(byteBuffer);
            int offset = byteBufferAsString.IndexOf("M\0a\0p\0l\0e\0\u0012\u0003L\0e\0a\0f\0L\0a\0b\0s\0\u0012\u0001");

            if (offset > 0)
            {
                usbSupportEnabled = true;
            }

            return usbSupportEnabled;
        }

        /// <summary>
        /// Checks that the compiled firmware will fit on the module.
        /// </summary>
        /// <returns>Returns a boolean indicating whehter or not the firmware size is OK.</returns>
        private bool CheckFirmwareFileSize()
        {
            // Get the file size
            long length = new System.IO.FileInfo(this.textFileName.Text).Length;

            // If the file is very large we don't want to check for USB support so throw a generic error
            if (length > 256000)
            {
                MessageBox.Show("Selected firmware file is too large.", "Firmware File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // If the file is smaller we can check if it has USB support and throw a more specific error
            int maxFileSize = this.CheckForUsbSupport() ? 120832 : 129024;

            if (length > maxFileSize)
            {
                string sizeMessage = $"Firmware file is too large.\r\n\r\nSelected file is {length / 1024:n0} KB, maximum size is {maxFileSize / 1024:n0} KB.";
                MessageBox.Show(sizeMessage, "Firmware File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        private async void ButtonRefresh_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("ButtonRefresh clicked");
            await this.PopulateComPortsAsync();
            Debug.WriteLine("ButtonRefresh handled");
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

        /// <summary>
        /// Handlse the Serial Monitor button being clicked.
        /// Opens the Serial Monitor window.
        /// </summary>
        private void ButtonSerialMonitor_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms.OfType<SerialMonitor>().Any())
            {
                SerialMonitor serialMonitor = Application.OpenForms.OfType<SerialMonitor>().FirstOrDefault();
                serialMonitor.BringToFront();
            }
            else
            {
                SerialMonitor serialMonitor = new SerialMonitor(this.comPortSelector.SelectedValue.ToString());
                serialMonitor.Show();
            }
        }
    }
}
