// -------------------------------------------------------------------------------
// <copyright file="MapleDevice.cs" company="Ben Lye">
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
    using System;
    using System.Collections.Generic;
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
        /// <param name="deviceName">The device name.</param>
        /// <param name="dfuMode">Indicates if the device is in DFU mode.</param>
        /// <param name="usbMode">Indicates if the device is in USB mode.</param>
        /// <param name="mode">The device mode.</param>
        public MapleDevice(bool deviceFound = false, string deviceId = null, string deviceName = null, bool dfuMode = false, bool usbMode = false, string mode = null)
        {
            this.DeviceFound = deviceFound;
            this.DeviceId = deviceId;
            this.DeviceName = deviceName;
            this.DfuMode = dfuMode;
            this.UsbMode = usbMode;
            this.Mode = mode;
        }

        /// <summary>
        /// Gets a value indicating whether a Maple device was found.
        /// </summary>
        public bool DeviceFound { get; private set; }

        /// <summary>
        /// Gets a string containing the device ID.
        /// </summary>
        public string DeviceId { get; private set; }

        /// <summary>
        /// Gets a string containing the device name.
        /// </summary>
        public string DeviceName { get; private set; }

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
        /// Async method to find a Maple device in either USB or DFU mode.
        /// </summary>
        /// <returns>Returns a <see cref="MapleDevice"/>.</returns>
        public static async Task<MapleDevice> FindMapleAsync()
        {
            MapleDevice mapleDevice = await Task.Run(() => FindMaple());
            return mapleDevice;
        }

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
                        result = new MapleDevice(true, "USB\\VID_1EAF&PID_0003", usbDevice.Name, true, false, "DFU");
                        break;
                    case "USB\\VID_1EAF&PID_0004":
                        result = new MapleDevice(true, "USB\\VID_1EAF&PID_0004", usbDevice.Name, false, true, "USB");
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
        /// Gets the name of the Maple Device COM port.
        /// </summary>
        /// <returns>A string containing the COM port name.</returns>
        public static string GetMapleComPort()
        {
            string portName = null;

            MapleDevice device = FindMaple();

            if (device.DeviceFound)
            {
                _ = new List<ComPort>();
                List<ComPort> comPorts = ComPort.EnumeratePortList();

                foreach (ComPort port in comPorts)
                {
                    if (device.DeviceName.Contains(port.Name))
                    {
                        portName = port.Name;
                    }
                }
            }

            return portName;
        }

        /// <summary>
        /// Erases the flash memory of a Maple USB device.
        /// </summary>
        /// <param name="flashMulti">An instance of the <see cref="FlashMulti"/> class.</param>
        /// <param name="comPort">The COM port where the Maple USB device can be found.</param>
        /// <param name="eraseEeprom">Flag indicating if the EEPROM should be erased.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<bool> EraseFlash(FlashMulti flashMulti, string comPort, bool eraseEeprom)
        {
            string command;
            string commandArgs;
            int returnCode = -1;

            flashMulti.AppendLog($"{Strings.modeErasing} {Strings.viaNativeUSB}\r\n");

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

            string mapleMode = MapleDevice.FindMaple().Mode;

            if (mapleMode == "USB")
            {
                flashMulti.AppendLog(Strings.progressSwitchingToDfuMode);
                command = ".\\tools\\maple-reset.exe";
                commandArgs = $"{comPort} 2000";
                await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });
                if (returnCode == 0)
                {
                    flashMulti.AppendLog($" {Strings.done}\r\n");
                    flashMulti.AppendVerbose(string.Empty);
                }
                else
                {
                    if (MapleDevice.FindMaple().DfuMode == false)
                    {
                        flashMulti.AppendLog($"  {Strings.failed}\r\n");
                        MessageBox.Show(Strings.failedtoEraseModule, Strings.dialogTitleErase, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        flashMulti.EnableControls(true);
                        return false;
                    }
                }
            }

            // It's not actually possible to erase a device with dfu-util so we write a file full of FF bytes.
            string fileName = string.Empty;
            if (eraseEeprom)
            {
                // 120KB file overwrites the EEPROM
                fileName = ".\\tools\\erase120.bin";
            }
            else
            {
                // 118KB file preserves the EEPROM
                fileName = ".\\tools\\erase118.bin";
            }

            // Erase the the flash
            flashMulti.AppendLog(Strings.progressErasingFlash);
            command = ".\\tools\\dfu-util-multi.exe";
            commandArgs = string.Format("-a 2 -d 1EAF:0003 -D \"{0}\" -v", fileName, comPort);
            await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });
            if (returnCode != 0)
            {
                flashMulti.AppendLog($" {Strings.failed}\r\n");
                MessageBox.Show(Strings.failedtoEraseModule, Strings.dialogTitleErase, MessageBoxButtons.OK, MessageBoxIcon.Error);
                flashMulti.EnableControls(true);
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
        /// Reads the flash memory of a Maple USB device.
        /// </summary>
        /// <param name="flashMulti">An instance of the <see cref="FlashMulti"/> class.</param>
        /// <param name="fileName">The path of the file to write to.</param>
        /// <param name="comPort">The COM port where the Maple USB device can be found.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<bool> ReadFlash(FlashMulti flashMulti, string fileName, string comPort)
        {
            string command;
            string commandArgs;
            int returnCode = -1;

            flashMulti.AppendLog($"{Strings.modeReading} {Strings.viaNativeUSB}\r\n");

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

            string mapleMode = MapleDevice.FindMaple().Mode;

            if (mapleMode == "USB")
            {
                flashMulti.AppendLog($"{Strings.progressSwitchingToDfuMode}");
                command = ".\\tools\\maple-reset.exe";
                commandArgs = $"{comPort} 2000";
                await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });
                if (returnCode == 0)
                {
                    flashMulti.AppendLog($" {Strings.done}\r\n");
                    flashMulti.AppendVerbose(string.Empty);
                }
                else
                {
                    if (MapleDevice.FindMaple().DfuMode == false)
                    {
                        flashMulti.AppendLog($" {Strings.failed}\r\n");
                        MessageBox.Show(Strings.failedToReadModule, Strings.dialogTitleRead, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        flashMulti.EnableControls(true);
                        return false;
                    }
                }
            }

            // Read the flash memory
            flashMulti.AppendLog(Strings.progressReadingFlash);
            command = ".\\tools\\dfu-util-multi.exe";
            commandArgs = string.Format("-a 2 -d 1EAF:0003 -U \"{0}\" -v", fileName, comPort);
            await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });
            if (returnCode != 0)
            {
                flashMulti.AppendLog($" {Strings.failed}\r\n");
                MessageBox.Show(Strings.failedToReadModule, Strings.dialogTitleRead, MessageBoxButtons.OK, MessageBoxIcon.Error);
                flashMulti.EnableControls(true);
                return false;
            }

            // Write a success message to the log
            flashMulti.AppendLog($" {Strings.done}\r\n\r\n");

            // Reconnect the serial monitor
            if (serialMonitor != null && reconnectSerialMonitor)
            {
                Thread.Sleep(1000);
                serialMonitor.SerialConnect(comPort);
            }

            // Re-enable the form controls
            flashMulti.EnableControls(true);

            return true;
        }

        /// <summary>
        /// Writes the firmware to a Maple serial or DFU device.
        /// </summary>
        /// <param name="flashMulti">An instance of the <see cref="FlashMulti"/> class.</param>
        /// <param name="fileName">The path of the file to flash.</param>
        /// <param name="comPort">The COM port where the Maple USB device can be found.</param>
        /// <param name="runAfterUpload">Indicates if the firmware should be run when the upload is finished.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task WriteFlash(FlashMulti flashMulti, string fileName, string comPort, bool runAfterUpload)
        {
            string command;
            string commandArgs;
            int returnCode = -1;

            flashMulti.AppendLog($"{Strings.modeWriting} {Strings.viaNativeUSB}\r\n");

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

            string mapleMode = MapleDevice.FindMaple().Mode;

            if (mapleMode == "USB")
            {
                flashMulti.AppendLog(Strings.progressSwitchingToDfuMode);
                command = ".\\tools\\maple-reset.exe";
                commandArgs = $"{comPort} 2000";
                await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });
                if (returnCode == 0)
                {
                    flashMulti.AppendLog($" {Strings.done}\r\n");
                    flashMulti.AppendVerbose(string.Empty);
                }
                else
                {
                    if (MapleDevice.FindMaple().DfuMode == false)
                    {
                        flashMulti.AppendLog($" {Strings.failed}\r\n");
                        flashMulti.AppendLog($"{Strings.dfuAttemptingRecovery}\r\n");

                        // Show the recovery mode dialog
                        DfuRecoveryDialog recoveryDialog = new DfuRecoveryDialog(flashMulti);
                        var recoveryResult = recoveryDialog.ShowDialog();

                        // Stop if we didn't make it into recovery mode
                        if (recoveryResult == DialogResult.Cancel)
                        {
                            flashMulti.AppendLog(Strings.dfuRecoveryCancelled);
                            flashMulti.EnableControls(true);
                            return;
                        }
                        else if (recoveryResult == DialogResult.Abort)
                        {
                            flashMulti.AppendLog(Strings.dfuRecoveryFailed);
                            flashMulti.EnableControls(true);
                            return;
                        }
                    }
                    else
                    {
                        flashMulti.AppendLog($" {Strings.done}\r\n");
                        flashMulti.AppendVerbose(string.Empty);
                    }
                }
            }

            // First attempt to flash the firmware
            flashMulti.AppendLog(Strings.progressWritingFirmware);
            command = ".\\tools\\dfu-util-multi.exe";
            commandArgs = string.Format("-a 2 -d 1EAF:0003 -D \"{0}\" -v", fileName, comPort);

            if (runAfterUpload)
            {
                commandArgs += " -R";
            }

            await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });
            if (returnCode != 0)
            {
                // First attempt failed so we need to try bootloader recovery
                flashMulti.AppendLog($" {Strings.failed}\r\n");

                flashMulti.AppendLog($"{Strings.dfuAttemptingRecovery}\r\n");

                // Show the recovery mode dialog
                DfuRecoveryDialog recoveryDialog = new DfuRecoveryDialog(flashMulti);
                var recoveryResult = recoveryDialog.ShowDialog();

                // If we made it into recovery mode, flash the module
                if (recoveryResult == DialogResult.OK)
                {
                    // Run the recovery flash command
                    flashMulti.AppendLog(Strings.progressWritingFirmware);
                    await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });
                    if (returnCode != 0)
                    {
                        flashMulti.AppendLog($" {Strings.failed}!\r\n");
                        MessageBox.Show(Strings.failedToWriteFirmware, Strings.dialogTitleWrite, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        flashMulti.EnableControls(true);
                        return;
                    }
                }
                else if (recoveryResult == DialogResult.Cancel)
                {
                    flashMulti.AppendLog(Strings.dfuRecoveryCancelled);
                    flashMulti.EnableControls(true);
                    return;
                }
                else
                {
                    flashMulti.AppendLog(Strings.dfuRecoveryFailed);
                    flashMulti.EnableControls(true);
                    return;
                }
            }

            flashMulti.AppendLog($" {Strings.done}\r\n");
            flashMulti.AppendLog($"\r\n{Strings.succeededWritingFirmware}");

            // Reconnect the serial monitor
            if (serialMonitor != null && reconnectSerialMonitor)
            {
                Thread.Sleep(1000);
                serialMonitor.SerialConnect(comPort);
            }

            // Re-enable the form controls
            flashMulti.EnableControls(true);

            // Show a success message box
            MessageBox.Show(Strings.succeededWritingFirmware, Strings.dialogTitleWrite, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Upgrades the bootloader of a Maple serial or DFU device.
        /// </summary>
        /// <param name="flashMulti">An instance of the <see cref="FlashMulti"/> class.</param>
        /// <param name="comPort">The COM port where the Maple USB device can be found.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<bool> UpgradeBootloader(FlashMulti flashMulti, string comPort)
        {
            string command;
            string commandArgs;
            int returnCode = -1;

            flashMulti.AppendLog($"{Strings.modeBootloaderUpgrade} {Strings.viaNativeUSB}\r\n");

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
                MessageBox.Show($"{Strings.failedToOpenPort} {comPort}", Strings.dialogTitleBootloaderUpgrade, MessageBoxButtons.OK, MessageBoxIcon.Error);
                flashMulti.EnableControls(true);
                return false;
            }

            string mapleMode = MapleDevice.FindMaple().Mode;

            if (mapleMode == "USB")
            {
                flashMulti.AppendLog(Strings.progressSwitchingToDfuMode);
                command = ".\\tools\\maple-reset.exe";
                commandArgs = $"{comPort} 2000";
                await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });
                if (returnCode == 0)
                {
                    flashMulti.AppendLog($" {Strings.done}\r\n");
                    flashMulti.AppendVerbose(string.Empty);
                }
                else
                {
                    if (MapleDevice.FindMaple().DfuMode == false)
                    {
                        flashMulti.AppendLog($"  {Strings.failed}\r\n");
                        MessageBox.Show(Strings.failedToWriteBootReloader, Strings.dialogTitleBootloaderUpgrade, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        flashMulti.EnableControls(true);
                        return false;
                    }
                }
            }

            // The bootreloader.bin file
            string fileName = ".\\tools\\bootreloader.bin";

            // Erase the the flash
            flashMulti.AppendLog(Strings.progressWritingBootReloader);
            command = ".\\tools\\dfu-util-multi.exe";
            commandArgs = string.Format("-a 2 -d 1EAF:0003 -D \"{0}\" -v -R", fileName, comPort);
            await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });
            if (returnCode != 0)
            {
                flashMulti.AppendLog($" {Strings.failed}\r\n");
                MessageBox.Show(Strings.failedToWriteBootReloader, Strings.dialogTitleErase, MessageBoxButtons.OK, MessageBoxIcon.Error);
                flashMulti.EnableControls(true);
                return false;
            }

            // Write a success message to the log
            flashMulti.AppendLog($" {Strings.done}\r\n");
            flashMulti.AppendLog($"\r\n{Strings.succeededWritingBootReloader}");

            // Re-enable the form controls
            flashMulti.EnableControls(true);

            return true;
        }

        /// <summary>
        /// Resets the Maple USB Serial device to DFU mode.
        /// </summary>
        /// <param name="flashMulti">An instance of the <see cref="FlashMulti"/> class.</param>
        /// <param name="comPort">The COM port where the Maple USB device can be found.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<bool> ResetToDfuMode(FlashMulti flashMulti, string comPort)
        {
            string command;
            string commandArgs;
            int returnCode = -1;

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
                MessageBox.Show($"{Strings.failedToOpenPort} {comPort}", Strings.dialogTitleRead, MessageBoxButtons.OK, MessageBoxIcon.Error);
                flashMulti.EnableControls(true);
                return false;
            }

            string mapleMode = MapleDevice.FindMaple().Mode;

            if (mapleMode == "USB")
            {
                flashMulti.AppendLog($"{Strings.progressSwitchingToDfuMode}");
                command = ".\\tools\\maple-reset.exe";
                commandArgs = $"{comPort} 2000";
                await Task.Run(() => { returnCode = RunCommand.Run(flashMulti, command, commandArgs); });
                if (returnCode == 0)
                {
                    flashMulti.AppendLog($" {Strings.done}\r\n");
                    flashMulti.AppendVerbose(string.Empty);
                }
                else
                {
                    if (MapleDevice.FindMaple().DfuMode == false)
                    {
                        flashMulti.AppendLog($" {Strings.failed}\r\n");
                        MessageBox.Show(Strings.failedToReadModule, Strings.dialogTitleRead, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        flashMulti.EnableControls(true);
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
