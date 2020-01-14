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
    using System.Threading.Tasks;
    using System.Windows.Forms;

    /// <summary>
    /// Class for working with a USBasp device.
    /// </summary>
    internal class UsbAspDevice
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
            flashMulti.FlashStep = 1;

            // Total number of steps in flash process
            flashMulti.FlashSteps = 4;

            // Fuses
            string unlockBits = "0x3F";
            string lockBits = "0x0F";
            string extendedFuses = "0xFD";
            string highFuses = "0xD7";
            string lowFuses = "0xFF";

            // Default avrdude command arguments (no bootloader)
            commandArgs = $"-C.\\tools\\avrdude.conf -patmega328p -cusbasp -Ulock:w:{unlockBits}:m -Uefuse:w:{extendedFuses}:m -Uhfuse:w:{highFuses}:m -Ulfuse:w:{lowFuses}:m -Uflash:w:{fileName}:a";

            if (writeBootloader)
            {
                // Increase the total number of steps
                flashMulti.FlashSteps = 5;

                // Set the high fuses
                highFuses = "0xD6";

                commandArgs = $"-C.\\tools\\avrdude.conf -patmega328p -cusbasp -Ulock:w:{unlockBits}:m -Uefuse:w:{extendedFuses}:m -Uhfuse:w:{highFuses}:m -Ulfuse:w:{lowFuses}:m -Uflash:w:{bootLoaderPath}:i -Ulock:w:{lockBits}:m -Uflash:w:{fileName}:a";
            }

            // Write to the log
            flashMulti.AppendLog("Starting MULTI-Module update via USBasp\r\n");

            // Run the command asynchronously and wait for it to finish
            await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });

            // Show an error message if the command failed for any reason
            if (returnCode != 0)
            {
                flashMulti.EnableControls(true);
                flashMulti.AppendLog("\r\nFirmware update failed!");
                MessageBox.Show("Failed to write flash memory.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            flashMulti.AppendLog("\r\nDone.");
            flashMulti.AppendLog("\r\nMULTI-Module updated successfully");

            MessageBox.Show("MULTI-Module updated successfully.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Information);

            flashMulti.EnableControls(true);
        }
    }
}
