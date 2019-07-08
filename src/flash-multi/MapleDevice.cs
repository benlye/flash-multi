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
        /// Writes the firmware to a Maple serial or DFU device.
        /// </summary>
        /// <param name="flashMulti">An instance of the <see cref="FlashMulti"/> class.</param>
        /// <param name="fileName">The path of the file to flash.</param>
        /// <param name="comPort">The COM port where the Maple USB device can be found.</param>
        public static async void WriteFlash(FlashMulti flashMulti, string fileName, string comPort)
        {
            string command;
            string commandArgs;
            int returnCode = -1;

            flashMulti.AppendLog("Starting Multimodule update\r\n");

            string mapleMode = MapleDevice.FindMaple().Mode;

            if (mapleMode == "USB")
            {
                flashMulti.AppendLog("Switching Multimodule into DFU mode ...");
                command = ".\\tools\\maple-reset.exe";
                commandArgs = comPort;
                await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });
                if (returnCode != 0)
                {
                    flashMulti.EnableControls(true);
                    flashMulti.AppendLog(" failed!");
                    MessageBox.Show("Failed to get module to DFU mode.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                flashMulti.AppendLog(" done\r\n");

                // Check for a Maple DFU device
                flashMulti.AppendLog("Waiting for DFU device ...");
                bool dfuCheck = false;
                int counter = 0;

                dfuCheck = MapleDevice.FindMaple().DfuMode;

                while (dfuCheck == false && counter < 20)
                {
                    Thread.Sleep(50);
                    dfuCheck = MapleDevice.FindMaple().DfuMode;
                    counter++;
                }

                if (dfuCheck)
                {
                    flashMulti.AppendLog(" got it\r\n");
                }
                else
                {
                    flashMulti.AppendLog(" failed!");
                    MessageBox.Show("Failed to find module in DFU mode.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    flashMulti.EnableControls(true);
                    return;
                }
            }

            // Flash firmware
            flashMulti.AppendLog("Writing firmware to Multimodule ...");
            command = ".\\tools\\dfu-util.exe";
            commandArgs = string.Format("-R -a 2 -d 1EAF:0003 -D \"{0}\"", fileName, comPort);
            await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });
            if (returnCode != 0)
            {
                flashMulti.EnableControls(true);
                flashMulti.AppendLog(" failed!");
                MessageBox.Show("Failed to write the firmware.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            flashMulti.AppendLog(" done\r\n");
            flashMulti.AppendLog("\r\nMultimodule updated sucessfully");

            MessageBox.Show("Multimodule updated sucessfully.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
            flashMulti.EnableControls(true);
        }
    }
}
