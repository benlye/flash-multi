// -------------------------------------------------------------------------------
// <copyright file="MapleDevice.cs" company="Ben Lye">
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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    /// <summary>
    /// Class for working with Maple USB devices.
    /// </summary>
    internal class MapleDevice
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapleDevice"/> class.
        /// </summary>
        /// <param name="deviceFound">Indicates if a device was found.</param>
        /// <param name="deviceId">The device ID.</param>
        /// <param name="dfuMode">Indicates if the device is in DFU mode.</param>
        /// <param name="usbMode">Indicates if the device is in USB mode.</param>
        /// <param name="mode">The device mode.</param>
        public MapleDevice(bool deviceFound = false, string deviceId = null, bool dfuMode = false, bool usbMode = false, string mode = null)
        {
            this.DeviceFound = deviceFound;
            this.DeviceId = deviceId;
            this.DfuMode = dfuMode;
            this.UsbMode = usbMode;
            this.Mode = mode;
        }

        /// <summary>
        /// Gets a value indicating whether a Maple device was found.
        /// </summary>
        public bool DeviceFound { get; private set; }

        /// <summary>
        /// Gets a string contaiing the device ID.
        /// </summary>
        public string DeviceId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the device is in DFU mode.
        /// </summary>
        public bool DfuMode { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the device is in USB mode.
        /// </summary>
        public bool UsbMode { get; private set; }

        /// <summary>
        /// Gets a string indicating the device mode.
        /// </summary>
        public string Mode { get; private set; }

        /// <summary>
        /// Finds a Maple device in either USB or DFU mode.
        /// </summary>
        /// <returns>Returns a <see cref="MapleDevice"/>.</returns>
        public static MapleDevice FindMaple()
        {
            MapleDevice result = new MapleDevice(false);

            var usbDevices = UsbDeviceInfo.GetUSBDevices();

            foreach (var usbDevice in usbDevices)
            {
                switch (usbDevice.PnpDeviceID.Substring(0, 21))
                {
                    case "USB\\VID_1EAF&PID_0003":
                        result = new MapleDevice(true, "USB\\VID_1EAF&PID_0003", true, false, "DFU");
                        break;
                    case "USB\\VID_1EAF&PID_0004":
                        result = new MapleDevice(true, "USB\\VID_1EAF&PID_0004", false, true, "USB");
                        break;
                    default:
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Waits for a Maple DFU device to be added or removed.
        /// </summary>
        /// <param name="timeout">Timeout in milliseconds.</param>
        /// <param name="removed">Set to true to wait for DFU device to be removed.</param>
        /// <returns>Boolean indicating whether or not a DFU device appeared or disappeared within the timeout.</returns>
        public static bool WaitForDFU(int timeout = 500, bool removed = false)
        {
            DateTime start = DateTime.Now;
            bool dfuCheck = MapleDevice.FindMaple().DfuMode;

            double elapsedMs = (DateTime.Now - start).TotalMilliseconds;

            while (dfuCheck == removed && elapsedMs < timeout)
            {
                Thread.Sleep(50);
                dfuCheck = MapleDevice.FindMaple().DfuMode;
                elapsedMs = (DateTime.Now - start).TotalMilliseconds;
            }

            Debug.WriteLine($"Elapsed time: {elapsedMs}");

            if (elapsedMs < timeout)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Writes the firmware to a Maple serial or DFU device.
        /// </summary>
        /// <param name="flashMulti">An instance of the <see cref="FlashMulti"/> class.</param>
        /// <param name="fileName">The path of the file to flash.</param>
        /// <param name="comPort">The COM port where the Maple USB device can be found.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task WriteFlash(FlashMulti flashMulti, string fileName, string comPort)
        {
            string command;
            string commandArgs;
            int returnCode = -1;

            flashMulti.AppendLog("Starting Multimodule update via native USB\r\n");

            // Stop the serial monitor if it's active
            SerialMonitor serialMonitor = null;
            bool reconnectSerialMonitor = false;
            if (Application.OpenForms.OfType<SerialMonitor>().Any())
            {
                Debug.WriteLine("Serial monitor window is open");
                serialMonitor = Application.OpenForms.OfType<SerialMonitor>().First();
                if (serialMonitor != null && serialMonitor.SerialPort != null && serialMonitor.SerialPort.IsOpen)
                {
                    reconnectSerialMonitor = true;
                    Debug.WriteLine($"Serial monitor is connected to {serialMonitor.SerialPort.PortName}");

                    Debug.WriteLine($"Closing serial monitor connection to {serialMonitor.SerialPort.PortName}");
                    serialMonitor.SerialDisconnect();
                }
            }
            else
            {
                Debug.WriteLine("Serial monitor is not open");
            }

            // Check if the port can be opened
            if (!ComPort.CheckPort(comPort))
            {
                flashMulti.AppendLog(string.Format("Couldn't open port {0}", comPort));
                MessageBox.Show(string.Format("Couldn't open port {0}", comPort), "Write Firmware", MessageBoxButtons.OK, MessageBoxIcon.Error);
                flashMulti.EnableControls(true);
                return;
            }

            string mapleMode = MapleDevice.FindMaple().Mode;

            if (mapleMode == "USB")
            {
                flashMulti.AppendLog("Switching Multimodule into DFU mode ...");
                command = ".\\tools\\maple-reset.exe";
                commandArgs = $"{comPort} 2000";
                await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });
                if (returnCode == 0)
                {
                    flashMulti.AppendLog(" done\r\n");
                    flashMulti.AppendVerbose(string.Empty);
                }
                else
                {
                    if (MapleDevice.FindMaple().DfuMode == false)
                    {
                        flashMulti.AppendLog(" failed!\r\n");
                        flashMulti.AppendLog("Attempting DFU Recovery Mode.\r\n");

                        // Show the recovery mode dialog
                        DfuRecoveryDialog recoveryDialog = new DfuRecoveryDialog(flashMulti);
                        var recoveryResult = recoveryDialog.ShowDialog();

                        // Stop if we didn't make it into recovery mode
                        if (recoveryResult == DialogResult.Cancel)
                        {
                            flashMulti.AppendLog("DFU Recovery cancelled.");
                            flashMulti.EnableControls(true);
                            return;
                        }
                        else if (recoveryResult == DialogResult.Abort)
                        {
                            flashMulti.AppendLog("DFU Recovery failed.");
                            flashMulti.EnableControls(true);
                            return;
                        }
                    }
                    else
                    {
                        flashMulti.AppendLog(" done\r\n");
                        flashMulti.AppendVerbose(string.Empty);
                    }
                }
            }

            // First attempt to flash the firmware
            flashMulti.AppendLog("Writing firmware to Multimodule ...");
            command = ".\\tools\\dfu-util.exe";
            commandArgs = string.Format("-R -a 2 -d 1EAF:0003 -D \"{0}\"", fileName, comPort);

            await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });

            if (returnCode != 0)
            {
                // First attempt failed so we need to try bootloader recovery
                flashMulti.AppendLog(" failed!\r\n");

                flashMulti.AppendLog("Attempting DFU Recovery Mode.\r\n");

                // Show the recovery mode dialog
                DfuRecoveryDialog recoveryDialog = new DfuRecoveryDialog(flashMulti);
                var recoveryResult = recoveryDialog.ShowDialog();

                // If we made it into recovery mode, flash the module
                if (recoveryResult == DialogResult.OK)
                {
                    // Run the recovery flash command
                    flashMulti.AppendLog("Writing firmware to Multimodule ...");
                    await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });
                    if (returnCode != 0)
                    {
                        flashMulti.AppendLog(" failed!\r\n");
                        MessageBox.Show("Failed to write the firmware.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        flashMulti.EnableControls(true);
                        return;
                    }
                }
                else if (recoveryResult == DialogResult.Cancel)
                {
                    flashMulti.AppendLog("DFU Recovery cancelled.");
                    flashMulti.EnableControls(true);
                    return;
                }
                else
                {
                    flashMulti.AppendLog("DFU Recovery failed.");
                    flashMulti.EnableControls(true);
                    return;
                }
            }

            flashMulti.AppendLog(" done\r\n");
            flashMulti.AppendLog("\r\nMultimodule updated successfully");

            // Reconnect the serial monitor
            if (serialMonitor != null && reconnectSerialMonitor)
            {
                Thread.Sleep(1000);
                serialMonitor.SerialConnect(comPort);
            }

            MessageBox.Show("Multimodule updated successfully.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
            flashMulti.EnableControls(true);
        }
    }
}
