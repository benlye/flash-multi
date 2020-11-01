// -------------------------------------------------------------------------------
// <copyright file="FlashMulti.cs" company="Ben Lye">
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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Flash_Multi.Properties;

    /// <summary>
    /// The FlashMulti Form class.
    /// </summary>
    public partial class FlashMulti : Form
    {
        /// <summary>
        /// Indicates that no backup data is available.
        /// </summary>
        internal const int NoBackup = 0;

        /// <summary>
        /// Indicates that the last backup was from an Atmega328p.
        /// </summary>
        internal const int AtmegaBackup = 1;

        /// <summary>
        /// Indicates that the available backup was from an STM32F103 with dfu-util.
        /// </summary>
        internal const int Stm32BackupDfuUtil = 2;

        /// <summary>
        /// Indicates that the available backup was from an STM32F103 with stm32flash.
        /// </summary>
        internal const int Stm32BackupStm32Flash = 3;

        /// <summary>
        ///  The number of steps required for a flash.
        /// </summary>
        internal int FlashSteps = 0;

        /// <summary>
        /// The current flash step.
        /// </summary>
        internal int FlashStep = 1;

        /// <summary>
        /// Keep track of the type of backup we have.
        /// </summary>
        internal int BackupModuleType;

        /// <summary>
        /// Load the the interface language from the settings - allows the user to override the system language.
        /// </summary>
        private readonly string language = Properties.Settings.Default.Language;

        /// <summary>
        /// The window handle - used to create the progress bars for the driver installers.
        /// </summary>
        private IntPtr windowHandle;

        /// <summary>
        /// Keep track of whether or not the controls are globally disabled.
        /// </summary>
        private bool controlsDisabled = false;

        private bool enableDeviceDetection = true;

        /// <summary>
        /// Buffer for verbose output logging.
        /// </summary>
        private string outputLineBuffer = string.Empty;

        /// <summary>
        /// Keep track of the current avrdude activity.
        /// </summary>
        private string avrdudeActivity = string.Empty;

        /// <summary>
        /// Keep track of the temp file used for firmware backups from the module.
        /// </summary>
        private string firmwareBackupFileName = string.Empty;

        /// <summary>
        /// Keep track of the temp file used for EEPROM backups from an Atmega328p module.
        /// </summary>
        private string eepromBackupFileName = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlashMulti"/> class.
        /// </summary>
        public FlashMulti()
        {
            // Set the language
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(this.language);

            // Initialize
            this.InitializeComponent();

            // Include the version in the window title
            this.Text = string.Format("Flash Multi v{0}", Application.ProductVersion);

            // Set focus away from the textbox
            this.ActiveControl = this.linkLabel2;

            // Populate the list of serial ports
            this.PopulateComPorts();

            // Disable the Upload button until we're ready
            this.buttonUpload.Enabled = false;

            // Set the serial speed from the settings
            if (Properties.Settings.Default.SerialBaudRate == 57600)
            {
                this.toolStripMenuItemBaudRate57600.Checked = true;
            }
            else
            {
                this.toolStripMenuItemBaudRate115200.Checked = true;
            }

            // Set 'Run after upload' from the settings
            this.runAfterUploadToolStripMenuItem.Checked = Properties.Settings.Default.RunAfterUpload;

            // Set 'Enable device detection' from the settings
            this.enableDeviceDetection = Properties.Settings.Default.EnableDeviceDetection;
            this.enableDeviceDetectionToolStripMenuItem.Checked = this.enableDeviceDetection;

            // Set 'Enable Flash Verification' from the settings
            this.disableCompatibilityCheckToolStripMenuItem.Checked = Settings.Default.DisableFlashVerification;

            // Set the 'Bootloader / USB Port' mode from the settings
            this.comPortUsbModeToolStripMenuItem.Checked = Settings.Default.ErrorIfNoUSB;
            this.stickyDfuUsbModeToolStripMenuItem.Checked = !Settings.Default.ErrorIfNoUSB;

            // Hide the verbose output panel and set the height of the other panel
            int initialHeight = 215;
            this.splitContainer1.Panel2Collapsed = true;
            this.splitContainer1.Panel1MinSize = 225;
            this.splitContainer1.Size = new System.Drawing.Size(this.splitContainer1.Width, initialHeight);
            this.splitContainer1.SplitterDistance = initialHeight;
            this.Size = new System.Drawing.Size(this.Width, 440);

            // Register a handler to check for a new version when the form is shown the first time
            this.Shown += this.FlashMulti_Shown;

            // Register a handler to run on loading the form
            this.Load += this.FlashMulti_Load;

            // Store the windowHandle
            this.windowHandle = this.Handle;

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
            if (eventArgs.Data != "error resetting after download: usb_reset: could not reset device, win error: A device which does not exist was specified.")
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
                // Suppress writing the dfu-util finished line, and the avrdude reading/writing finished lines
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
                    this.AppendLog($"[{this.FlashStep}/{this.FlashSteps}] Erasing flash ... ");
                    this.avrdudeActivity = "erasing";
                    this.FlashStep++;
                }

                if (this.outputLineBuffer == "avrdude.exe: writing lock (1 bytes):" && this.avrdudeActivity == "erasing")
                {
                    this.AppendLog($"done\r\n[{this.FlashStep}/{this.FlashSteps}] Setting fuses ... ");
                    this.avrdudeActivity = "fuses";
                    this.FlashStep++;
                }

                if (this.outputLineBuffer == "avrdude.exe: reading flash memory:")
                {
                    this.AppendLog($"[{this.FlashStep}/{this.FlashSteps}] Reading flash ... ");
                    this.avrdudeActivity = "readflash";
                    this.FlashStep++;
                }

                if (this.outputLineBuffer == "avrdude.exe: reading eeprom memory:")
                {
                    this.AppendLog($"done\r\n[{this.FlashStep}/{this.FlashSteps}] Reading EEPROM ... ");
                    this.avrdudeActivity = "readeeprom";
                    this.FlashStep++;
                }

                if (this.outputLineBuffer.StartsWith("avrdude.exe: writing flash ") && this.outputLineBuffer.EndsWith("):"))
                {
                    if (this.avrdudeActivity == "writebootloader" && this.FlashSteps == 5)
                    {
                        // Writing firmware after bootloader
                        this.AppendLog($"done\r\n[{this.FlashStep}/{this.FlashSteps}] Writing firmware ... ");
                        this.avrdudeActivity = "writefirmware";
                        this.FlashStep++;
                    }
                    else if (this.avrdudeActivity == "fuses" && (this.FlashSteps == 4 || this.FlashSteps == 6))
                    {
                        // Writing firmware after fuses
                        this.AppendLog($"done\r\n[{this.FlashStep}/{this.FlashSteps}] Writing flash ... ");
                        this.avrdudeActivity = "writefirmware";
                        this.FlashStep++;
                    }
                    else if (this.avrdudeActivity == "fuses" && this.FlashSteps == 5)
                    {
                        // Writing bootloader after fuses
                        this.AppendLog($"done\r\n[{this.FlashStep}/{this.FlashSteps}] Writing bootloader ... ");
                        this.avrdudeActivity = "writebootloader";
                        this.FlashStep++;
                    }
                }

                if (this.outputLineBuffer == "avrdude.exe: reading on-chip flash data:" && this.avrdudeActivity == "writefirmware")
                {
                    this.avrdudeActivity = "verifyfirmware";
                    this.AppendLog($"done\r\n[{this.FlashStep}/{this.FlashSteps}] Verifying flash ...");
                    this.FlashStep++;
                }

                if (this.outputLineBuffer.StartsWith("avrdude.exe: writing eeprom ") && this.outputLineBuffer.EndsWith("):"))
                {
                    if (this.FlashStep > 1)
                    {
                        this.AppendLog($"done\r\n");
                    }

                    this.avrdudeActivity = "writeeeprom";
                    this.AppendLog($"[{this.FlashStep}/{this.FlashSteps}] Writing EEPROM ...");
                    this.FlashStep++;
                }

                if (this.outputLineBuffer == "avrdude.exe: reading on-chip eeprom data:" && this.avrdudeActivity == "writeeeprom")
                {
                    this.avrdudeActivity = "verifyeeprom";
                    this.AppendLog($"done\r\n[{this.FlashStep}/{this.FlashSteps}] Verifying EEPROM ...");
                    this.FlashStep++;
                }

                if (this.outputLineBuffer == "avrdude.exe done.  Thank you." && this.avrdudeActivity != string.Empty)
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
                    if (data == '#' && (this.avrdudeActivity == "writefirmware" || this.avrdudeActivity == "verifyfirmware" || this.avrdudeActivity == "readflash" || this.avrdudeActivity == "readeeprom"))
                    {
                        // Convert number of hashes in string to progress bar percentage
                        int avrdudeProgress = (this.outputLineBuffer.Length - 10) * 2;
                        this.UpdateProgress(avrdudeProgress);
                    }

                    // Append the character to the output, unless it's a carriage return
                    if (data != '\r')
                    {
                        this.AppendVerbose(((char)data).ToString(), false);
                    }
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
        /// <param name="newline">Boolean indicating whether or not to append a newline.</param>
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
        public async void EnableControls(bool arg)
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

            // Set the global state
            this.controlsDisabled = !arg;

            if (arg)
            {
                // Populate the COM ports
                await this.PopulateComPortsAsync();
            }

            // Toggle the controls
            Debug.WriteLine("Toggling controls");
            this.buttonRefresh.Enabled = arg;
            this.buttonUpload.Enabled = arg;
            this.buttonBrowse.Enabled = arg;
            this.buttonRefresh.Enabled = arg;
            this.buttonSerialMonitor.Enabled = arg;
            this.buttonRead.Enabled = arg;
            this.buttonSaveBackup.Enabled = arg;
            this.buttonErase.Enabled = arg;
            this.textFileName.Enabled = arg;
            this.comPortSelector.Enabled = arg;

            // Toggle the menus
            this.fileToolStripMenuItem.Enabled = arg;
            this.advancedToolStripMenuItem.Enabled = arg;
            this.helpToolStripMenuItem.Enabled = arg;

            // Check a couple of things if we're re-enabling
            if (arg)
            {
                // Check if the Upload button can be enabled
                this.CheckControlsAsync();
            }
        }

        /// <summary>
        /// Async method to checks which controls should be enabled or disabled and sets their states accordingly.
        /// Called by changes to the file name or COM port selector.
        /// </summary>
        public async void CheckControlsAsync()
        {
            await Task.Run(() => this.CheckControls());
        }

        /// <summary>
        /// Checks which controls should be enabled or disabled and sets their states accordingly.
        /// Called by changes to the file name or COM port selector.
        /// </summary>
        public async void CheckControls()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(this.CheckControls));
                return;
            }

            if (this.controlsDisabled)
            {
                // Controls are globally disabled
                return;
            }

            Debug.WriteLine("Checking controls");

            // Get the selected COM port
            var selectedPort = this.GetSelectedPort();

            if (this.textFileName.Text != string.Empty && this.GetSelectedPort() != null)
            {
                this.buttonUpload.Enabled = true;
            }
            else
            {
                this.buttonUpload.Enabled = false;
            }

            if (selectedPort != null && selectedPort.ToString() != "USBasp" && selectedPort.ToString() != "DFU Device")
            {
                this.buttonSerialMonitor.Enabled = true;
                this.runFirmwareToolStripMenuItem.Enabled = true;
            }
            else
            {
                this.buttonSerialMonitor.Enabled = false;
                this.runFirmwareToolStripMenuItem.Enabled = false;
                this.buttonRead.Enabled = false;
            }

            if (this.GetSelectedPort() != null)
            {
                this.buttonRead.Enabled = true;
                this.buttonErase.Enabled = true;
            }
            else
            {
                this.buttonRead.Enabled = false;
                this.buttonErase.Enabled = false;
            }

            if (this.BackupModuleType != NoBackup)
            {
                this.buttonSaveBackup.Enabled = true;
            }
            else
            {
                this.buttonSaveBackup.Enabled = false;
            }

            // Check for a Maple device
            MapleDevice mapleCheck = await Task.Run(() => MapleDevice.FindMaple());
            if (mapleCheck.DeviceFound)
            {
                string mapleComPort = await Task.Run(() => MapleDevice.GetMapleComPort());

                // If a Maple Serial device is present and the selected port is the Maple COM port, enable the Reset to DFU Mode menu item
                if (mapleCheck.DeviceFound && mapleCheck.UsbMode == true && selectedPort != null && selectedPort.ToString() == mapleComPort)
                {
                    this.resetToDFUModeToolStripMenuItem.Enabled = true;
                    this.runFirmwareToolStripMenuItem.Enabled = false;
                }
                else
                {
                    this.resetToDFUModeToolStripMenuItem.Enabled = false;
                }

                // If a Maple DFU is present and the selected port is the Maple COM port or DFU device, enable the upload bootloader menu item
                if ((mapleCheck.DeviceFound && mapleCheck.UsbMode == true && selectedPort != null && selectedPort.ToString() == mapleComPort)
                    || (mapleCheck.DeviceFound && mapleCheck.DfuMode == true && selectedPort != null && selectedPort.ToString() == "DFU Device"))
                {
                    this.upgradeBootloaderToolStripMenuItem.Enabled = true;
                }
                else
                {
                    this.upgradeBootloaderToolStripMenuItem.Enabled = false;
                }
            }
            else
            {
                this.resetToDFUModeToolStripMenuItem.Enabled = false;
                this.upgradeBootloaderToolStripMenuItem.Enabled = false;
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
                // Don't do anything if the controls are disabled
                if (this.enableDeviceDetection)
                {
                    switch ((int)m.WParam)
                    {
                        case UsbNotification.DbtDeviceremovecomplete:
                            // Update the COM port list
                            Debug.WriteLine($"USB device removed");
                            this.enableDeviceDetection = false;

                            // Wait 2s before populating the COM port selector
                            this.DelayedExecute(() => this.TimerCallback(), 2000);

                            break;
                        case UsbNotification.DbtDevicearrival:
                            // Update the COM port list
                            Debug.WriteLine($"USB device attached");
                            this.enableDeviceDetection = false;

                            // Wait 2s before populating the COM port selector
                            this.DelayedExecute(() => this.TimerCallback(), 2000);

                            break;
                    }
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
        /// Method to execute an action after a delay.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="timeoutInMilliseconds">The delay in ms before the action is executed.</param>
        private async void DelayedExecute(Action action, int timeoutInMilliseconds)
        {
            await Task.Delay(timeoutInMilliseconds);
            action();
        }

        /// <summary>
        /// Populates the COM ports after a USB device was added or removed.
        /// </summary>
        private async void TimerCallback()
        {
            Debug.WriteLine("Device detection timer elapsed");

            // Revert the device detection setting
            this.enableDeviceDetection = Settings.Default.EnableDeviceDetection;

            // Populate the COM port selector
            await this.PopulateComPortsAsync();
        }

        /// <summary>
        /// Handles the user clicking the Read button.
        /// </summary>
        private async void ButtonRead_Click(object sender, EventArgs e)
        {
            // Read the module
            await this.ReadModule();
        }

        /// <summary>
        /// Handles the user clicking the Write button.
        /// </summary>
        private async void ButtonWrite_Click(object sender, EventArgs e)
        {
            // Write the module
            await this.WriteModule();
        }

        /// <summary>
        /// Handles the user clicking the Browse button.
        /// </summary>
        private void ButtonBrowse_Click(object sender, EventArgs e)
        {
            // Choose a file to flash
            this.OpenFile();
        }

        /// <summary>
        /// Handles a change in the COM port selection dropdown.
        /// </summary>
        private void ComPortSelector_SelectionChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("COM port selector changed");
            if (this.buttonRefresh.Enabled)
            {
                // Check if the Upload button should be enabled yet
                this.CheckControlsAsync();
            }
        }

        /// <summary>
        /// Handles input in the firmware file name text box.
        /// </summary>
        private void TextFileName_OnChange(object sender, EventArgs e)
        {
            // Check if the Upload button should be enabled yet
            this.CheckControlsAsync();
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
                // Grow the window by the height of the verbose panel and splitter bar
                int oldHeight = this.splitContainer1.Panel1.Height;
                int newHeight = this.Height + 150;
                this.splitContainer1.Panel2Collapsed = false;
                this.splitContainer1.Panel2MinSize = 150;
                this.Size = new System.Drawing.Size(this.Width, newHeight + this.splitContainer1.SplitterWidth);
                this.splitContainer1.SplitterDistance = oldHeight;
                this.MinimumSize = new System.Drawing.Size(570, 593);
            }
            else
            {
                // Shrink the window by the height of the verbose panel and the splitter bar
                this.MinimumSize = new System.Drawing.Size(570, 440);
                int newHeight = this.Height - this.splitContainer1.Panel2.Height;
                this.Size = new System.Drawing.Size(this.Width, newHeight - this.splitContainer1.SplitterWidth);

                // Hide the panel
                this.splitContainer1.Panel2Collapsed = true;
                this.splitContainer1.Panel2MinSize = 0;
            }
        }

        /// <summary>
        /// Handles the refresh button being clicked.
        /// Updates the list of COM ports in the drop down.
        /// </summary>
        private async void ButtonRefresh_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("ButtonRefresh clicked");
            this.buttonRefresh.Enabled = false;
            await this.PopulateComPortsAsync();
            this.buttonRefresh.Enabled = true;
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
            this.OpenLink("https://downloads.multi-module.org/");
        }

        /// <summary>
        /// Handles the Serial Monitor button being clicked.
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
                SerialMonitor serialMonitor = new SerialMonitor(this.GetSelectedPort().ToString());
                serialMonitor.Show();
            }
        }

        /// <summary>
        /// Handles the Backup button being clicked.
        /// </summary>
        private void ButtonSaveBackup_Click(object sender, EventArgs e)
        {
            if (this.firmwareBackupFileName != string.Empty)
            {
                // Disable the controls
                this.EnableControls(false);

                // Create the backup
                FileUtils.SaveFirmwareBackup(this, this.firmwareBackupFileName, this.eepromBackupFileName);

                // Re-enable the controls
                this.EnableControls(true);
            }
            else
            {
                using (new CenterWinDialog(this))
                {
                    MessageBox.Show("No backup file. Read the MULTI-Module first.", "Save Backup", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Handles the user clicking the Erase button.
        /// </summary>
        private async void ButtonErase_Click(object sender, EventArgs e)
        {
            // Erase the module
            await this.EraseModule();
        }

        /// <summary>
        /// Handles the user clicking the Exit menu item.
        /// </summary>
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles the user clicking the Install USB Drivers menu item.
        /// </summary>
        private async void InstallUSBDriversToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Install the USB drivers
            await this.InstallUsbDrivers();
        }

        /// <summary>
        /// Handles the user clicking the menu item to upgrade the booloader.
        /// </summary>
        private async void UpgradeBootloaderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Upgrade the bootloader
            await this.UpgradeBootloader();
        }

        /// <summary>
        /// Handles the user setting the serial baud rate to 57600.
        /// </summary>
        private void ToolStripMenuItemBaudRate57600_Click(object sender, EventArgs e)
        {
            Settings.Default.SerialBaudRate = 57600;
            Settings.Default.Save();
            this.toolStripMenuItemBaudRate57600.Checked = true;
            this.toolStripMenuItemBaudRate115200.Checked = false;
            this.toolStripMenuItemBaudRate500000.Checked = false;
        }

        /// <summary>
        /// Handles the user setting the serial baud rate to 115200.
        /// </summary>
        private void ToolStripMenuItemBaudRate115200_Click(object sender, EventArgs e)
        {
            Settings.Default.SerialBaudRate = 115200;
            Settings.Default.Save();
            this.toolStripMenuItemBaudRate57600.Checked = false;
            this.toolStripMenuItemBaudRate115200.Checked = true;
            this.toolStripMenuItemBaudRate500000.Checked = false;
        }

        /// <summary>
        /// Handles the user setting the serial baud rate to 500000.
        /// </summary>
        private void ToolStripMenuItemBaudRate500000_Click(object sender, EventArgs e)
        {
            Settings.Default.SerialBaudRate = 500000;
            Settings.Default.Save();
            this.toolStripMenuItemBaudRate57600.Checked = false;
            this.toolStripMenuItemBaudRate115200.Checked = false;
            this.toolStripMenuItemBaudRate500000.Checked = true;
        }

        /// <summary>
        /// Handles the user toggling the Run After Upload menu setting.
        /// </summary>
        private void RunAfterUploadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.RunAfterUpload = this.runAfterUploadToolStripMenuItem.Checked;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Handles the user toggling the Device Detection menu setting.
        /// </summary>
        private void EnableDeviceDetectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.enableDeviceDetection = this.enableDeviceDetectionToolStripMenuItem.Checked;
            Properties.Settings.Default.EnableDeviceDetection = this.enableDeviceDetectionToolStripMenuItem.Checked;
            Properties.Settings.Default.Save();
        }

        private void DisableCompatibilityCheckToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.DisableFlashVerification = this.disableCompatibilityCheckToolStripMenuItem.Checked;
            Settings.Default.Save();
        }

        /// <summary>
        /// Handles setting the Bootloader / USB Port mode to 'legacy'.
        /// </summary>
        private void ComPortUsbModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.stickyDfuUsbModeToolStripMenuItem.Checked = false;
            this.comPortUsbModeToolStripMenuItem.Checked = true;
            Settings.Default.ErrorIfNoUSB = true;
            Settings.Default.WarnIfNoUSB = true;
            Settings.Default.RunAfterUpload = true;
            Settings.Default.Save();
        }

        /// <summary>
        /// Handles setting the Bootloader / USB Port mode to 'new'.
        /// </summary>
        private void StickyDfuUsbModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.stickyDfuUsbModeToolStripMenuItem.Checked = true;
            this.comPortUsbModeToolStripMenuItem.Checked = false;
            Settings.Default.ErrorIfNoUSB = false;
            Settings.Default.RunAfterUpload = false;
            Settings.Default.Save();
        }

        /// <summary>
        /// Handles the user clicking the Check for Update menu item.
        /// </summary>
        private void CheckForUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateCheck.DoCheck(this, true);
        }

        /// <summary>
        /// Handles the user clicking the Documentation menu item.
        /// </summary>
        private void DocumentationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.OpenLink("https://github.com/benlye/flash-multi");
        }

        /// <summary>
        /// Handles the user clicking the Download Firmware menu item.
        /// </summary>
        private void DownloadFirmwareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.OpenLink("https://downloads.multi-module.org/");
        }

        /// <summary>
        /// Handles the user clicking the Reset to DFU Mode menu item.
        /// </summary>
        private async void ResetToDFUModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Reset the module to DFU mode
            await this.ResetToDfuMode();
        }

        /// <summary>
        /// Handles the user clicking the Run Firmare menu item.
        /// </summary>
        private async void RunFirmwareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Run the module firmware
            await this.RunMultiFirmware();
        }

        /// <summary>
        /// Calls the device-specific method to read the module.
        /// </summary>
        private async Task ReadModule()
        {
            // Disable the buttons until this flash attempt is complete
            Debug.WriteLine("Disabling the controls...");
            this.EnableControls(false);

            // Clear the output box
            Debug.WriteLine("Clearing the output textboxes...");
            this.textActivity.Clear();
            this.textVerbose.Clear();
            this.progressBar1.Value = 0;
            this.outputLineBuffer = string.Empty;

            // Discard the last backup
            this.BackupModuleType = NoBackup;
            this.firmwareBackupFileName = string.Empty;
            this.eepromBackupFileName = string.Empty;

            // Determine if we should use Maple device
            MapleDevice mapleResult = MapleDevice.FindMaple();

            // Determine if we should use a USBasp device
            UsbAspDevice usbaspResult = UsbAspDevice.FindUsbAsp();

            // Get the selected COM port
            string comPort = this.GetSelectedPort().ToString();

            // Generate a temp file to read the firmware into
            string tempFirmwareFileName = Path.GetTempFileName();
            Debug.WriteLine($"TEMP firmware file: {tempFirmwareFileName}");

            string tempEepromFilename = string.Empty;

            // Do the selected flash using the appropriate method
            bool readSucceeded;
            if (mapleResult.DeviceFound == true)
            {
                Debug.WriteLine($"Maple device found in {mapleResult.Mode} mode");

                // Set the backup type
                this.BackupModuleType = Stm32BackupDfuUtil;

                // Make the backup
                readSucceeded = await MapleDevice.ReadFlash(this, tempFirmwareFileName, comPort);
            }
            else if (usbaspResult.DeviceFound == true && comPort == "USBasp")
            {
                // Set the backup type
                this.BackupModuleType = AtmegaBackup;

                // Generate a temp file to read the EEPROM into
                tempEepromFilename = Path.GetTempFileName();
                Debug.WriteLine($"TEMP EEPROM file: {tempEepromFilename}");

                // Make the backup
                readSucceeded = await UsbAspDevice.ReadFlash(this, tempFirmwareFileName, tempEepromFilename);
            }
            else
            {
                // Set the backup type
                this.BackupModuleType = Stm32BackupStm32Flash;
                readSucceeded = await SerialDevice.ReadFlash(this, tempFirmwareFileName, comPort);
            }

            if (readSucceeded)
            {
                byte[] eepromData = { };

                // Get the file size
                long length = new System.IO.FileInfo(tempFirmwareFileName).Length;

                // Parse the firmware file
                if (length > 0)
                {
                    // Get the signature from the firmware file
                    FileUtils.FirmwareFile fileDetails = FileUtils.GetFirmwareSignature(tempFirmwareFileName);

                    // Check for USB Serial support
                    bool usbSerialSupport = FileUtils.CheckForUsbSupport(tempFirmwareFileName);

                    // If we got details from the signature write them to the log window
                    if (fileDetails != null)
                    {
                        this.AppendLog($"Multi Firmware Version:   {fileDetails.Version} ({fileDetails.ModuleType})\r\n");
                        this.AppendLog($"Firmware Target:          {fileDetails.ModuleSubType} ({fileDetails.ModuleMcuFlashSizeKb}KB)\r\n");
                        this.AppendLog($"Expected Channel Order:   {fileDetails.ChannelOrder}\r\n");
                        this.AppendLog($"Telemetry Type:           {fileDetails.MultiTelemetryType}\r\n");
                        this.AppendLog($"Invert Telemetry Enabled: {fileDetails.InvertTelemetry}\r\n");
                        this.AppendLog($"Flash from Radio Enabled: {fileDetails.CheckForBootloader}\r\n");
                        this.AppendLog($"Bootloader Enabled:       {fileDetails.BootloaderSupport}\r\n");
                        this.AppendLog($"USB Serial Support:       {usbSerialSupport}\r\n");
                        this.AppendLog($"Serial Debug Enabled:     {fileDetails.DebugSerial}\r\n");
                    }
                    else
                    {
                        // Check if the firmware actually contains any data
                        if (Stm32EepromUtils.FirmwareIsEmpty(tempFirmwareFileName))
                        {
                            this.AppendLog("MULTI-Module flash did not contain any data.\r\n");
                        }
                        else
                        {
                            this.AppendLog($"Firmware signature not found; extended information is not available. This is expected for modules with firmware prior to v1.2.1.79.\r\n");
                        }
                    }

                    // Get the EEPROM data from an STM32 flash backup
                    if (this.BackupModuleType == Stm32BackupDfuUtil || this.BackupModuleType == Stm32BackupStm32Flash)
                    {
                        if (!Stm32EepromUtils.EepromIsEmpty(tempFirmwareFileName))
                        {
                            eepromData = Stm32EepromUtils.GetEepromDataFromBackup(tempFirmwareFileName);
                        }
                    }

                    // Keep track of the temp file
                    this.firmwareBackupFileName = tempFirmwareFileName;
                }
                else
                {
                    this.AppendLog("MULTI-Module flash did not contain any data.\r\n");
                }

                // Get the EEPROM data from an Atmega EEPROM backup
                if (this.BackupModuleType == AtmegaBackup)
                {
                    this.eepromBackupFileName = tempEepromFilename;
                    eepromData = AtmegaEepromUtils.GetEepromDataFromBackup(tempEepromFilename);
                }

                // Parse the EEPROM data
                if (eepromData.Length > 0)
                {
                    uint globalId;
                    if (tempEepromFilename != string.Empty)
                    {
                        globalId = AtmegaEepromUtils.ReadGlobalId(eepromData);
                    }
                    else
                    {
                        globalId = Stm32EepromUtils.ReadGlobalId(eepromData);
                    }

                    if (globalId > 0)
                    {
                        this.AppendLog($"\r\nEEPROM Global ID:         0x{globalId:X8}\r\n");
                    }
                    else
                    {
                        this.AppendLog($"\r\nEEPROM Global ID:         Not found\r\n");
                    }
                }
                else
                {
                    this.AppendLog($"\r\nMULTI-Module EEPROM did not contain any data.\r\n");
                }

                this.AppendLog("\r\nMULTI-Module read successfully");
            }
            else
            {
                this.BackupModuleType = NoBackup;
            }

            // Re-enable the controls
            this.CheckControlsAsync();

            // Populate the COM ports in case they changed
            await this.PopulateComPortsAsync();
        }

        /// <summary>
        /// Calls the device-specific method to erase the module.
        /// </summary>
        private async Task EraseModule()
        {
            // Disable the buttons until this flash attempt is complete
            Debug.WriteLine("Disabling the controls...");
            this.EnableControls(false);

            // Prompt for confirmation
            DialogResult eraseConfirm;
            using (new CenterWinDialog(this))
            {
                eraseConfirm = MessageBox.Show("Are you sure you want to erase the MULTI-Module?", "Erase Module", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            }

            if (eraseConfirm != DialogResult.Yes)
            {
                this.EnableControls(true);
                return;
            }

            // Ask if we should erase the EEPROM as well
            bool eraseEeprom = false;
            DialogResult eraseEepromConfirm;
            using (new CenterWinDialog(this))
            {
                eraseEepromConfirm = MessageBox.Show("Also erase the EEPROM data?", "Erase Module", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            }

            if (eraseEepromConfirm == DialogResult.Yes)
            {
                eraseEeprom = true;
            }

            // Prompt for second confirmation
            DialogResult eraseReallyConfirm;
            using (new CenterWinDialog(this))
            {
                eraseReallyConfirm = MessageBox.Show("Are you really sure you want to erase the MULTI-Module?\r\nThis action cannot be undone.", "Erase Module", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
            }

            if (eraseReallyConfirm != DialogResult.Yes)
            {
                this.EnableControls(true);
                return;
            }

            // Clear the output box
            Debug.WriteLine("Clearing the output textboxes...");
            this.textActivity.Clear();
            this.textVerbose.Clear();
            this.progressBar1.Value = 0;
            this.outputLineBuffer = string.Empty;

            // Determine if we should use Maple device
            MapleDevice mapleResult = MapleDevice.FindMaple();

            // Determine if we should use a USBasp device
            UsbAspDevice usbaspResult = UsbAspDevice.FindUsbAsp();

            // Get the selected COM port
            string comPort = this.GetSelectedPort().ToString();

            // Do the selected flash using the appropriate method
            bool eraseSucceeded;
            if (mapleResult.DeviceFound == true)
            {
                Debug.WriteLine($"Maple device found in {mapleResult.Mode} mode");
                eraseSucceeded = await MapleDevice.EraseFlash(this, comPort, eraseEeprom);
            }
            else if (usbaspResult.DeviceFound == true && comPort == "USBasp")
            {
                eraseSucceeded = await UsbAspDevice.EraseFlash(this, eraseEeprom);
            }
            else
            {
                eraseSucceeded = await SerialDevice.EraseFlash(this, comPort, eraseEeprom);
            }

            // Re-enable the controls
            this.CheckControlsAsync();

            // Populate the COM ports in case they changed
            await this.PopulateComPortsAsync();
        }

        /// <summary>
        /// Checks the selected file, then calls the device-specific method to write the file to the module.
        /// </summary>
        private async Task WriteModule()
        {
            // Disable the buttons until this flash attempt is complete
            Debug.WriteLine("Disabling the controls...");
            this.EnableControls(false);

            // Discard the last backup
            this.BackupModuleType = NoBackup;
            this.firmwareBackupFileName = string.Empty;
            this.eepromBackupFileName = string.Empty;

            // Clear the output box
            Debug.WriteLine("Clearing the output textboxes...");
            this.textActivity.Clear();
            this.textVerbose.Clear();
            this.progressBar1.Value = 0;
            this.outputLineBuffer = string.Empty;

            // Check if the file extension matches our expectation
            if (!(this.textFileName.Text.EndsWith(".bin") || this.textFileName.Text.EndsWith(".eep")))
            {
                this.AppendLog(string.Format("Unknown file type '{0}'", this.textFileName.Text.Substring(this.textFileName.Text.Length - 4, 4)));
                using (new CenterWinDialog(this))
                {
                    MessageBox.Show("Unknown file type '{0}'. File extension must be '.bin' or '.eep'.", "Write Firmware", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                this.EnableControls(true);
                return;
            }

            // Check if the file exists
            if (!File.Exists(this.textFileName.Text))
            {
                this.AppendLog(string.Format("File {0} does not exist", this.textFileName.Text));
                using (new CenterWinDialog(this))
                {
                    MessageBox.Show("Firmware file does not exist.", "Write Firmware", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                this.EnableControls(true);
                return;
            }

            // Check the file size
            if (!FileUtils.CheckFirmwareFileSize(this.textFileName.Text))
            {
                this.EnableControls(true);
                return;
            }

            // Check if the file contains EEPROM data
            bool firmwareContainsEeprom = false;
            if (this.textFileName.Text.EndsWith(".bin"))
            {
                byte[] eePromData = Stm32EepromUtils.GetEepromDataFromBackup(this.textFileName.Text);
                if (eePromData != null && Stm32EepromUtils.FindValidPage(eePromData) >= 0)
                {
                    firmwareContainsEeprom = true;
                }
            }
            else if (this.textFileName.Text.EndsWith(".eep"))
            {
                firmwareContainsEeprom = true;
            }

            // Warn if we're restoring EEPROM data
            if (firmwareContainsEeprom)
            {
                DialogResult overwriteEeprom;
                using (new CenterWinDialog(this))
                {
                    overwriteEeprom = MessageBox.Show("The selected file contains EEPROM data. Continuing will overwrite the existing EEPROM data in the flash memory.", "Overwrite EEPROM", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                }

                if (overwriteEeprom == DialogResult.Cancel)
                {
                    this.EnableControls(true);
                    return;
                }
            }

            // Determine if we should use Maple device
            MapleDevice mapleResult = MapleDevice.FindMaple();

            // Determine if we should use a USBasp device
            UsbAspDevice usbaspResult = UsbAspDevice.FindUsbAsp();

            // Determine if the selected file contains USB / bootloader support
            bool firmwareSupportsUsb = FileUtils.CheckForUsbSupport(this.textFileName.Text);

            // Get the signature from the firmware file
            FileUtils.FirmwareFile fileSignature = FileUtils.GetFirmwareSignature(this.textFileName.Text);

            // Stop if this is a firmware file without a signature - we can't be sure that it will match the module
            if (this.textFileName.Text.EndsWith(".bin") && fileSignature == null)
            {
                string msgBoxMessage = "Unable to validate the selected firmware file. The firmware signature is missing.";
                using (new CenterWinDialog(this))
                {
                    MessageBox.Show(msgBoxMessage, "Incompatible Firmware", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                this.EnableControls(true);
                return;
            }

            // Error if flashing firmware without bootloader support firmware via native USB port
            if (mapleResult.DeviceFound && !fileSignature.BootloaderSupport)
            {
                string msgBoxMessage = "The selected firmware file was compiled without bootloader support\r\n\r\nThis firmware cannot be written to the connected MULTI-Module.";
                using (new CenterWinDialog(this))
                {
                    MessageBox.Show(msgBoxMessage, "Incompatible Firmware", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                this.EnableControls(true);
                return;
            }

            // Error or warn if flashing non-USB firmware via native USB port and we're not configured for hte new bootloader
            if (mapleResult.DeviceFound && !firmwareSupportsUsb)
            {
                if (Settings.Default.ErrorIfNoUSB)
                {
                    // Error if the new bootloader hasn't been enabled
                    UsbSupportErrorDialog usbSupportErrorDialog = new UsbSupportErrorDialog();
                    usbSupportErrorDialog.ShowDialog();
                    this.EnableControls(true);
                    return;
                }
                else
                {
                    if (Settings.Default.WarnIfNoUSB)
                    {
                        // Warn that bootloader update is required if the firmware file does not have USB support
                        UsbSupportWarningDialog usbSupportWarning = new UsbSupportWarningDialog();
                        var warnResult = usbSupportWarning.ShowDialog();

                        if (warnResult != DialogResult.OK)
                        {
                            this.EnableControls(true);
                            return;
                        }
                    }
                }
            }

            // Get the selected COM port
            string comPort = this.GetSelectedPort().ToString();

            // Clear the backup file name - the backup will be invalid after an upload
            this.firmwareBackupFileName = string.Empty;

            // Do the selected flash using the appropriate method
            if (mapleResult.DeviceFound == true)
            {
                Debug.WriteLine($"Maple device found in {mapleResult.Mode} mode\r\n");
                await MapleDevice.WriteFlash(this, this.textFileName.Text, comPort, this.runAfterUploadToolStripMenuItem.Checked);
            }
            else if (usbaspResult.DeviceFound == true && comPort == "USBasp")
            {
                // Stop if this is a firmware file without a signature - we can't be sure that it's for an Atmega328p module
                if (this.textFileName.Text.EndsWith(".bin") && fileSignature == null)
                {
                    string msgBoxMessage = "Unable to check the specified firmware file for compatibility with this upload method.";
                    using (new CenterWinDialog(this))
                    {
                        MessageBox.Show(msgBoxMessage, "Incompatible Firmware", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    this.EnableControls(true);
                    return;
                }

                // Stop if this is a bin file with a signature that doesn't match an Atmega328p module
                if (this.textFileName.Text.EndsWith(".bin") && fileSignature.ModuleType != "AVR")
                {
                    string msgBoxMessage = "The selected firmware file is not compatible with this upload method.";
                    using (new CenterWinDialog(this))
                    {
                        MessageBox.Show(msgBoxMessage, "Incompatible Firmware", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    this.EnableControls(true);
                    return;
                }

                if (this.textFileName.Text.EndsWith(".eep"))
                {
                    // Writing EEPROM data
                    await UsbAspDevice.WriteEeprom(this, this.textFileName.Text);
                }
                else
                {
                    // Writing flash data
                    await UsbAspDevice.WriteFlash(this, this.textFileName.Text, fileSignature.BootloaderSupport);
                }
            }
            else
            {
                await SerialDevice.WriteFlash(this, this.textFileName.Text, comPort, fileSignature.BootloaderSupport, firmwareContainsEeprom, this.runAfterUploadToolStripMenuItem.Checked);
            }

            // Populate the COM ports in case they changed
            await this.PopulateComPortsAsync();
        }

        /// <summary>
        /// Selects a firmware file to flash.
        /// </summary>
        private void OpenFile()
        {
            // Create the file open dialog
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // Title for the dialog
                openFileDialog.Title = Strings.dialogTitleFileOpen;

                // Filter for .bin files
                openFileDialog.Filter = "Firmware Files (*.bin)|*.bin|EEPROM Files (*.eep)|*.eep|All files (*.*)|*.*";

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

                    uint globalId = 0;
                    if (openFileDialog.FileName.EndsWith(".bin"))
                    {
                        // Check for USB Serial support
                        bool usbSerialSupport = FileUtils.CheckForUsbSupport(this.textFileName.Text);

                        // Get the signature from the firmware file
                        FileUtils.FirmwareFile fileDetails = FileUtils.GetFirmwareSignature(this.textFileName.Text);

                        // If we got details from the signature write them to the log window
                        if (fileDetails != null)
                        {
                            this.AppendLog($"Firmware File Name:       {this.textFileName.Text.Substring(this.textFileName.Text.LastIndexOf("\\") + 1)}\r\n");
                            this.AppendLog($"Multi Firmware Version:   {fileDetails.Version} ({fileDetails.ModuleType})\r\n");
                            this.AppendLog($"Firmware Target:          {fileDetails.ModuleSubType} ({fileDetails.ModuleMcuFlashSizeKb}KB)\r\n");
                            this.AppendLog($"Expected Channel Order:   {fileDetails.ChannelOrder}\r\n");
                            this.AppendLog($"Telemetry Type:           {fileDetails.MultiTelemetryType}\r\n");
                            this.AppendLog($"Invert Telemetry Enabled: {fileDetails.InvertTelemetry}\r\n");
                            this.AppendLog($"Flash from Radio Enabled: {fileDetails.CheckForBootloader}\r\n");
                            this.AppendLog($"Bootloader Enabled:       {fileDetails.BootloaderSupport}\r\n");
                            this.AppendLog($"USB Serial Support:       {usbSerialSupport}\r\n");
                            this.AppendLog($"Serial Debug Enabled:     {fileDetails.DebugSerial}");
                        }
                        else
                        {
                            this.AppendLog($"Firmware File Name: {this.textFileName.Text.Substring(this.textFileName.Text.LastIndexOf("\\") + 1)}\r\n\r\n");
                            this.AppendLog($"Firmware signature not found in file, extended information is not available. This is normal for firmware prior to v1.2.1.79.\r\n");
                        }

                        byte[] eePromData = Stm32EepromUtils.GetEepromDataFromBackup(this.textFileName.Text);
                        if (eePromData != null)
                        {
                            globalId = Stm32EepromUtils.ReadGlobalId(eePromData);
                        }
                    }
                    else if (openFileDialog.FileName.EndsWith(".eep"))
                    {
                        this.AppendLog($"EEPROM File Name:         {this.textFileName.Text.Substring(this.textFileName.Text.LastIndexOf("\\") + 1)}");

                        byte[] eePromData = AtmegaEepromUtils.GetEepromDataFromBackup(this.textFileName.Text);
                        if (eePromData != null)
                        {
                            globalId = AtmegaEepromUtils.ReadGlobalId(eePromData);
                        }
                    }

                    if (globalId > 0)
                    {
                        this.AppendLog($"\r\nEEPROM Global ID:         0x{globalId:X8}");
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
            this.CheckControlsAsync();
        }

        /// <summary>
        /// Resets the module to DFU Bootloader mode.
        /// </summary>
        private async Task ResetToDfuMode()
        {
            // Prompt for confirmation
            DialogResult confirm;
            using (new CenterWinDialog(this))
            {
                confirm = MessageBox.Show(Strings.dfuResetPrompt, Strings.dialogTitleDfuReset, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            }

            // Abort if the user didn't click OK
            if (confirm != DialogResult.OK)
            {
                return;
            }

            // Disable the buttons until we're done
            Debug.WriteLine("Disabling the controls...");
            this.EnableControls(false);

            // Clear the output box
            Debug.WriteLine("Clearing the output textboxes...");
            this.textActivity.Clear();
            this.textVerbose.Clear();
            this.progressBar1.Value = 0;
            this.outputLineBuffer = string.Empty;

            // Get the name of the Maple COM port
            string mapleComPort = MapleDevice.GetMapleComPort();
            if (mapleComPort != null)
            {
                // Reset the module to DFU mode
                await MapleDevice.ResetToDfuMode(this, mapleComPort);
            }

            // Re-enable the controls
            this.EnableControls(true);
        }

        /// <summary>
        /// Installs the native USB drivers.
        /// </summary>
        private async Task InstallUsbDrivers()
        {
            // Prompt for confirmation
            DialogResult confirm;
            using (new CenterWinDialog(this))
            {
                confirm = MessageBox.Show(Strings.driverInstallerPrompt, Strings.dialogTitleDriverInstaller, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            }

            // Abort if the user didn't click OK
            if (confirm != DialogResult.OK)
            {
                return;
            }

            // Clear the output box
            Debug.WriteLine("Clearing the output textboxes...");
            this.textActivity.Clear();
            this.textVerbose.Clear();
            this.progressBar1.Value = 0;
            this.outputLineBuffer = string.Empty;

            // Create a temp directory to extract the drivers into
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);

            int returnCode = -1;

            // Run the USB Serial Driver installer
            this.AppendLog(Strings.progressInstallingUsbSerialDriver);
            await Task.Run(() => { returnCode = this.RunDriverInstallerSync(".\\drivers\\wdi-simple.exe", $"--vid 0x1EAF --pid 0x0004 --type 3 --name \"MULTI-Module USB Serial\" --dest \"{tempDirectory}\\MULTI-USB-Serial\" --progressbar={this.windowHandle}"); });
            if (returnCode == 0)
            {
                this.AppendLog($" {Strings.done}\r\n");
            }
            else
            {
                this.AppendLog($" {Strings.failed}\r\n");
            }

            // If there's a Maple device connected try to reset it to DFU mode
            string mapleComPort = MapleDevice.GetMapleComPort();
            if (mapleComPort != null)
            {
                await MapleDevice.ResetToDfuMode(this, mapleComPort);
            }

            // Run the DFU Bootloader Driver installer
            this.AppendLog(Strings.progressInstallingDfuDriver);
            await Task.Run(() => { returnCode = this.RunDriverInstallerSync(".\\drivers\\wdi-simple.exe", $"--vid 0x1EAF --pid 0x0003 --type 2 --name \"MULTI-Module DFU Bootloader\" --dest \"{tempDirectory}\\MULTI-DFU-Bootloader\" --progressbar={this.windowHandle}"); });
            if (returnCode == 0)
            {
                this.AppendLog($" {Strings.done}\r\n");
            }
            else
            {
                this.AppendLog($" {Strings.failed}\r\n");
            }
        }

        /// <summary>
        /// Upgrade the MULTI-Module booloader.
        /// </summary>
        private async Task UpgradeBootloader()
        {
            // Prompt for confirmation
            DialogResult confirm;
            using (new CenterWinDialog(this))
            {
                confirm = MessageBox.Show(Strings.bootloaderUpgradePrompt, Strings.dialogTitleBootloaderUpgrade, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            }

            // Abort if the user didn't click OK
            if (confirm != DialogResult.OK)
            {
                return;
            }

            // Disable the buttons until this flash attempt is complete
            Debug.WriteLine("Disabling the controls...");
            this.EnableControls(false);

            // Clear the output box
            Debug.WriteLine("Clearing the output textboxes...");
            this.textActivity.Clear();
            this.textVerbose.Clear();
            this.progressBar1.Value = 0;
            this.outputLineBuffer = string.Empty;

            // Discard the last backup
            this.BackupModuleType = NoBackup;
            this.firmwareBackupFileName = string.Empty;
            this.eepromBackupFileName = string.Empty;

            // Get the port
            string comPort = this.GetSelectedPort().ToString();

            bool upgradeSucceeded;

            // Write the bootloader upgrader
            upgradeSucceeded = await MapleDevice.UpgradeBootloader(this, comPort);

            if (upgradeSucceeded)
            {
                using (new CenterWinDialog(this))
                {
                    MessageBox.Show(Strings.bootloaderUpgradeDone, Strings.dialogTitleBootloaderUpgrade, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                using (new CenterWinDialog(this))
                {
                    MessageBox.Show(Strings.bootloaderUpgradeFailed, Strings.dialogTitleBootloaderUpgrade, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        /// <summary>
        /// Synchronously run the driver installer commands. Prompts the user to run with elevated rights.
        /// </summary>
        private int RunDriverInstallerSync(string command, string args)
        {
            // Output the command
            this.AppendVerbose($"{command} {args}");

            Process myProcess = new Process();
            myProcess.StartInfo.FileName = command;
            myProcess.StartInfo.Arguments = args;
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.CreateNoWindow = true;
            myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            myProcess.StartInfo.RedirectStandardOutput = false;
            myProcess.StartInfo.RedirectStandardError = false;
            myProcess.StartInfo.UseShellExecute = true;
            myProcess.StartInfo.Verb = "runas";

            try
            {
                myProcess.Start();
            }
            catch (Exception e)
            {
                this.AppendVerbose(e.Message);
                return -1;
            }

            // Wait for the installer to finish
            myProcess.WaitForExit();

            // Capture the exit code
            int returnCode = myProcess.ExitCode;
            myProcess.Dispose();

            // Return the exit code from the process
            return returnCode;
        }

        private async Task RunMultiFirmware()
        {
            // Disable the buttons until this flash attempt is complete
            Debug.WriteLine("Disabling the controls...");
            this.EnableControls(false);

            // Clear the output box
            Debug.WriteLine("Clearing the output textboxes...");
            this.textActivity.Clear();
            this.textVerbose.Clear();
            this.progressBar1.Value = 0;
            this.outputLineBuffer = string.Empty;

            /*
            // Determine if we should use Maple device
            MapleDevice mapleResult = MapleDevice.FindMaple();

            // Determine if we should use a USBasp device
            UsbAspDevice usbaspResult = UsbAspDevice.FindUsbAsp();
            */

            // Get the selected COM port
            string comPort = this.GetSelectedPort().ToString();

            await SerialDevice.RunFirmware(this, comPort);

            this.EnableControls(true);
        }

        /// <summary>
        /// Async method to poulate the COM ports without blocking the UI.
        /// </summary>
        /// <returns>A task.</returns>
        private async Task PopulateComPortsAsync()
        {
            await Task.Run(() => { this.PopulateComPorts(); });
        }

        /// <summary>
        /// Populates the list of COM ports.
        /// </summary>
        private void PopulateComPorts()
        {
            Debug.WriteLine("Populate COM ports started");
            DateTime start = DateTime.Now;

            // Don't refresh if the control is not enabled
            if (!this.comPortSelector.Enabled)
            {
                return;
            }

            // Stop firing the the COM port selector change handler
            this.comPortSelector.SelectedIndexChanged -= new System.EventHandler(this.ComPortSelector_SelectionChanged);

            // Get the current list from the combobox so we can auto-select the new device
            var oldPortList = this.comPortSelector.Items;

            // Cache the selected item so we can try to re-select it later
            object selectedValue = this.GetSelectedPort();
            object portToSelect = selectedValue;

            // Enumerate the COM ports and bind the COM port selector
            Debug.WriteLine($"{DateTime.Now - start} Enumerating COM ports.");
            _ = new List<ComPort>();
            List<ComPort> comPorts = ComPort.EnumeratePortList();

            // Check if we have a Maple device
            Debug.WriteLine($"{DateTime.Now - start} Checking for a Maple device.");
            _ = MapleDevice.FindMaple();

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
                        portToSelect = newPort.Name;
                    }
                }
            }

            // Populate the COM port selector
            Debug.WriteLine($"{DateTime.Now - start} Populating port selector.");
            this.PopulatePortSelector(comPorts);

            // Re-select the previously selected item
            this.SelectPort(portToSelect);

            // Set the width of the dropdown
            // this.comPortSelector.DropDownWidth = comPorts.Select(c => c.DisplayName).ToList().Max(x => TextRenderer.MeasureText(x, this.comPortSelector.Font).Width);

            // Make sure the Update button is disabled if there is no port selected
            this.CheckControlsAsync();

            // Resume firing the the COM port selector change handler
            this.comPortSelector.SelectedIndexChanged += new System.EventHandler(this.ComPortSelector_SelectionChanged);
        }

        /// <summary>
        /// Selects a specific COM port.
        /// </summary>
        /// <param name="selectedPort">The port to select.</param>
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

        /// <summary>
        /// Gets the currently selected COM port.
        /// </summary>
        /// <returns>The name of the selected port.</returns>
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

        /// <summary>
        /// Populates the COM port selector with a list of ports.
        /// </summary>
        /// <param name="comPorts">The list of ports.</param>
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
    }
}
