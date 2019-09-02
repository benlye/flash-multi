// -------------------------------------------------------------------------------
// <copyright file="SerialMonitor.cs" company="Ben Lye">
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
    using System.Windows.Forms;

    /// <summary>
    /// The SerialMonitor class.
    /// </summary>
    public partial class SerialMonitor : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerialMonitor"/> class.
        /// </summary>
        /// <param name="serialPortName">The name of the serial port to monitor.</param>
        public SerialMonitor(string serialPortName)
        {
            this.InitializeComponent();

            // Register a handler to run on loading the form
            this.Load += this.SerialMonitor_Load;

            this.Text = $"Flash Multi Serial Monitor - {serialPortName}";
            this.SerialPortName = serialPortName;
            this.SerialConnect(this.SerialPortName);
        }

        /// <summary>
        /// Gets the <see cref="SerialPort"/> object being monitored.
        /// </summary>
        public SerialPort SerialPort { get; private set; }

        /// <summary>
        /// Gets the name of the serial port being monitored.
        /// </summary>
        public string SerialPortName { get; private set; }

        /// <summary>
        /// Connects to the specified serial port and registers a handler for data received.
        /// </summary>
        /// <param name="serialPortName">The name of the serial port to connect to.</param>
        /// <returns>A value indicating whether or not the port was successfully opened.</returns>
        public bool SerialConnect(string serialPortName)
        {
            try
            {
                SerialPort serialPort = new SerialPort(serialPortName, 115200, Parity.None, 8, StopBits.One)
                {
                    Handshake = Handshake.XOnXOff,
                    DtrEnable = true,
                    RtsEnable = true,
                };
                serialPort.Open();

                serialPort.DataReceived += new SerialDataReceivedEventHandler(this.SerialPortDataReceived);

                this.SerialPort = serialPort;

                this.buttonConnect.Enabled = false;
                this.buttonDisconnect.Enabled = true;

                this.Text = $"Flash Multi Serial Monitor (Connected to {serialPortName})";

                Debug.WriteLine($"Connected to {serialPortName}.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to open port:\n{ex.Message}");
                MessageBox.Show(ex.Message, "Serial Monitor Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
        }

        /// <summary>
        /// Closes and disposes the serial port being monitored.
        /// </summary>
        public void SerialDisconnect()
        {
            SerialPort serialPort = this.SerialPort;

            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.DtrEnable = false;
                serialPort.RtsEnable = false;
                serialPort.Close();
                serialPort.Dispose();
            }

            this.SerialPort = null;
            this.buttonConnect.Enabled = true;
            this.buttonDisconnect.Enabled = false;

            this.Text = $"Flash Multi Serial Monitor (Disconnected)";

            Debug.WriteLine($"Disconnected from {this.SerialPortName}.");
        }

        /// <summary>
        /// Override method to handle the Serial Monitor window closing.
        /// </summary>
        /// <param name="e">The event.</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Call the base method
            base.OnFormClosing(e);

            // Save the window position
            Properties.Settings.Default.SerialMonitorWindowLocation = this.Location;
            Properties.Settings.Default.Save();

            // Close the serial port
            this.SerialDisconnect();
        }

        /// <summary>
        /// Event handler for the Serial Monitor window loading.
        /// </summary>
        /// <param name="e">The event.</param>
        private void SerialMonitor_Load(object sender, EventArgs e)
        {
            // Restore the last window location
            var windowLocation = Properties.Settings.Default.SerialMonitorWindowLocation;
            if (windowLocation.X != -1 && windowLocation.Y != -1)
            {
                this.Location = Properties.Settings.Default.SerialMonitorWindowLocation;
            }
        }

        /// <summary>
        /// Handles data being received from the serial port.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Debug.WriteLine($"Data received.");
            try
            {
                SerialPort serialPort = sender as SerialPort;
                string data = serialPort.ReadExisting();
                this.AppendOutput(data);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while reading data from serial port.\n{ex.Message}");
            }
        }

        /// <summary>
        /// Handles the Connect button being clicked.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void ButtonConnect_Click(object sender, EventArgs e)
        {
            if (this.SerialPort == null || !this.SerialPort.IsOpen)
            {
                if (!this.SerialConnect(this.SerialPortName))
                {
                    Debug.WriteLine("Failed to connect.");
                    return;
                }
            }
        }

        /// <summary>
        /// Handles the Disconnect button being clicked.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void ButtonDisconnect_Click(object sender, EventArgs e)
        {
            this.SerialDisconnect();
        }

        /// <summary>
        /// Handles the Close button being clicked.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void ButtonClose_Click(object sender, EventArgs e)
        {
            // Close the serial monitor window
            this.Close();
        }

        /// <summary>
        /// Handles the Clear button being clicked.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void ButtonClear_Click(object sender, EventArgs e)
        {
            // Clear the serial ouput
            this.serialOutput.Clear();
        }

        /// <summary>
        /// Handles the Save button being clicked.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void ButtonSave_Click(object sender, EventArgs e)
        {
            // Create the file open dialog
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                // Title for the dialog
                saveFileDialog.Title = "Save log file";

                // Filter for .bin files
                saveFileDialog.Filter = ".log File|*.log";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Save the log data to the selected file name
                    File.WriteAllText(saveFileDialog.FileName, this.serialOutput.Text);
                }
            }
        }

        /// <summary>
        /// Appends serial output to the text box.
        /// </summary>
        /// <param name="data">Text to append to the text box.</param>
        private void AppendOutput(string data)
        {
            // Check if we're called from another thread
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(this.AppendOutput), new object[] { data });
                return;
            }

            // Use the append method if autoscroll is enabled otherwise just add it
            if (this.checkBoxAutoScroll.Checked)
            {
                this.serialOutput.AppendText(data);
            }
            else
            {
                this.serialOutput.Text += data;
            }
        }
    }
}
