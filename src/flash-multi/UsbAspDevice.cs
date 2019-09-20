// -------------------------------------------------------------------------------
// <copyright file="UsbAspDevice.cs" company="Ben Lye">
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
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    class UsbAspDevice
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UsbAspDevice"/> class.
        /// </summary>
        /// <param name="deviceFound">Indicates if a device was found.</param>
        /// <param name="deviceId">The device ID.</param>
        public UsbAspDevice(bool deviceFound = false, string deviceId = null)
        {
            this.DeviceFound = deviceFound;
            this.DeviceId = deviceId;
        }

        /// <summary>
        /// Gets a value indicating whether a UsbAsp device was found.
        /// </summary>
        public bool DeviceFound { get; private set; }

        /// <summary>
        /// Gets a string contaiing the device ID.
        /// </summary>
        public string DeviceId { get; private set; }

        /// <summary>
        /// Finds a UsbAsp device.
        /// </summary>
        /// <returns>Returns a <see cref="UsbAspDevice"/>.</returns>
        public static UsbAspDevice FindUsbAsp()
        {
            UsbAspDevice result = new UsbAspDevice(false);

            var usbDevices = UsbDeviceInfo.GetUSBDevices();

            foreach (var usbDevice in usbDevices)
            {
                if (usbDevice.PnpDeviceID.Substring(0, 21) == "USB\\VID_16C0&PID_05DC")
                {
                    result = new UsbAspDevice(true, "USB\\VID_16C0&PID_05DC");
                }
            }

            return result;
        }

        /// <summary>
        /// Writes the firmware to a serial device.
        /// </summary>
        /// <param name="flashMulti">An instance of the <see cref="FlashMulti"/> class.</param>
        /// <param name="fileName">The path of the file to flash.</param>
        /// <param name="writeBootloader">Indicates whether or not the bootloader should be written.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task WriteFlash(FlashMulti flashMulti, string fileName, bool writeBootloader)
        {
            // Path to the flashing tool, stm32flash.exe
            string command = ".\\tools\\avrdude.exe";

            // Path to the bootloader file
            string bootLoaderPath = ".\\bootloaders\\AtmegaMultiBoot.hex";

            // Arguments for the command line - will vary at each step of the process
            string commandArgs;

            // Variable to keep the return code from executed commands
            int returnCode = -1;

            // First step in flash process
            int flashStep = 1;

            // Total number of steps in flash process
            int flashSteps = 2;

            // Fuses
            string lockBits = "0x0F";
            string unlockBits = "0x3F";
            string extendedFuses = "0xFD";
            string highFuses = "0xD7";
            string lowFuses = "0xFF";

            if (writeBootloader)
            {
                // Increase the total number of steps
                flashSteps = 3;

                // Set the high fuses
                highFuses = "0xD6";
            }

            // Write to the log
            flashMulti.AppendLog("Starting Multimodule update via Usbasp\r\n");

            // Erase the flash
            flashMulti.AppendLog($"[{flashStep}/{flashSteps}] Writing firmware...");

            // Set the avrdude.exe command line arguments
            commandArgs = $"-C.\\tools\\avrdude.conf -patmega328p -cusbasp -e -Ulock:w:{unlockBits}:m -Uefuse:w:{extendedFuses}:m -Uhfuse:w:{highFuses}:m -Ulfuse:w:{lowFuses}:m -Uflash:w:{bootLoaderPath}:i -Uflash:w:{fileName}:a";

            // Run the erase command asynchronously and wait for it to finish
            await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });

            // Show an error message if the command failed for any reason
            if (returnCode != 0)
            {
                flashMulti.EnableControls(true);
                flashMulti.AppendLog(" failed!");
                MessageBox.Show("Failed to write flash memory.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            flashMulti.AppendLog(" done\r\n");
            
            /*
            // Write the bootloader if required
            if (writeBootloader)
            {
                // Increment the step counter and write to the log
                flashStep++;
                flashMulti.AppendLog($"[{flashStep}/{flashSteps}] Writing bootloader...");

                // Prepare the command line arguments for writing the bootloader
                commandArgs = $"-C.\\tools\\avrdude.conf -patmega328p -cusbasp -Ulock:w:{unlockBits}:m -Uefuse:w:{extendedFuses}:m -Uhfuse:w:{highFuses}:m -Ulfuse:w:{lowFuses}:m  -Uflash:w:{bootLoaderPath}:i";

                // Run the write command asynchronously and wait for it to finish
                await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });

                // Show an error message if the command failed for any reason
                if (returnCode != 0)
                {
                    flashMulti.EnableControls(true);
                    flashMulti.AppendLog(" failed!");
                    MessageBox.Show("Failed to write the bootloader.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                flashMulti.AppendLog(" done\r\n");
            }

            /*
            // Increment the step counter and write to the log
            flashStep++;
            flashMulti.AppendLog($"[{flashStep}/{flashSteps}] Writing Multimodule firmware...");

            // Prepare the command line arguments for writing the firmware
            commandArgs = $"-v -s {flashStart} -e 0 -g {executionAddress} -b {serialBaud} -w \"{fileName}\" {comPort}";

            // Run the write command asynchronously and wait for it to finish
            await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });

            // Show an error message if the command failed for any reason
            if (returnCode != 0)
            {
                flashMulti.EnableControls(true);
                flashMulti.AppendLog(" failed!");
                MessageBox.Show("Failed to write the firmware.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Reconnect the serial monitor if it was connected before
            if (serialMonitor != null && serialMonitor.IsDisposed != true && reconnectSerialMonitor)
            {
                serialMonitor.SerialConnect(comPort);
            }

            // Write a success message to the log
            flashMulti.AppendLog(" done\r\n");
            flashMulti.AppendLog("\r\nMultimodule updated sucessfully");

            // Show a success message box
            MessageBox.Show("Multimodule updated sucessfully.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Information);

    */
            // Re-enable the form controls
            flashMulti.EnableControls(true);
        }

    }
}
