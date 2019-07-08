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
using System.Net;

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

            // Register a hendler to check for a new version when the form is shown the first time
            Shown += FlashMulti_Shown;
        }

        /// <summary>
        /// Called when the form has finished loading for the first time.
        /// Checks Github for a newer version.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FlashMulti_Shown(object sender, EventArgs e)
        {
            // Check for a new version
            UpdateCheck();
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
            if (textFileName.Text != "" && comPortSelector.SelectedItem != null && (writeBootloader_No.Checked || writeBootloader_Yes.Checked))
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

            // Check if we there's a Maple device plugged in
            MapleTools.FindMapleResult mapleCheck = MapleTools.FindMaple();

            if (mapleCheck.MapleFound)
            {
                // Set the Write Bootloader radio button and disable the controls if a Maple device is present
                // Required so that the firmware size is calculated correctly
                writeBootloader_Yes.Checked = true;
                writeBootloader_Yes.Enabled = false;
                writeBootloader_No.Enabled = false;

                // If the Maple device is in DFU mode add a DFU device to the list
                // Required in case there are no other serial devices present as the user need to select something from the list
                if (mapleCheck.Device.DfuMode)
                {
                    comPortSelector.Items.Add("DFU Device");
                }
            } else
            {
                writeBootloader_Yes.Enabled = true;
                writeBootloader_No.Enabled = true;
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
                MessageBox.Show("Firmware file does not exist.", "Write Firmware", MessageBoxButtons.OK, MessageBoxIcon.Error);
                EnableControls(true);
                return;
            }

            // Check that the file size is OK
            // Max size is 120,832B (118KB) with bootloader, 129,024B (126KB) without
            int maxFileSize = 129024;
            if (writeBootloader_Yes.Checked)
            {
                maxFileSize = 120832;
            }
            
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
                Debug.WriteLine("Disabling the controls...");
            }

            // Toggle the controls
            buttonUpload.Enabled = arg;
            buttonBrowse.Enabled = arg;
            buttonRefresh.Enabled = arg;
            textFileName.Enabled = arg;
            comPortSelector.Enabled = arg;
            writeBootloader_Yes.Enabled = arg;
            writeBootloader_No.Enabled = arg;

            // Check a couple of things if we're re-enabling
            if (arg)
            {
                // Keep the Write Bootloader controls disabled if a Maple device is plugged in.
                if (MapleTools.FindMaple().MapleFound)
                {
                    writeBootloader_Yes.Checked = true;
                    writeBootloader_Yes.Enabled = false;
                    writeBootloader_No.Enabled = false;
                }

                // Check if the Upload button can be enabled
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
            int flashStep = 1;
            int flashSteps = 2;

            int flashStart = 0;
            string executionAddress = "0x8000000";

            if (writeBootloader_Yes.Checked)
            {
                flashSteps = 3;
            }

            AppendLog("Starting Multimodule update\r\n");

            // Erase
            AppendLog($"[{flashStep}/{flashSteps}] Erasing flash memory...");
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

            if (writeBootloader_Yes.Checked)
            {
                // Flash bootloader
                flashStep ++;
                AppendLog($"[{flashStep}/{flashSteps}] Writing bootloader...");
                commandArgs = $"-v -e 0 -g {executionAddress} -b 115200 -w \"{bootLoaderPath}\" {comPort}";
                await Task.Run(() => { returnCode = RunCommand(command, commandArgs); });
                if (returnCode != 0)
                {
                    EnableControls(true);
                    AppendLog(" failed!");
                    MessageBox.Show("Failed to write the bootloader.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                AppendLog(" done\r\n");

                // Set the flash address for a flash with bootloader
                flashStart = 8;
                executionAddress = "0x8002000";
            }

            // Flash firmware
            flashStep++;
            AppendLog($"[{flashStep}/{flashSteps}] Writing Multimodule firmware...");
            commandArgs = $"-v -s {flashStart} -e 0 -g {executionAddress} -b 115200 -w \"{fileName}\" {comPort}";
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

            // Check the file name and pre-set the Write Bootloader option
            if (textFileName.Text.IndexOf("_FTDI_") > -1)
            {
                writeBootloader_No.Checked = true;
            }
            else if (textFileName.Text.IndexOf("_TXFLASH_") > -1)
            {
                writeBootloader_Yes.Checked = true;
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

        private void WriteBootloader_OnChange(object sender, EventArgs e)
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

        /// <summary>
        /// Opens a URL in the default browser.
        /// </summary>
        private void OpenLink(string url)
        {
            System.Diagnostics.Process.Start(url);
        }

        /// <summary>
        /// Compares the latest Github version to the current one and prompts the user to update if necessary.
        /// </summary>
        private void UpdateCheck()
        {
            // Get check for the latest version on Github
            UpdateCheckResult check = GetLatestVersion();

            // If the check completed successfully
            if (check.CheckSuccess)
            {
                // Get the current version
                Version currentVersion = Version.Parse(Application.ProductVersion);

                // Get the version available on Github
                Version latestVersion = check.LatestVersion;

                // Check if the current version is older than the latest Github release version
                if (currentVersion.CompareTo(latestVersion) == -1)
                {
                    // A newer version is available to show the user a prompt
                    Debug.WriteLine($"App version is older than latest version: {currentVersion} < {latestVersion}");
                    DialogResult showUpdate = MessageBox.Show(
                        $"A newer version of Flash Multi is available.\n\nYou have v{currentVersion.ToString()} and v{latestVersion.ToString()} is available.\n\nSee the latest release on Github?", 
                        "Flash Multi Update Available", 
                        MessageBoxButtons.YesNo, 
                        MessageBoxIcon.Information
                        );

                    // Show the Github release page for the new version if the user clicked Yes
                    if (showUpdate == DialogResult.Yes)
                    {
                        try
                        {
                            OpenLink(check.ReleaseUrl);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks Github for the latest release.
        /// <returns>Returns a boolean indicating if the check succeeds, the latest version number, and the latest release URL.</returns>
        /// </summary>
        private UpdateCheckResult GetLatestVersion()
        {
            // Somewhere to store the results
            UpdateCheckResult results = new UpdateCheckResult
            {
                CheckSuccess = false
            };

            try
            {
                // The API URL to check for the latest release
                // Returns a JSON payload containing all the details of the latest release
                string releasesUrl = "https://api.github.com/repos/benlye/flash-multi/releases/latest";

                // Set TLS1.2, as required by the Github API
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                
                // Create the WebRequest
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(releasesUrl);

                // Set the UserAgent, as required by the Github API
                webRequest.UserAgent = $"flash-multi-{Application.ProductVersion}";

                // Disable keepalive so we don't hold a connection open
                webRequest.KeepAlive = false;

                // Get the response and read it
                using (WebResponse myResponse = webRequest.GetResponse())
                using (StreamReader sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8))
                {
                    // Read all of the output from the StreamReader
                    string result = sr.ReadToEnd();

                    // Parse the release tag out of the JSON
                    // This contains the version string
                    int tagStart = result.IndexOf("\"tag_name\":");
                    int tagEnd = result.IndexOf(",", tagStart);
                    string tagLine = result.Substring(tagStart, tagEnd - tagStart);
                    string tag = tagLine.Split('"')[3];

                    // Add the release tag to the results
                    results.LatestVersion = Version.Parse(tag);

                    // Parse the release URL out of the JSON
                    // This is the URL of the Github page containing details of the latest release
                    int urlStart = result.IndexOf("\"html_url\":");
                    int urlEnd = result.IndexOf(",", urlStart);
                    string urlLine = result.Substring(urlStart, urlEnd - urlStart);
                    string url = urlLine.Split('"')[3];

                    // Add the release URL to the results
                    results.ReleaseUrl = url;
                }

                // Define a regular expression to test the version number looks how we expect it to
                Regex versionRegex = new Regex(@"\d+\.\d+\.\d+");

                // Check that the URL and version number are as we expect
                if (results.ReleaseUrl.StartsWith("https://github.com/benlye/flash-multi/releases/") && versionRegex.Match(results.LatestVersion.ToString()).Success)
                {
                    // All looks good; the check succeeded
                    results.CheckSuccess = true;
                } 
                
            }
            catch ( Exception ex)
            {
                Debug.WriteLine($"Error getting latest version: {ex.Message}");
            }

            // Return the results
            return results;
        }
    }

    /// <summary>
    /// Class to store the results of a Github version check.
    /// </summary>
    public class UpdateCheckResult
    {
        /// <summary>
        /// Gets or sets a boolean indicating whether or not the Github version check succeeded.
        /// </summary>
        public bool CheckSuccess { get; set; }
        /// <summary>
        /// Gets or sets a Version containing the version of the latest release.
        /// </summary>
        public Version LatestVersion { get; set; }
        /// <summary>
        /// Gets or sets a string containing the URL of the latest release
        /// </summary>
        public string ReleaseUrl { get; set; }
    }
}
