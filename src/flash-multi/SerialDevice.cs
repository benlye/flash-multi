// -------------------------------------------------------------------------------
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
    using System;
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
        public static async void WriteFlash(FlashMulti flashMulti, string fileName, string comPort)
        {
            string command = ".\\tools\\stm32flash.exe";
            string bootLoaderPath = ".\\bootloaders\\StmMulti4in1.bin";
            string commandArgs;

            int returnCode = -1;
            int flashStep = 1;
            int flashSteps = 2;

            int flashStart = 0;
            string executionAddress = "0x8000000";

            if (flashMulti.writeBootloader_Yes.Checked)
            {
                flashSteps = 3;
            }

            flashMulti.AppendLog("Starting Multimodule update\r\n");

            // Erase
            flashMulti.AppendLog($"[{flashStep}/{flashSteps}] Erasing flash memory...");
            commandArgs = string.Format("-o -S 0x8000000:129024 -b 115200 {0}", comPort);
            await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });
            if (returnCode != 0)
            {
                flashMulti.EnableControls(true);
                flashMulti.AppendLog(" failed!");
                MessageBox.Show("Failed to erase flash memory.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            flashMulti.AppendLog(" done\r\n");

            if (flashMulti.writeBootloader_Yes.Checked)
            {
                // Flash bootloader
                flashStep++;
                flashMulti.AppendLog($"[{flashStep}/{flashSteps}] Writing bootloader...");
                commandArgs = $"-v -e 0 -g {executionAddress} -b 115200 -w \"{bootLoaderPath}\" {comPort}";
                await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });
                if (returnCode != 0)
                {
                    flashMulti.EnableControls(true);
                    flashMulti.AppendLog(" failed!");
                    MessageBox.Show("Failed to write the bootloader.", "Firmware Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                flashMulti.AppendLog(" done\r\n");

                // Set the flash address for a flash with bootloader
                flashStart = 8;
                executionAddress = "0x8002000";
            }

            // Flash firmware
            flashStep++;
            flashMulti.AppendLog($"[{flashStep}/{flashSteps}] Writing Multimodule firmware...");
            commandArgs = $"-v -s {flashStart} -e 0 -g {executionAddress} -b 115200 -w \"{fileName}\" {comPort}";
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
