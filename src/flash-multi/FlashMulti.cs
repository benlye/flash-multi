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
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Timers;
    using System.Windows.Forms;

    /// <summary>
    /// The FlashMulti Form class.
    /// </summary>
    public partial class FlashMulti : Form
    {
        /// <summary>
        /// Buffer for verbose output logging.
        /// </summary>
        private string outputLineBuffer = string.Empty;

        /// <summary>
        /// Keep track of the current avrdude activity.
        /// </summary>
        private string avrdudeActivity = string.Empty;

        /// <summary>
        ///  The number of steps required for an avrdude flash.
        /// </summary>
        internal int AvrdudeSteps = 0;

        /// <summary>
        /// The current avrdude flash step.
        /// </summary>
        internal int AvrdudeFlashStep = 1;

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
            // Ignore the meaningless DFU error we get on every upload
            if (eventArgs.Data != "error resetting after download: usb_reset: could not reset device, win error: The system cannot find the file specified.")
            {
                // Append to the verbose log box
                this.AppendVerbose(eventArgs.Data);
            }

            Debug.WriteLine(eventArgs.Data);

            // Update the progress bar if there is a percentage in the output
            Regex regexSerialProgress = new Regex(@"\((\d+)\.\d\d\%\)");
            if (eventArgs.Data != null)
            {
                Match match = regexSerialProgress.Match(eventArgs.Data);
                if (match.Success)
                {
                    this.UpdateProgress(int.Parse(match.Groups[1].Value));
                }
            }
        }

        /// <summary>
        /// Appends a character to the verbose output text box.
        /// </summary>
        /// <param name="data">String to append.</param>
        public void CharOutputHandler(char data)
        {
            this.outputLineBuffer = this.outputLineBuffer + (char)data;

            // Write complete lines to verbose output box
            // Match a 'normal' end of line, or the end of line used by stm32flash
            if (this.outputLineBuffer.EndsWith("\r\n") || this.outputLineBuffer.EndsWith(") \r"))
            {
                // Suppress writing the dfu-util finished line and the avrdude reading/writing finished lines
                if (this.outputLineBuffer != "Starting download: [##################################################] finished!\r\n")
                {
                    if ((this.outputLineBuffer.StartsWith("Reading | #") && this.outputLineBuffer.EndsWith("\r\n")) || (this.outputLineBuffer.StartsWith("Writing | #") && this.outputLineBuffer.EndsWith("\r\n")))
                    {
                        this.AppendVerbose(string.Empty);
                    }
                    else
                    {
                        this.outputLineBuffer = this.outputLineBuffer.TrimEnd();
                        this.AppendVerbose(this.outputLineBuffer);
                    }
                }

                // Update the progress bar if there is a percentage in the output (stm32flash)
                Regex regexSerialProgress = new Regex(@"\((\d+)\.\d\d\%\)");
                if (this.outputLineBuffer != string.Empty)
                {
                    Match match = regexSerialProgress.Match(this.outputLineBuffer);
                    if (match.Success)
                    {
                        this.UpdateProgress(int.Parse(match.Groups[1].Value));
                    }
                }

                // Clear the buffer
                this.outputLineBuffer = string.Empty;
            }
            else
            {
                // Handle progress from dfu-util
                if (this.outputLineBuffer == "Starting download: [")
                {
                    this.AppendVerbose(this.outputLineBuffer, false);
                }

                if (this.outputLineBuffer.StartsWith("Starting download: [#"))
                {
                    if (data == '#')
                    {
                        // Convert number of hashes in string to progress bar percentage
                        int dfuProgress = (this.outputLineBuffer.Length - 20) * 2;

                        // Update the progress bar
                        this.UpdateProgress(dfuProgress);
                    }

                    // Append the character to the output
                    this.AppendVerbose(((char)data).ToString(), false);

                    // Progress line is finished so end it with a newline
                    if (this.outputLineBuffer == "Starting download: [##################################################] finished!")
                    {
                        this.AppendVerbose(string.Empty);
                    }
                }

                // Handle progress from avrdude
                if (this.outputLineBuffer == "avrdude.exe: erasing chip")
                {
                    this.AppendLog($"[{this.AvrdudeFlashStep}/{this.AvrdudeSteps}] Erasing module ... ");
                    this.avrdudeActivity = "erasing";
                    this.AvrdudeFlashStep++;
                }

                if (this.outputLineBuffer == "avrdude.exe: writing lock (1 bytes):" && this.avrdudeActivity == "erasing")
                {
                    this.AppendLog($"done\r\n[{this.AvrdudeFlashStep}/{this.AvrdudeSteps}] Setting fuses ... ");
                    this.avrdudeActivity = "fuses";
                    this.AvrdudeFlashStep++;
                }

                if (this.outputLineBuffer.StartsWith("avrdude.exe: writing flash ") && this.outputLineBuffer.EndsWith("):"))
                {
                    if (this.avrdudeActivity == "writebootloader" && this.AvrdudeSteps == 5)
                    {
                        // Writing firmware after bootloader
                        this.AppendLog($"done\r\n[{this.AvrdudeFlashStep}/{this.AvrdudeSteps}] Writing firmware ... ");
                        this.avrdudeActivity = "writefirmware";
                        this.AvrdudeFlashStep++;
                    }
                    else if (this.avrdudeActivity == "fuses" && this.AvrdudeSteps == 4)
                    {
                        // Writing firmware after fuses
                        this.AppendLog($"done\r\n[{this.AvrdudeFlashStep}/{this.AvrdudeSteps}] Writing flash ... ");
                        this.avrdudeActivity = "writefirmware";
                        this.AvrdudeFlashStep++;
                    }
                    else if (this.avrdudeActivity == "fuses" && this.AvrdudeSteps == 5)
                    {
                        // Writing bootloader after fuses
                        this.AppendLog($"done\r\n[{this.AvrdudeFlashStep}/{this.AvrdudeSteps}] Writing bootloader ... ");
                        this.avrdudeActivity = "writebootloader";
                        this.AvrdudeFlashStep++;
                    }
                }

                if (this.outputLineBuffer == "avrdude.exe: reading on-chip flash data:" && this.avrdudeActivity == "writefirmware")
                {
                    this.avrdudeActivity = "verifyfirmware";
                    this.AppendLog($"done\r\n[{this.AvrdudeFlashStep}/{this.AvrdudeSteps}] Verifying flash ...");
                    this.AvrdudeFlashStep++;
                }

                if (this.outputLineBuffer == "avrdude.exe done.  Thank you." && this.avrdudeActivity == "verifyfirmware")
                {
                    this.avrdudeActivity = string.Empty;
                    this.AppendLog(" done\r\n");
                }

                if (this.outputLineBuffer == "Reading | " || this.outputLineBuffer == "Writing | ")
                {
                    this.AppendVerbose(this.outputLineBuffer, false);
                }

                if (this.outputLineBuffer.StartsWith("Reading | #") || this.outputLineBuffer.StartsWith("Writing | #"))
                {
                    // Update the progress bar only when writing and verifying the firmware
                    if (data == '#' && (this.avrdudeActivity == "writefirmware" || this.avrdudeActivity == "verifyfirmware"))
                    {
                        // Convert number of hashes in string to progress bar percentage
                        int avrdudeProgress = (this.outputLineBuffer.Length - 10) * 2;
                        this.UpdateProgress(avrdudeProgress);
                    }

                    // Append the character to the output
                    this.AppendVerbose(((char)data).ToString(), false);
                }
            }
        }

        /// <summary>
        /// Appends a string to the verbose output text box.
        /// </summary>
        /// <param name="text">String to append.</param>
        public void AppendVerbose(char text)
        {
            // Check if we're called from another thread
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<char>(this.AppendVerbose), new object[] { text });
                return;
            }

            // Append the text
            this.textVerbose.AppendText(text.ToString());
        }

        /// <summary>
        /// Appends a string to the verbose output text box.
        /// </summary>
        /// <param name="text">String to append.</param>
        /// <param name="newline">Boolean indeicating whtether or not append a newline.</param>
        public void AppendVerbose(string text, bool newline = true)
        {
            // Check if we're called from another thread
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string, bool>(this.AppendVerbose), new object[] { text, newline });
                return;
            }

            // Append the text
            if (newline)
            {
                text = text + "\r\n";
            }

            this.textVerbose.AppendText(text);
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
            if (!FileUtils.CheckFirmwareFileSize(this.textFileName.Text))
            {
                this.EnableControls(true);
                return;
            }

            // Determine if we should use Maple device
            MapleDevice mapleResult = MapleDevice.FindMaple();

            // Determine if we should use a USBasp device
            UsbAspDevice usbaspResult = UsbAspDevice.FindUsbAsp();

            // Determine if the selected file contains USB / bootloader support
            bool firmwareSupportsUsb = FileUtils.CheckForUsbSupport(this.textFileName.Text);

            // Get the signature from the firmware file
            FileUtils.FirmwareFile fileSignature = FileUtils.GetFirmwareSignature(this.textFileName.Text);

            // Error if flashing non-USB firmware via native USB port
            if (mapleResult.DeviceFound && !firmwareSupportsUsb)
            {
                string msgBoxMessage = "The selected firmware file was compiled without USB support.\r\n\r\nFlashing this firmware would prevent the Multiprotocol module from functioning correctly.\r\n\r\nPlease select a different firmware file.";
                MessageBox.Show(msgBoxMessage, "Incompatible Firmware", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.EnableControls(true);
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
            else if (usbaspResult.DeviceFound == true && comPort == "USBasp")
            {
                if (fileSignature == null)
                {
                    string msgBoxMessage = "Unable to check the specified firmware file for compatibility with this upload method.";
                    MessageBox.Show(msgBoxMessage, "Incompatible Firmware", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.EnableControls(true);
                    return;
                }

                if (fileSignature.ModuleType != "AVR")
                {
                    string msgBoxMessage = "The specified firmware file is not compatible with this upload method.";
                    MessageBox.Show(msgBoxMessage, "Incompatible Firmware", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.EnableControls(true);
                    return;
                }

                await UsbAspDevice.WriteFlash(this, this.textFileName.Text, fileSignature.BootloaderSupport);
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
                    // Clear the output boxes
                    this.textActivity.Clear();
                    this.textVerbose.Clear();

                    // Set the text box to the selected file name
                    this.textFileName.Text = openFileDialog.FileName;

                    // Check the file size
                    if (!FileUtils.CheckFirmwareFileSize(this.textFileName.Text))
                    {
                        return;
                    }

                    // Get the signature from the firmware file
                    FileUtils.FirmwareFile fileDetails = FileUtils.GetFirmwareSignature(this.textFileName.Text);

                    // If we got details from the signature write them to the log window
                    if (fileDetails != null)
                    {
                        this.AppendLog($"Firmware File Name:       {this.textFileName.Text.Substring(this.textFileName.Text.LastIndexOf("\\") + 1)}\r\n");
                        this.AppendLog($"Multi Firmware Version:   {fileDetails.Version} ({fileDetails.ModuleType})\r\n");
                        this.AppendLog($"Multi Telemetry Type:     {fileDetails.MultiTelemetryType}\r\n");
                        this.AppendLog($"Invert Telemetry Enabled: {fileDetails.InvertTelemetry}\r\n");
                        this.AppendLog($"Flash from Radio Enabled: {fileDetails.CheckForBootloader}\r\n");
                        this.AppendLog($"Bootloader Enabled:       {fileDetails.BootloaderSupport}\r\n");
                        this.AppendLog($"Serial Debug Enabled:     {fileDetails.DebugSerial}\r\n");
                    }

                    // Check if the binary file contains USB / bootloader support
                    if (FileUtils.CheckForUsbSupport(this.textFileName.Text))
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

            // Value must be between 0 and 100
            if (value > 0 && value <= 100)
            {
                // Hack to make the progress bar jump to the next value rather than animate
                // The animation makes the bar look weird when it goes to 100% because the bar is still moving when the work is done.
                this.progressBar1.Value = value * 10;
                this.progressBar1.Value = (value * 10) - 1;
                this.progressBar1.Value = value * 10;
            }
        }

        /// <summary>
        /// Handles the show verbose output text box being checked or unchecked.
        /// Shows or hides the verbose output text box.
        /// </summary>
        private void ShowVerboseOutput_OnChange(object sender, EventArgs e)
        {
            if (this.showVerboseOutput.Checked == true)
            {
                this.tableLayoutPanel1.SuspendLayout();
                this.tableLayoutPanel1.RowStyles[0].SizeType = SizeType.Absolute;
                this.tableLayoutPanel1.RowStyles[0].Height = 126;
                this.tableLayoutPanel1.RowStyles[3].SizeType = SizeType.Percent;
                this.tableLayoutPanel1.RowStyles[3].Height = 50;
                this.MinimumSize = new System.Drawing.Size(500, 505);
                this.tableLayoutPanel1.ResumeLayout();
            }
            else
            {
                this.tableLayoutPanel1.SuspendLayout();
                this.MinimumSize = new System.Drawing.Size(500, 359);
                this.Size = new System.Drawing.Size(this.Width, 359);
                this.tableLayoutPanel1.RowStyles[0].SizeType = SizeType.Percent;
                this.tableLayoutPanel1.RowStyles[0].Height = 100;
                this.tableLayoutPanel1.RowStyles[3].SizeType = SizeType.Absolute;
                this.tableLayoutPanel1.RowStyles[3].Height = 0;
                this.tableLayoutPanel1.ResumeLayout();
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
