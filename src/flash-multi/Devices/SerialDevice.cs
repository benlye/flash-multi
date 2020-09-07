// -------------------------------------------------------------------------------
// <copyright file="SerialDevice.cs" company="Ben Lye">
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
    using System.Collections;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    /// <summary>
    /// Class for working with serial devices.
    /// </summary>
    internal class SerialDevice
    {
        /// <summary>
        /// Erases the flash memory of a serial device.
        /// </summary>
        /// <param name="flashMulti">An instance of the <see cref="FlashMulti"/> class.</param>
        /// <param name="comPort">The COM port where the serial device can be found.</param>
        /// <param name="eraseEeprom">Flag indicating if the EEPROM should be erased.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<bool> EraseFlash(FlashMulti flashMulti, string comPort, bool eraseEeprom)
        {
            // Path to the flashing tool, stm32flash.exe
            string command = ".\\tools\\stm32flash.exe";

            // Baud rate for serial flash commands
            int serialBaud = Properties.Settings.Default.SerialBaudRate;

            // Arguments for the command line - will vary at each step of the process
            string commandArgs;

            // Variable to keep the return code from executed commands
            int returnCode = -1;

            // By default we preserve the last 2KB of flash, which is where the EEPROM data lives.
            int eraseBytes = 129024;

            // But if we're writing the EEPROM we need to erase it first
            if (eraseEeprom)
            {
                eraseBytes = 131072;
            }

            // Write to the log
            flashMulti.AppendLog($"{Strings.modeErasing} {Strings.viaSerial}\r\n");

            // Stop the serial monitor if it's active
            SerialMonitor serialMonitor = null;
            if (Application.OpenForms.OfType<SerialMonitor>().Any())
            {
                Debug.WriteLine("Serial monitor window is open");
                serialMonitor = Application.OpenForms.OfType<SerialMonitor>().First();
                if (serialMonitor != null && serialMonitor.SerialPort != null && serialMonitor.SerialPort.IsOpen)
                {
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
                flashMulti.AppendLog($"{Strings.failedToOpenPort} {comPort}");
                MessageBox.Show($"{Strings.failedToOpenPort} {comPort}", Strings.dialogTitleErase, MessageBoxButtons.OK, MessageBoxIcon.Error);
                flashMulti.EnableControls(true);
                return false;
            }

            // Write to the log
            flashMulti.AppendLog($"[1/1] {Strings.progressErasingFlash}");

            // Prepare the command line arguments for erasing the firmware
            commandArgs = $"-o -S 0x8000000:{eraseBytes} -b {serialBaud} {comPort}";

            // Run the write command asynchronously and wait for it to finish
            await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });

            // Show an error message if the command failed for any reason
            if (returnCode != 0)
            {
                flashMulti.EnableControls(true);
                flashMulti.AppendLog(Strings.failed);
                MessageBox.Show(Strings.failedtoEraseModule, Strings.dialogTitleErase, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Write a success message to the log
            flashMulti.AppendLog($" {Strings.done}\r\n");
            flashMulti.AppendLog($"\r\n{Strings.succeededErasing}");

            // Re-enable the form controls
            flashMulti.EnableControls(true);

            return true;
        }

        /// <summary>
        /// Reads the flash memory of a serial device.
        /// </summary>
        /// <param name="flashMulti">An instance of the <see cref="FlashMulti"/> class.</param>
        /// <param name="fileName">The path of the file to write to.</param>
        /// <param name="comPort">The COM port where the serial device can be found.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<bool> ReadFlash(FlashMulti flashMulti, string fileName, string comPort)
        {
            // Path to the flashing tool, stm32flash.exe
            string command = ".\\tools\\stm32flash.exe";

            // Baud rate for serial flash commands
            int serialBaud = Properties.Settings.Default.SerialBaudRate;

            // Arguments for the command line - will vary at each step of the process
            string commandArgs;

            // Variable to keep the return code from executed commands
            int returnCode = -1;

            // Write to the log
            flashMulti.AppendLog($"{Strings.modeReading} {Strings.viaSerial}\r\n");

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
                flashMulti.AppendLog($"{Strings.failedToOpenPort} {comPort}");
                MessageBox.Show($"{Strings.failedToOpenPort} {comPort}", Strings.dialogTitleRead, MessageBoxButtons.OK, MessageBoxIcon.Error);
                flashMulti.EnableControls(true);
                return false;
            }

            // Write to the log
            flashMulti.AppendLog($"[1/1] {Strings.progressReadingFlash}");

            // Prepare the command line arguments for writing the firmware
            commandArgs = $"-b {serialBaud} -r \"{fileName}\" {comPort}";

            // Run the write command asynchronously and wait for it to finish
            await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });

            // Show an error message if the command failed for any reason
            if (returnCode != 0)
            {
                flashMulti.AppendLog($" {Strings.failed}");
                MessageBox.Show(Strings.failedToReadModule, Strings.dialogTitleRead, MessageBoxButtons.OK, MessageBoxIcon.Error);
                flashMulti.EnableControls(true);
                return false;
            }

            // Reconnect the serial monitor if it was connected before
            if (serialMonitor != null && serialMonitor.IsDisposed != true && reconnectSerialMonitor)
            {
                serialMonitor.SerialConnect(comPort);
            }

            // Write a success message to the log
            flashMulti.AppendLog($" {Strings.done}\r\n\r\n");

            // Re-enable the form controls
            flashMulti.EnableControls(true);

            return true;
        }

        /// <summary>
        /// Writes the firmware to a serial device.
        /// </summary>
        /// <param name="flashMulti">An instance of the <see cref="FlashMulti"/> class.</param>
        /// <param name="fileName">The path of the file to flash.</param>
        /// <param name="comPort">The COM port where the serial device can be found.</param>
        /// <param name="writeBootloader">Indicates whether or not the bootloader should be written.</param>
        /// <param name="writeEeprom">Indicates whether or not the EEPROM is being written, therefore should be erased before the write.</param>
        /// <param name="runAfterUpload">Indicates whether or not the firmware should run after it is uploaded.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task WriteFlash(FlashMulti flashMulti, string fileName, string comPort, bool writeBootloader, bool writeEeprom, bool runAfterUpload)
        {
            // Path to the flashing tool, stm32flash.exe
            string command = ".\\tools\\stm32flash.exe";

            // Path to the bootloader file
            string bootLoaderPath;
            if (Properties.Settings.Default.ErrorIfNoUSB)
            {
                bootLoaderPath = ".\\bootloaders\\StmMulti4in1_Legacy.bin";
            }
            else
            {
                bootLoaderPath = ".\\bootloaders\\StmMulti4in1_StickyDfu.bin";
            }

            // Baud rate for serial flash commands
            int serialBaud = Properties.Settings.Default.SerialBaudRate;

            // Arguments for the command line - will vary at each step of the process
            string commandArgs;

            // Variable to keep the return code from executed commands
            int returnCode = -1;

            // First step in flash process
            int flashStep = 0;

            // Total number of steps in flash process
            int flashSteps = 2;

            // Page in the STM32 flash memory where we will begin writing
            int flashStart = 0;

            // By default we preserve the last 2KB of flash, which is where the EEPROM data lives.
            int eraseBytes = 129024;

            // But if we're writing the EEPROM we need to erase it first
            if (writeEeprom)
            {
                eraseBytes = 131072;
            }

            // Address where we will start execution after flashing
            string executionAddress = "0x8000000";

            if (writeBootloader)
            {
                // Increase the total number of steps
                flashSteps++;

                // The bootloader occupies the first 8 pages (0-7), so start writing after it
                flashStart = 8;

                // Start execution at the MULTI-Module code rather than the bootloader
                executionAddress = "0x8002000";
            }

            // Write to the log
            flashMulti.AppendLog($"{Strings.modeWriting} {Strings.viaSerial}\r\n");

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
                flashMulti.AppendLog($"{Strings.failedToOpenPort} {comPort}");
                MessageBox.Show($"{Strings.failedToOpenPort} {comPort}", Strings.dialogTitleWrite, MessageBoxButtons.OK, MessageBoxIcon.Error);
                flashMulti.EnableControls(true);
                return;
            }

            if (!Properties.Settings.Default.DisableFlashVerification)
            {
                flashStep++;

                // Increase the total number of steps
                flashSteps++;

                // Check that the STM32 MCU supports 128KB
                flashMulti.AppendLog($"[{flashStep}/{flashSteps}] {Strings.progressCheckingFlashSize}");

                // Create a temporary file name to read into
                string tempFileName = Path.GetTempFileName();

                // Set the stm32flash.exe command line arguments for reading the 32B of flash above 64KB
                commandArgs = $"-r {tempFileName} -S 0x8010000:32 -b {serialBaud} {comPort}";

                // Run the read command asynchronously and wait for it to finish
                await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });

                // Show an error message if the read command failed for any reason
                if (returnCode != 0)
                {
                    flashMulti.EnableControls(true);
                    flashMulti.AppendLog($" {Strings.failed}");
                    MessageBox.Show(Strings.failedToReadModule, Strings.dialogTitleWrite, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                flashMulti.AppendLog($" {Strings.done}\r\n");

                // Read the data we read from the module
                byte[] byteBuffer = File.ReadAllBytes(tempFileName);

                // 32 Bytes of bad data to compare to
                byte[] badData = { 7, 73, 8, 128, 7, 73, 8, 128, 7, 73, 8, 128, 7, 73, 8, 128, 7, 73, 8, 128, 7, 73, 8, 128, 7, 73, 8, 128, 7, 73, 8, 128 };

                // Compare the data we read to the known 'bad' data
                if (StructuralComparisons.StructuralEqualityComparer.Equals(byteBuffer, badData))
                {
                    // Throw a message and stop
                    flashMulti.EnableControls(true);
                    MessageBox.Show(Strings.failedToVerifyMcuFlash, Strings.dialogTitleWrite, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Increment the step counter and write to the log
            flashStep++;
            flashMulti.AppendLog($"[{flashStep}/{flashSteps}] {Strings.progressErasingFlash}");

            // Set the stm32flash.exe command line arguments for erasing
            commandArgs = $"-o -S 0x8000000:{eraseBytes} -b {serialBaud} {comPort}";

            // Run the erase command asynchronously and wait for it to finish
            await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });

            // Show an error message if the erase command failed for any reason
            if (returnCode != 0)
            {
                flashMulti.EnableControls(true);
                flashMulti.AppendLog($" {Strings.failed}");
                MessageBox.Show(Strings.failedtoEraseModule, Strings.dialogTitleWrite, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            flashMulti.AppendLog($" {Strings.done}\r\n");

            // Write the bootloader if required
            if (writeBootloader)
            {
                // Increment the step counter and write to the log
                flashStep++;
                flashMulti.AppendLog($"[{flashStep}/{flashSteps}] {Strings.progressWritingBootloader}");

                // Prepare the command line arguments for writing the bootloader
                commandArgs = $"-v -e 0 -g 0x8000000 -b {serialBaud} -w \"{bootLoaderPath}\" {comPort}";

                // Run the write command asynchronously and wait for it to finish
                await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });

                // Show an error message if the command failed for any reason
                if (returnCode != 0)
                {
                    flashMulti.EnableControls(true);
                    flashMulti.AppendLog($" {Strings.failed}");
                    MessageBox.Show(Strings.failedToWriteBootloader, Strings.dialogTitleWrite, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                flashMulti.AppendLog($" {Strings.done}\r\n");
            }

            // Increment the step counter and write to the log
            flashStep++;
            flashMulti.AppendLog($"[{flashStep}/{flashSteps}] {Strings.progressWritingFirmware}");

            // Prepare the command line arguments for writing the firmware
            commandArgs = $"-v -s {flashStart} -e 0 -b {serialBaud} -w \"{fileName}\" {comPort}";

            if (runAfterUpload)
            {
                commandArgs += $" -g {executionAddress}";
            }

            // Run the write command asynchronously and wait for it to finish
            await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });

            // Show an error message if the command failed for any reason
            if (returnCode != 0)
            {
                flashMulti.EnableControls(true);
                flashMulti.AppendLog($" {Strings.failed}");
                MessageBox.Show(Strings.failedToWriteFirmware, Strings.dialogTitleWrite, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Reconnect the serial monitor if it was connected before
            if (serialMonitor != null && serialMonitor.IsDisposed != true && reconnectSerialMonitor)
            {
                serialMonitor.SerialConnect(comPort);
            }

            // Write a success message to the log
            flashMulti.AppendLog($" {Strings.done}\r\n");
            flashMulti.AppendLog($"\r\n{Strings.succeededWritingFirmware}");

            // Re-enable the form controls
            flashMulti.EnableControls(true);

            // Show a success message box
            MessageBox.Show(Strings.succeededWritingFirmware, Strings.succeededWritingFirmware, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
