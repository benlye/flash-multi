using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Windows.Forms;
using System.Threading;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace flash_multi
{
    public partial class FlashMulti : Form
    {
        public delegate void InvokeDelegate();

        /// <summary>
        /// The main FlashMulti method.
        /// </summary>
        public FlashMulti()
        {
            InitializeComponent();

            // Include the version in the window title
            this.Text = String.Format("Flash Multi v{0}", Application.ProductVersion);

            // Set focus away from the textbox
            this.ActiveControl = linkLabel2;

            // Populate the list of serial ports
            PopulateComPorts();

            // Disable the Upload button until we're ready
            buttonUpload.Enabled = false;
        }

        /// <summary>
        /// Handle USB device arrival/removal notifications.
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == UsbNotification.WmDevicechange)
            {
                switch ((int)m.WParam)
                {
                    case UsbNotification.DbtDeviceremovecomplete:
                        BeginInvoke(new InvokeDelegate(PopulateComPorts));
                        break;
                    case UsbNotification.DbtDevicearrival:
                        BeginInvoke(new InvokeDelegate(PopulateComPorts));
                        break;
                }
            }
        }

        /// <summary>
        /// Checks if the Upload button should be enabled or not.
        /// Called by changes to the file name or COM port selector
        /// </summary>
        private void CheckControls()
        {
            if (textFileName.Text != "" && comPortSelector.SelectedItem != null)
            {
                buttonUpload.Enabled = true;
            }
            else
            {
                buttonUpload.Enabled = false;
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

            // Try to open the serial port, catch an exception if fail
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
            selectedItem = comPortSelector.SelectedItem;

            // Get the list of COM port names
            string[] comPorts = SerialPort.GetPortNames();
            
            // Sort the list of ports
            var orderedComPorts = comPorts.OrderBy(c => c.Length).ThenBy(c => c).ToList();

            // Clear the existing list
            comPortSelector.Items.Clear();

            // Add the ports one by one
            foreach (string port in orderedComPorts)
            {
                comPortSelector.Items.Add(port);
            }
                        
            // Short pause to give a DFU device time to show up
            Thread.Sleep(50);

            // Check if we should add a DFU device to the list (purely cosmetic, not really required)
            MapleTools.FindMapleResult mapleCheck = MapleTools.FindMaple();
            if (mapleCheck.Device.DfuMode)
            {
                comPortSelector.Items.Add("DFU Device");
            }

            // Re-select the previously selected item
            comPortSelector.SelectedItem = selectedItem;

            // Make sure the Update button is disabled if there is no port selected
            CheckControls();
        }

        /// <summary>
        /// Main method where all the action happens.
        /// Called by the Upload button.
        /// </summary>
        private void ButtonUpload_Click(object sender, EventArgs e)
        {
            // Clear the output box
            Debug.WriteLine("Clearing the output textboxes...");
            textActivity.Clear();
            textVerbose.Clear();
            progressBar1.Value = 0;

            // Check if the file exists
            if (! File.Exists(textFileName.Text))
            {
                AppendLog(String.Format("File {0} does not exist", textFileName.Text));
                MessageBox.Show("Fimrware file does not exist.", "Write Firmware", MessageBoxButtons.OK, MessageBoxIcon.Error);
                EnableControls(true);
                return;
            }

            // Check that the file size is OK - max size is 120,832B (118KB) to allow for bootloader and EEPROM emulation
            int maxFileSize = 120832;
            long length = new System.IO.FileInfo(textFileName.Text).Length;

            if (length > maxFileSize)
            {
                AppendLog(String.Format("Firmware file is too large.\r\nFile is {1:n0} KB, maximum size is {2:n0} KB.", textFileName.Text, length/1024, maxFileSize/1024));
                MessageBox.Show("Firmware file is too large.", "Write Firmware", MessageBoxButtons.OK, MessageBoxIcon.Error);
                EnableControls(true);
                return;
            }

            // Get the selected COM port
            string comPort = comPortSelector.SelectedItem.ToString();

            // Check if the port can be opened
            if (!PortCheck(comPort))
            {
                AppendLog(String.Format("Couldn't open port {0}", comPort));
                MessageBox.Show(String.Format("Couldn't open port {0}", comPort), "Write Firmware", MessageBoxButtons.OK, MessageBoxIcon.Error);
                EnableControls(true);
                return;
            }

            // Disable the buttons until this flash attempt is complete
            Debug.WriteLine("Disabling the controls...");
            EnableControls(false);

            // Determine if we should use Maple or serial interface
            MapleTools.FindMapleResult mapleResult = MapleTools.FindMaple();

            if (mapleResult.MapleFound == true)
            {
                AppendLog(String.Format("Maple device found in {0} mode\r\n", mapleResult.Device.Mode));
            }

            // Do the selected flash using the appropriate method
            if (mapleResult.MapleFound == true)
            {
                MapleFlashWrite(textFileName.Text, comPort);
            }
            else
            {
                SerialFlashWrite(textFileName.Text, comPort);
            }
        }

        /// <summary>
        /// Enable or disable the controls.
        /// </summary>
        private void EnableControls(bool arg)
        {
            // Enable the buttons
            if (arg)
            {
                Debug.WriteLine("Re-enabling the controls...");
            }
            else
            {
                Debug.WriteLine("Disnabling the controls...");
            }

            // Toggle the controls
            buttonUpload.Enabled = arg;
            buttonBrowse.Enabled = arg;
            buttonRefresh.Enabled = arg;
            textFileName.Enabled = arg;
            comPortSelector.Enabled = arg;

            // If we're re-enabling, check if the Upload button should be enabled or disabled
            if (arg)
            {
                CheckControls();
            }
        }

        /// <summary>
        /// Writes the firmware to a Maple serial or DFU device
        /// </summary>
        private async void MapleFlashWrite(string fileName, string comPort)
        {
            string command;
            string commandArgs;
            int returnCode = -1;

            AppendLog("Starting Multimodule update\r\n");

            string mapleMode = MapleTools.FindMaple().Device.Mode;

            if (mapleMode == "USB")
            {
                AppendLog("Switching Multimodule into DFU mode ...");
                command = ".\\tools\\maple-reset.exe";
                commandArgs = comPort;
                await Task.Run(() => { returnCode = RunCommand(command, commandArgs); });
                if (returnCode != 0)
                {
                    EnableControls(true);
                    AppendLog(" failed!");
                    MessageBox.Show("Failed to get module to DFU mode.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                AppendLog(" done\r\n");

                // Check for a Maple DFU device
                AppendLog("Waiting for DFU device ...");
                bool dfuCheck = false;
                int counter = 0;

                dfuCheck = MapleTools.FindMaple().Device.DfuMode;
                
                while (dfuCheck == false && counter < 20)
                {
                    Thread.Sleep(50);
                    dfuCheck = MapleTools.FindMaple().Device.DfuMode;
                    counter++;
                }

                if (dfuCheck)
                {
                    AppendLog(" got it\r\n");
                }
                else
                {
                    AppendLog(" failed!");
                    MessageBox.Show("Failed to find module in DFU mode.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    EnableControls(true);
                    return;
                }
            }

            // Flash firmware
            AppendLog("Writing firmware to Multimodule ...");
            command = ".\\tools\\dfu-util.exe";
            commandArgs = String.Format("-R -a 2 -d 1EAF:0003 -D \"{0}\"", fileName, comPort);
            await Task.Run(() => { returnCode = RunCommand(command, commandArgs); });
            if (returnCode != 0)
            {
                EnableControls(true);
                AppendLog(" failed!");
                MessageBox.Show("Failed to write the firmware.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            AppendLog(" done\r\n");

            AppendLog("\r\nMultimodule updated sucessfully");

            MessageBox.Show("Multimodule updated sucessfully.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
            EnableControls(true);
        }

        /// <summary>
        /// Writes the firmware to a serial (FTDI) device.
        /// </summary>
        private async void SerialFlashWrite(string fileName, string comPort)
        {
            string command = ".\\tools\\stm32flash.exe";
            string bootLoaderPath = ".\\bootloaders\\StmMulti4in1.bin";
            string commandArgs;

            int returnCode = -1;

            AppendLog("Starting Multimodule update\r\n");

            // Erase
            AppendLog("[1/3] Erasing flash memory...");
            commandArgs = String.Format("-o -S 0x8000000:129024 -b 115200 {0}", comPort);
            await Task.Run(() => { returnCode = RunCommand(command, commandArgs); });
            if (returnCode != 0)
            {
                EnableControls(true);
                AppendLog(" failed!");
                MessageBox.Show("Failed to erase flash memory.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            AppendLog(" done\r\n");

            // Flash bootloader
            AppendLog("[2/3] Writing bootloader...");
            commandArgs = String.Format("-v -e 0 -g 0x8000000 -b 115200 -w \"{0}\" {1}", bootLoaderPath, comPort);
            await Task.Run(() => { returnCode = RunCommand(command, commandArgs); });
            if (returnCode != 0)
            {
                EnableControls(true);
                AppendLog(" failed!");
                MessageBox.Show("Failed to write the bootloader.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            AppendLog(" done\r\n");

            // Flash firmware
            AppendLog("[3/3] Writing Multimodule firmware...");
            commandArgs = String.Format("-v -s 8 -e 0 -g 0x8002000 -b 115200 -w \"{0}\" {1}", fileName, comPort);
            await Task.Run(() => { returnCode = RunCommand(command, commandArgs); });
            if (returnCode != 0)
            {
                EnableControls(true);
                AppendLog(" failed!");
                MessageBox.Show("Failed to write the firmware.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            AppendLog(" done\r\n");

            AppendLog("\r\nMultimodule updated sucessfully");

            MessageBox.Show("Multimodule updated sucessfully.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
            EnableControls(true);

        }

        /// <summary>
        /// Runs a command with arguments
        /// </summary>
        private int RunCommand(string command, string args)
        {
            Debug.WriteLine("\n" + command + " " + args + "\n");
            AppendVerbose(command + " " + args + "\r\n");

            // Check if the file exists
            if (!File.Exists(command))
            {
                AppendVerbose(String.Format("{0} does not exist", command));
                return -1;
            }

            Process myProcess = new Process();

            // Process process = new Process();
            myProcess.StartInfo.FileName = command;
            myProcess.StartInfo.Arguments = args;
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.RedirectStandardOutput = true;
            myProcess.StartInfo.RedirectStandardError = true;
            myProcess.StartInfo.CreateNoWindow = true;

            //* Set your output and error (asynchronous) handlers
            myProcess.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            myProcess.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);

            //* Start process and handlers
            myProcess.Start();

            //* Read the output
            myProcess.BeginOutputReadLine();
            myProcess.BeginErrorReadLine();

            // Loop until the process finishes
            myProcess.WaitForExit();

            //* Return the exit code from the process
            return myProcess.ExitCode;
        }

        /// <summary>
        /// Handles the standard and error output from a running command.
        /// Updates the verbose output text box.
        /// </summary>
        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            //* Do your stuff with the output
            Debug.WriteLine(outLine.Data);
            /*
            // Update the progress bar if there is a percentage in the output
            Regex regex = new Regex(@"\((\d+)\.\d\d\%\)");
            if (outLine.Data != null)
            {
                Match match = regex.Match(outLine.Data);
                if (match.Success)
                {
                    UpdateProgress(int.Parse(match.Groups[1].Value));
                }
            }
            */

            AppendVerbose(outLine.Data);
        }

        /// <summary>
        /// Selects a firmware file to flash
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
                    textFileName.Text = openFileDialog.FileName;
                }
            }

            // Check if the Upload button should be enabled yet
            CheckControls();
        }

        /// <summary>
        /// Handles a change in the COM port selection dropdown.
        /// </summary>
        private void ComPortSelector_SelectionChanged(object sender, EventArgs e)
        {
            // Check if the Upload button should be enabled yet
            CheckControls();
        }

        /// <summary>
        /// Handles input in the firmware file name text box.
        /// </summary>
        private void TextFileName_OnChange(object sender, EventArgs e)
        {
            // Check if the Upload button should be enabled yet
            CheckControls();
        }

        /// <summary>
        /// Appends a string to the verbose output text box.
        /// </summary>
        private void AppendVerbose(string text)
        {
            // Check if we're called from another thread
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendVerbose), new object[] { text });
                return;
            }

            // Append the text
            textVerbose.AppendText(text + "\r\n");
        }

        /// <summary>
        /// Appends a string to the log output text box.
        /// </summary>
        private void AppendLog(string text)
        {
            // Check if we're called from another thread
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendLog), new object[] { text });
                return;
            }

            // Append the text
            textActivity.AppendText(text);
        }

        /// <summary>
        /// Updates the progress bar.
        /// </summary>
        private void UpdateProgress(int value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<int>(UpdateProgress), new object[] { value });
                return;
            }
            progressBar1.Value = value;
        }

        /// <summary>
        /// Handles the show verbose output text box being checked or unchecked.
        /// Shows or hides the verbose output text box.
        /// </summary>
        private void ShowVerboseOutput_OnChange(object sender, EventArgs e)
        {
            if (showVerboseOutput.Checked == true)
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
            PopulateComPorts();
        }

        /// <summary>
        /// Handles the Github repo link being clicked.
        /// </summary>
        private void RepoLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                VisitGitRepoLink();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Opens the Github repo link in the default browser.
        /// </summary>
        private void VisitGitRepoLink()
        {
            System.Diagnostics.Process.Start("https://github.com/benlye/flash-multi");
        }

        /// <summary>
        /// Handles the Multi firmware repo releases link being clicked.
        /// </summary>
        private void ReleasesLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                VisitReleaseLink();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Opens the Multi firmware repo releases link in the default browser.
        /// </summary>
        private void VisitReleaseLink()
        {
            System.Diagnostics.Process.Start("https://github.com/pascallanger/DIY-Multiprotocol-TX-Module/releases");
        }
    }
}
