// -------------------------------------------------------------------------------
// <copyright file="UsbAspDevice.cs" company="Ben Lye">
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
    using System.Threading.Tasks;
    using System.Windows.Forms;

    /// <summary>
    /// Class for working with a USBasp device.
    /// </summary>
    internal class UsbAspDevice
    {
        // Fuses
        private const string UnlockBits = "0x3F";
        private const string LockBits = "0x0F";
        private const string ExtendedFuses = "0xFD";
        private const string HighFusesBoot = "0xD6";
        private const string HighFusesNoBoot = "0xD7";
        private const string LowFuses = "0xFF";

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
        /// Erases the module via a USBasp device.
        /// </summary>
        /// <param name="flashMulti">An instance of the <see cref="FlashMulti"/> class.</param>
        /// <param name="eraseEeprom">Flag indicating if the EEPROM should be erased.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<bool> EraseFlash(FlashMulti flashMulti, bool eraseEeprom)
        {
            // Path to the flashing tool, stm32flash.exe
            string command = ".\\tools\\avrdude.exe";

            // Arguments for the command line
            string commandArgs;

            // Variable to keep the return code from executed commands
            int returnCode = -1;

            // First step in flash process
            flashMulti.FlashStep = 1;

            // Avrdude command arguments
            if (eraseEeprom)
            {
                flashMulti.FlashSteps = 6;
                commandArgs = $"-C.\\tools\\avrdude.conf -patmega328p -cusbasp -Ulock:w:{UnlockBits}:m -Uefuse:w:{ExtendedFuses}:m -Uhfuse:w:{HighFusesNoBoot}:m -Ulfuse:w:{LowFuses}:m -Uflash:w:.\\tools\\erase32.bin:a -Ueeprom:w:.\\tools\\erase1.bin:r";
            }
            else
            {
                flashMulti.FlashSteps = 4;
                commandArgs = $"-C.\\tools\\avrdude.conf -patmega328p -cusbasp -Ulock:w:{UnlockBits}:m -Uefuse:w:{ExtendedFuses}:m -Uhfuse:w:{HighFusesNoBoot}:m -Ulfuse:w:{LowFuses}:m -Uflash:w:.\\tools\\erase32.bin:a";
            }

            // Write to the log
            flashMulti.AppendLog("Erasing MULTI-Module via USBasp\r\n");

            // Run the command asynchronously and wait for it to finish
            await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });

            // Show an error message if the command failed for any reason
            if (returnCode != 0)
            {
                flashMulti.EnableControls(true);
                flashMulti.AppendLog("\r\nModule erase failed!");
                MessageBox.Show("Failed to erase the module.", "MULTI-Module Erase", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            flashMulti.AppendLog("\r\nMULTI-Module erased successfully");

            flashMulti.EnableControls(true);

            return true;
        }

        /// <summary>
        /// Reads the firmware via a USBasp device.
        /// </summary>
        /// <param name="flashMulti">An instance of the <see cref="FlashMulti"/> class.</param>
        /// <param name="firmwareFileName">The path of the file to save the firmware in.</param>
        /// <param name="eepromFilename">The path of the file to save the EEPROM in.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<bool> ReadFlash(FlashMulti flashMulti, string firmwareFileName, string eepromFilename)
        {
            // Path to the flashing tool, stm32flash.exe
            string command = ".\\tools\\avrdude.exe";

            // Arguments for the command line
            string commandArgs = $"-C.\\tools\\avrdude.conf -patmega328p -cusbasp -Uflash:r:{firmwareFileName}:r -Ueeprom:r:{eepromFilename}:r";

            // Variable to keep the return code from executed commands
            int returnCode = -1;

            // First step in flash process
            flashMulti.FlashStep = 1;

            // Total number of steps in flash process
            flashMulti.FlashSteps = 2;

            // Write to the log
            flashMulti.AppendLog("Reading MULTI-Module via USBasp\r\n");

            // Run the command asynchronously and wait for it to finish
            await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });

            // Show an error message if the command failed for any reason
            if (returnCode != 0)
            {
                flashMulti.EnableControls(true);
                flashMulti.AppendLog("\r\nMULTI-Module read failed!");
                MessageBox.Show("Failed to read the MULTI-module.", "MULTI-Module Read", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            flashMulti.AppendLog("\r\n");

            flashMulti.EnableControls(true);

            return true;
        }

        /// <summary>
        /// Writes the firmware via a USBasp device.
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

            // Default avrdude command arguments (no bootloader)
            commandArgs = $"-C.\\tools\\avrdude.conf -patmega328p -cusbasp -Ulock:w:{UnlockBits}:m -Uefuse:w:{ExtendedFuses}:m -Uhfuse:w:{HighFusesNoBoot}:m -Ulfuse:w:{LowFuses}:m -Uflash:w:{fileName}:a";

            if (writeBootloader)
            {
                // Increase the total number of steps
                flashMulti.FlashSteps = 5;

                commandArgs = $"-C.\\tools\\avrdude.conf -patmega328p -cusbasp -Ulock:w:{UnlockBits}:m -Uefuse:w:{ExtendedFuses}:m -Uhfuse:w:{HighFusesBoot}:m -Ulfuse:w:{LowFuses}:m -Uflash:w:{bootLoaderPath}:i -Ulock:w:{LockBits}:m -Uflash:w:{fileName}:a";
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

            flashMulti.AppendLog("\r\nMULTI-Module updated successfully");

            MessageBox.Show("MULTI-Module updated successfully.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Information);

            flashMulti.EnableControls(true);
        }
    }
}
