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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO.Ports;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public partial class SerialMonitor : Form
    {
        public SerialPort serialPort;

        public bool serialConnected;

        private string serialPortName = string.Empty;

        public SerialMonitor(string serialPortName)
        {
            this.InitializeComponent();

            this.serialPortName = serialPortName;
            this.SerialConnect(this.serialPortName);
        }

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

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            // Disconnect the serial port
            this.SerialDisconnect();

            // Close the serial monitor windows
            this.Close();
        }

        private void ButtonClear_Click(object sender, EventArgs e)
        {
            // Clear the serial ouput
            this.serialOutput.Clear();
        }

        private void ButtonConnect_Click(object sender, EventArgs e)
        {
            if (!this.serialConnected)
            {
                this.serialConnected = this.SerialConnect(this.serialPortName);
                if (!this.serialConnected)
                {
                    Debug.WriteLine("Failed to connect.");
                    return;
                }

                Debug.WriteLine("Connected");
            }
        }

        private void AppendOutput(string data)
        {
            // Check if we're called from another thread
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(this.AppendOutput), new object[] { data });
                return;
            }

            // Append the text
            this.serialOutput.AppendText(data);
        }

        public bool SerialConnect(string serialPortName)
        {
            try
            {
                SerialPort serialPort = new SerialPort(serialPortName, 115200, Parity.None, 8, StopBits.One);
                serialPort.NewLine = "\r\n";
                serialPort.Handshake = Handshake.XOnXOff;
                serialPort.DtrEnable = true;
                serialPort.RtsEnable = true;
                serialPort.Open();

                serialPort.DataReceived += new SerialDataReceivedEventHandler(this.SerialPortDataReceived);

                this.serialPort = serialPort;
                serialPort.Write("\r\n");

                this.buttonConnect.Enabled = false;
                this.buttonDisconnect.Enabled = true;

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

        public void SerialDisconnect()
        {
            SerialPort serialPort = this.serialPort;
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
            }

            this.serialConnected = false;
            this.serialPort = null;
            this.buttonConnect.Enabled = true;
            this.buttonDisconnect.Enabled = false;

            Debug.WriteLine($"Disconnected from {this.serialPortName}.");
        }

        private void ButtonDisconnect_Click(object sender, EventArgs e)
        {
            this.SerialDisconnect();
        }
    }
}
