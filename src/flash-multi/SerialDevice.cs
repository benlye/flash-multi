﻿// -------------------------------------------------------------------------------
// <copyright file="SerialDevice.cs" company="Ben Lye">
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
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    /// <summary>
    /// Class for working with serial devices.
    /// </summary>
    internal class SerialDevice
    {
        /// <summary>
        /// Writes the firmware to a serial device.
        /// </summary>
        /// <param name="flashMulti">An instance of the <see cref="FlashMulti"/> class.</param>
        /// <param name="fileName">The path of the file to flash.</param>
        /// <param name="comPort">The COM port where the serial device can be found.</param>
        /// <param name="writeBootloader">Indicates whether or not the bootloader should be written.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task WriteFlash(FlashMulti flashMulti, string fileName, string comPort, bool writeBootloader)
        {
            // Path to the flashing tool, stm32flash.exe
            string command = ".\\tools\\stm32flash.exe";

            // Path to the bootloader file
            string bootLoaderPath = ".\\bootloaders\\StmMulti4in1.bin";

            // Baud rate for serial flash commands
            int serialBaud = Properties.Settings.Default.SerialBaudRate;

            // Arguments for the command line - will vary at each step of the process
            string commandArgs;

            // Variable to keep the return code from executed commands
            int returnCode = -1;

            // First step in flash process
            int flashStep = 1;

            // Total number of steps in flash process
            int flashSteps = 2;

            // Page in the STM32 flash memory where we will begin writing
            int flashStart = 0;

            // Address where we will start execution after flashing
            string executionAddress = "0x8000000";

            if (writeBootloader)
            {
                // Increase the total number of steps
                flashSteps = 3;

                // The bootloader occupies the first 8 pages (0-7), so start writing after it
                flashStart = 8;

                // Start execution at the Multiprotocol code rather than the bootloader
                executionAddress = "0x8002000";
            }

            // Write to the log
            flashMulti.AppendLog("Starting Multimodule update via serial\r\n");

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

            // Erase the flash
            flashMulti.AppendLog($"[{flashStep}/{flashSteps}] Erasing flash memory...");

            // Set the stm32flash.exe command line arguments for erasing
            // We preserve the last 2KB of flash, which is where the EEPROM data lives.
            commandArgs = $"-o -S 0x8000000:129024 -b {serialBaud} {comPort}";

            // Run the erase command asynchronously and wait for it to finish
            await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });

            // Show an error message if the erase command failed for any reason
            if (returnCode != 0)
            {
                flashMulti.EnableControls(true);
                flashMulti.AppendLog(" failed!");
                MessageBox.Show("Failed to erase flash memory.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            flashMulti.AppendLog(" done\r\n");

            // Write the bootloader if required
            if (writeBootloader)
            {
                // Increment the step counter and write to the log
                flashStep++;
                flashMulti.AppendLog($"[{flashStep}/{flashSteps}] Writing bootloader...");

                // Prepare the command line arguments for writing the bootloader
                commandArgs = $"-v -e 0 -g 0x8000000 -b {serialBaud} -w \"{bootLoaderPath}\" {comPort}";

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

            // Re-enable the form controls
            flashMulti.EnableControls(true);
        }
    }
}
