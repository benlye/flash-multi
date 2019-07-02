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

namespace flash_multi
{
    public partial class FlashMulti : Form
    {
        private bool deviceUpdate = false;
        public delegate void InvokeDelegate();

        public FlashMulti()
        {
            InitializeComponent();

            // Include the version in the window title
            this.Text = String.Format("Flash Multi v{0}", Application.ProductVersion);

            // Set focus away from the textbox
            this.ActiveControl = labelAbout;

            // Populate the list of serial ports
            PopulateComPorts();

            // Disable the Go button until we're ready
            buttonGo.Enabled = false;

        }
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

        private void CheckControls()
        {
            if (textFileName.Text != "" && comPortSelector.SelectedItem != null)
            {
                buttonGo.Enabled = true;
            }
            else
            {
                buttonGo.Enabled = false;
            }
        }

        private bool PortCheck(string port)
        {
            if (port == "DFU Device")
            {
                return true;
            }

            bool result = false;

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

        private void PopulateComPorts()
        {
            object selectedItem = null;
            selectedItem = comPortSelector.SelectedItem;

            string[] comPorts = SerialPort.GetPortNames();
            comPortSelector.Items.Clear();

            foreach (string port in comPorts)
            {
                comPortSelector.Items.Add(port);
            }

            Thread.Sleep(50);

            // Check if we should add the DFU device (purely cosmetic, not really required)
            MapleTools.FindMapleResult mapleCheck = MapleTools.FindMaple();
            if (mapleCheck.Device.DfuMode)
            {
                comPortSelector.Items.Add("DFU Device");
                comPortSelector.SelectedValue = "DFU Device";
            }

            comPortSelector.SelectedItem = selectedItem;

            CheckControls();
        }

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
                MessageBox.Show("File does not exist", "Write Firmware", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void EnableControls(bool arg)
        {
            // Enable the buttons
            Debug.WriteLine("Re-enabling the controls...");
            buttonGo.Enabled = arg;
            buttonBrowse.Enabled = arg;
            buttonRefresh.Enabled = arg;
            textFileName.Enabled = arg;
            comPortSelector.Enabled = arg;

            if (arg)
            {
                CheckControls();
            }
        }

        private void ComPortSelectorDroppedDown(object sender, EventArgs e)
        {
            if (deviceUpdate)
            {
                PopulateComPorts();
                deviceUpdate = false;
            }
        }
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
                command = ".\\bin\\maple-reset.exe";
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
            command = ".\\bin\\dfu-util.exe";
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

        private async void SerialFlashWrite(string fileName, string comPort)
        {
            string command = ".\\bin\\stm32flash.exe";
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

            //Process process = new Process();
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

        private void ButtonBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = ".bin File|*.bin";
            openFileDialog.Title = "Choose file flash";
            openFileDialog.ShowDialog();
            textFileName.Text = openFileDialog.FileName;
            CheckControls();

        }

        private void ComPortSelector_SelectionChanged(object sender, EventArgs e)
        {
            CheckControls();
        }

        private void TextFileName_OnChange(object sender, EventArgs e)
        {
            CheckControls();
        }

        private void AppendVerbose(string text)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendVerbose), new object[] { text });
                return;
            }
            textVerbose.AppendText(text + "\r\n");
            
        }

        private void AppendLog(string text)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendLog), new object[] { text });
                return;
            }
            textActivity.AppendText(text);
        }

        private void UpdateProgress(int value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<int>(UpdateProgress), new object[] { value });
                return;
            }
            progressBar1.Value = value;
        }

        private void showVerboseOutput_OnChange(object sender, EventArgs e)
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

        private void ButtonRefresh_Click(object sender, EventArgs e)
        {
            PopulateComPorts();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                VisitLink();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        private void VisitLink()
        {
            System.Diagnostics.Process.Start("https://github.com/benlye/flash-multi");
        }
    }
}
