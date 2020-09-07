// -------------------------------------------------------------------------------
// <copyright file="FileUtils.cs" company="Ben Lye">
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
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    /// <summary>
    /// Utilities for firmware files.
    /// </summary>
    internal class FileUtils
    {
        // Multi-bit bitmasks for firmware options
        private const int ModuleTypeMask = 0x3;
        private const int ChannelOrderMask = 0x7C;
        private const int MultiTelemetryTypeMask = 0xC00;

        // Single-bit bitmasks for firmware options
        private const int BootloaderSupportMask = 0x80;
        private const int CheckForBootloaderMask = 0x100;
        private const int InvertTelemetryMask = 0x200;
        private const int MultiStatusMask = 0x400;
        private const int MultiTelemetryMask = 0x800;
        private const int SerialDebugMask = 0x1000;

        /// <summary>
        /// Parses the binary file looking for a string which indicates that the compiled firmware images contains USB support.
        /// The binary firmware file will contain the strings 'Maple' and 'LeafLabs' if it was compiled with USB serial support.
        /// </summary>
        /// <param name="filename">The path to the firmware file.</param>
        /// <returns>A boolean value indicatating whether or not the firmware supports USB serial.</returns>
        internal static bool CheckForUsbSupport(string filename)
        {
            bool usbSupportEnabled = false;

            // Get the file size
            long length = new System.IO.FileInfo(filename).Length;

            // File is too small to contain USB support
            if (length < 8192)
            {
                return usbSupportEnabled;
            }

            // Parse the file and find the first instance of the signature
            byte[] byteBuffer = File.ReadAllBytes(filename);
            string byteBufferAsString = System.Text.Encoding.ASCII.GetString(byteBuffer);

            // Find the signature
            int signatureOffset = byteBufferAsString.IndexOf("multi-");

            // Look for the USB string
            int usbOffset = byteBufferAsString.IndexOf("M\0a\0p\0l\0e\0\u0012\u0003L\0e\0a\0f\0L\0a\0b\0s\0\u0012\u0001");

            // If we found the usb offset before the signature, or we found the USB offset without a signature, USB is enabled
            if ((usbOffset > 0 && signatureOffset > 0 && usbOffset < signatureOffset) || (usbOffset > 0 && signatureOffset == 0))
            {
                usbSupportEnabled = true;
            }

            return usbSupportEnabled;
        }

        /// <summary>
        /// Converts a channel order index to the string representation.
        /// </summary>
        /// <param name="index">Integer representing the channel order.</param>
        /// <returns>A string containing the channel order, e.g. 'AETR'.</returns>
        internal static string GetChannelOrderString(uint index)
        {
            string result = string.Empty;
            switch (index)
            {
                case 0:
                    result = "AETR";
                    break;
                case 1:
                    result = "AERT";
                    break;
                case 2:
                    result = "ARET";
                    break;
                case 3:
                    result = "ARTE";
                    break;
                case 4:
                    result = "ATRE";
                    break;
                case 5:
                    result = "ATER";
                    break;
                case 6:
                    result = "EATR";
                    break;
                case 7:
                    result = "EART";
                    break;
                case 8:
                    result = "ERAT";
                    break;
                case 9:
                    result = "ERTA";
                    break;
                case 10:
                    result = "ETRA";
                    break;
                case 11:
                    result = "ETAR";
                    break;
                case 12:
                    result = "TEAR";
                    break;
                case 13:
                    result = "TERA";
                    break;
                case 14:
                    result = "TREA";
                    break;
                case 15:
                    result = "TRAE";
                    break;
                case 16:
                    result = "TARE";
                    break;
                case 17:
                    result = "TAER";
                    break;
                case 18:
                    result = "RETA";
                    break;
                case 19:
                    result = "REAT";
                    break;
                case 20:
                    result = "RAET";
                    break;
                case 21:
                    result = "RATE";
                    break;
                case 22:
                    result = "RTAE";
                    break;
                case 23:
                    result = "RTEA";
                    break;
            }

            return result;
        }

        /// <summary>
        /// Checks that the compiled firmware will fit on the module.
        /// </summary>
        /// <param name="filename">The path to the firmware file.</param>
        /// <returns>Returns a boolean indicating whehter or not the firmware size is OK.</returns>
        internal static bool CheckFirmwareFileSize(string filename)
        {
            // Get the file size
            long length = new System.IO.FileInfo(filename).Length;

            // Absolute max size is 128KB
            int maxFileSize = 128 * 1024;

            // If the file is larger than the max flash space we don't need any more checks
            if (length > maxFileSize)
            {
                MessageBox.Show("Selected firmware file is too large.", "Firmware File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // If the file is smaller we can check if it has bootloader support and do a more exact check
            FirmwareFile fileDetails = GetFirmwareSignature(filename);
            if (fileDetails == null)
            {
                // Warn that we can't accurately check the file size
                string sizeMessage = $"Firmware file does not have a signature - unable to validate the size accurately.";
                DialogResult sizeWarning = MessageBox.Show(sizeMessage, "Firmware File Size", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (sizeWarning != DialogResult.OK)
                {
                    return false;
                }
            }
            else
            {
                switch (fileDetails.ModuleType)
                {
                    case "AVR":
                        maxFileSize = fileDetails.BootloaderSupport ? 32232 : 32744;
                        break;
                    case "STM32":
                        maxFileSize = fileDetails.BootloaderSupport ? 120832 : 129024;
                        break;
                }
            }

            // Check if the file contains EEPROM data if it is for an STM32
            if (fileDetails.ModuleType == "STM32")
            {
                byte[] eePromData = Stm32EepromUtils.GetEepromDataFromBackup(filename);
                if (eePromData != null && Stm32EepromUtils.FindValidPage(eePromData) >= 0)
                {
                    maxFileSize += 2048;
                }
            }

            Debug.WriteLine($"Selected file is {length / 1024:n0} KB, maximum size is {maxFileSize / 1024:n0} KB.");

            if (length > maxFileSize)
            {
                string sizeMessage = $"Firmware file is too large.\r\n\r\nSelected file is {length / 1024:n0} KB, maximum size is {maxFileSize / 1024:n0} KB.";
                MessageBox.Show(sizeMessage, "Firmware File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Reads and parses the firmware file signature if it is present.
        /// </summary>
        /// <param name="filename">The path to the firmware file.</param>
        /// <returns>A <see cref="FirmwareFile"/> object.</returns>
        internal static FirmwareFile GetFirmwareSignature(string filename)
        {
            string signature = string.Empty;

            // Get the file size
            long length = new System.IO.FileInfo(filename).Length;

            // File is too small to contain a signature
            if (length < 24)
            {
                return null;
            }

            // Parse the file and find the first instance of the signature
            byte[] byteBuffer = File.ReadAllBytes(filename);
            string byteBufferAsString = System.Text.Encoding.ASCII.GetString(byteBuffer);
            int offset = byteBufferAsString.IndexOf("multi-");

            if (offset > 0)
            {
                signature = byteBufferAsString.Substring(offset, 24);
            }

            Debug.WriteLine(signature);

            // Handle firmware signature v1
            Regex regexFirmwareSignature = new Regex(@"^multi-(avr|stm|orx)-([a-z]{5})-(\d{8}$)");
            Match match = regexFirmwareSignature.Match(signature);
            if (match.Success)
            {
                FirmwareFile file = new FirmwareFile
                {
                    Signature = signature,
                    ModuleType = match.Groups[1].Value == "avr" ? "AVR" : match.Groups[1].Value == "stm" ? "STM32" : match.Groups[1].Value == "orx" ? "OrangeRX" : "Unkown",
                    BootloaderSupport = match.Groups[2].Value.Substring(0, 1) == "b" ? true : false,
                    CheckForBootloader = match.Groups[2].Value.Substring(1, 1) == "c" ? true : false,
                    MultiTelemetryType = match.Groups[2].Value.Substring(2, 1) == "t" ? "OpenTX" : match.Groups[2].Value.Substring(2, 1) == "s" ? "erskyTx" : "Undefined",
                    InvertTelemetry = match.Groups[2].Value.Substring(3, 1) == "i" ? true : false,
                    DebugSerial = match.Groups[2].Value.Substring(4, 1) == "d" ? true : false,
                    ChannelOrder = "Unknown",
                    Version = match.Groups[3].Value.Substring(0, 2).TrimStart('0') + "." + match.Groups[3].Value.Substring(2, 2).TrimStart('0') + "." + match.Groups[3].Value.Substring(4, 2).TrimStart('0') + "." + match.Groups[3].Value.Substring(6, 2).TrimStart('0'),
                };
                return file;
            }

            // Handle firmware signature v2
            regexFirmwareSignature = new Regex(@"^multi-x([a-z0-9]{8})-(\d{8}$)");
            match = regexFirmwareSignature.Match(signature);
            if (match.Success)
            {
                try
                {
                    // Get the hex value of the firmware flags from the regex match
                    string flagHexString = "0x" + match.Groups[1].Value;

                    // Convert the hex string to a number
                    uint flagDecimal = Convert.ToUInt32(flagHexString, 16);

                    // Get the module type from the rightmost two bits
                    uint moduleType = flagDecimal & ModuleTypeMask;

                    // Get the channel order from bits 3-7
                    uint channelOrder = (flagDecimal & ChannelOrderMask) >> 2;
                    string channelOrderString = GetChannelOrderString(channelOrder);

                    // Get the version from the regex
                    string versionString = match.Groups[2].Value;

                    // Convert the zero-padded string to a dot-separated version string
                    int.TryParse(versionString.Substring(0, 2), out int versionMajor);
                    int.TryParse(versionString.Substring(2, 2), out int versionMinor);
                    int.TryParse(versionString.Substring(4, 2), out int versionRevision);
                    int.TryParse(versionString.Substring(6, 2), out int versionPatch);
                    string parsedVersion = versionMajor + "." + versionMinor + "." + versionRevision + "." + versionPatch;

                    // Create the firmware file signature and return it
                    FirmwareFile file = new FirmwareFile
                    {
                        Signature = signature,
                        ModuleType = moduleType == 0 ? "AVR" : moduleType == 1 ? "STM32" : moduleType == 3 ? "OrangeRX" : "Unknown",
                        ChannelOrder = channelOrderString,
                        BootloaderSupport = (flagDecimal & BootloaderSupportMask) > 0 ? true : false,
                        CheckForBootloader = (flagDecimal & CheckForBootloaderMask) > 0 ? true : false,
                        InvertTelemetry = (flagDecimal & InvertTelemetryMask) > 0 ? true : false,
                        MultiTelemetryType = ((flagDecimal & MultiTelemetryTypeMask) >> 10) == 2 ? "OpenTX" : ((flagDecimal & MultiTelemetryTypeMask) >> 10) == 1 ? "erskyTx" : "Undefined",
                        DebugSerial = (flagDecimal & SerialDebugMask) > 0 ? true : false,
                        Version = parsedVersion,
                    };
                    return file;
                }
                catch (Exception)
                {
                    // Throw a warning if we fail to parse the signature
                    MessageBox.Show("Unable to read the details from the firmware file - the signature could not be parsed.", "Firmware Signature", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            // Didn't find a signature in either format, return null
            return null;
        }

        /// <summary>
        /// Uses the temporary file read from the module to extract and save a firmware backup.
        /// </summary>
        /// <param name="flashMulti">An instance of the <see cref="FlashMulti"/> class.</param>
        /// <param name="flashFileName">The temporary file containing the module's flash data.</param>
        /// <param name="eepromFileName">The temporary file containing an Atmega328p module's EEPROM data.</param>
        internal static void SaveFirmwareBackup(FlashMulti flashMulti, string flashFileName, string eepromFileName)
        {
            Debug.WriteLine($"Flash backup file:  {flashFileName}");

            // Stop if the backup file isn't found or the type isn't valid
            if (!File.Exists(flashFileName) || flashMulti.BackupModuleType < 1)
            {
                MessageBox.Show("Backup file not found. Please read the MULTI-Module again.", "Save Backup", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Ask the user if they want to include the EEPROM
            DialogResult includeEeprom = DialogResult.Cancel;
            if (flashMulti.BackupModuleType == FlashMulti.Stm32BackupDfuUtil || flashMulti.BackupModuleType == FlashMulti.Stm32BackupStm32Flash)
            {
                // STM32 backup - ask if EEPROM data should be included in the backup
                includeEeprom = MessageBox.Show("Include the EEPROM data in the backup?", "Include EEPROM", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            }
            else if (flashMulti.BackupModuleType == FlashMulti.AtmegaBackup)
            {
                // Atmega328p backup - ask if the EEPROM should be save seperately
                Debug.WriteLine($"EEPROM backup file: {eepromFileName}");
                includeEeprom = MessageBox.Show("Save the EEPROM data?  It will be saved in a separate file.", "Include EEPROM", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            }

            // Stop if the user cancelled
            if (includeEeprom == DialogResult.Cancel)
            {
                return;
            }

            // Get the size of the back up file
            long flashFileSize = new System.IO.FileInfo(flashFileName).Length;
            Debug.WriteLine($"Flash file is {flashFileSize} bytes.");

            // Get the maximum file size for each backup type
            long firmwareSizeMax = 0;
            switch (flashMulti.BackupModuleType)
            {
                case FlashMulti.AtmegaBackup:
                    firmwareSizeMax = 32 * 1024;
                    break;
                case FlashMulti.Stm32BackupDfuUtil:
                    firmwareSizeMax = 120 * 1024;
                    break;
                case FlashMulti.Stm32BackupStm32Flash:
                    firmwareSizeMax = 128 * 1024;
                    break;
            }

            // Stop if the file is bigger than we expect
            if (flashFileSize > firmwareSizeMax)
            {
                MessageBox.Show($"Unexpected backup file size.  File is {flashFileSize} bytes, maximum size is {firmwareSizeMax} bytes.", "Save Backup", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Get the start byte
            long backupStartByte = 0;

            // Skip the first 8KB if the backup includes the bootloader
            if (flashMulti.BackupModuleType == FlashMulti.Stm32BackupStm32Flash && FirmwareContainsBootloader(flashFileName) && flashFileSize == 128 * 1024)
            {
                backupStartByte = 8192;
            }

            // Get the end byte - default to the entire file
            long backupEndByte = flashFileSize;

            // If this is an STM32 backup and the EEPROM is not being included we don't need the last 2KB of the backup
            if (flashMulti.BackupModuleType != FlashMulti.AtmegaBackup && includeEeprom == DialogResult.No)
            {
                // Discard the last 2KB of the flash backup
                backupEndByte = flashFileSize - 2048;
            }

            // Create the file save dialog for the flash backup
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                // Title for the dialog
                saveFileDialog.Title = "Location to save the flash backup";

                // Filter for .bin files
                saveFileDialog.Filter = ".bin File|*.bin";

                // Return if the dialog was cancelled
                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                // Extract the firmware from the backup file
                byte[] firmwareData;

                // Read the last firmware from the binary file
                using (BinaryReader b = new BinaryReader(File.Open(flashFileName, FileMode.Open, FileAccess.Read)))
                {
                    // Length of data to read
                    long byteLength = backupEndByte - backupStartByte;
                    Debug.WriteLine($"Capturing {byteLength} bytes between {backupStartByte} and {backupEndByte}");

                    // Seek to the start position
                    b.BaseStream.Seek(backupStartByte, SeekOrigin.Begin);

                    // Read the firmware data
                    firmwareData = b.ReadBytes((int)byteLength);
                }

                // Save the file
                using (BinaryWriter b = new BinaryWriter(File.Open(saveFileDialog.FileName, FileMode.Create, FileAccess.Write)))
                {
                    // Write the data
                    b.Write(firmwareData);
                }

                flashMulti.AppendLog($"\r\n\r\nMULTI-Module firmware saved successfully");
                flashMulti.AppendVerbose($"Firmware saved to '{saveFileDialog.FileName}'");
            }

            // Save the Atmega328p EEPROM to a separate file
            if (flashMulti.BackupModuleType == FlashMulti.AtmegaBackup && includeEeprom == DialogResult.Yes)
            {
                // Create the file save dialog for the flash backup
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    // Title for the dialog
                    saveFileDialog.Title = "Location to save the EEPROM backup";

                    // Filter for .eep files
                    saveFileDialog.Filter = ".eep File|*.eep";

                    // Return if the dialog was cancelled
                    if (saveFileDialog.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    // Extract the firmware from the backup file
                    byte[] firmwareData;

                    // Read the last firmware from the binary file
                    using (BinaryReader b = new BinaryReader(File.Open(eepromFileName, FileMode.Open, FileAccess.Read)))
                    {
                        // Seek to the start position
                        b.BaseStream.Seek(0, SeekOrigin.Begin);

                        // Read the firmware data
                        firmwareData = b.ReadBytes(1024);
                    }

                    // Save the file
                    using (BinaryWriter b = new BinaryWriter(File.Open(saveFileDialog.FileName, FileMode.Create, FileAccess.Write)))
                    {
                        // Write the data
                        b.Write(firmwareData);
                    }

                    flashMulti.AppendLog($"\r\nMULTI-Module EEPROM saved successfully");
                    flashMulti.AppendVerbose($"EEPROM saved to '{saveFileDialog.FileName}'");
                }
            }
        }

        /// <summary>
        /// Checks the first 32 bytes of a firmware file and compares it to the first 32 bytes of the MULTI-Module bootloader.
        /// </summary>
        /// <param name="filename">Name of the file to check.</param>
        /// <returns>Boolean indicating whether or not the file contains the bootloader.</returns>
        internal static bool FirmwareContainsBootloader(string filename)
        {
            using (BinaryReader b = new BinaryReader(File.Open(filename, FileMode.Open, FileAccess.Read)))
            {
                // Read the first 32 bytes of the file
                byte[] firmwareHeader;
                b.BaseStream.Seek(0, SeekOrigin.Begin);
                firmwareHeader = b.ReadBytes(32);

                // Convert the bytes to a string
                string firmwareHeaderAsString = System.Text.Encoding.ASCII.GetString(firmwareHeader);

                // Compare the read bytes against the known value of the bootloader
                if (firmwareHeaderAsString == "\0P\0 ?\0\0\b9\u0001\0\b9\u0001\0\b9\u0001\0\b9\u0001\0\b9\u0001\0\b\0\0\0\0")
                {
                    Debug.WriteLine($"Backup file contains bootloader");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Backup file does not contain bootloader");
                    return false;
                }
            }
        }

        /// <summary>
        /// Contains information about a firmware file.
        /// </summary>
        internal class FirmwareFile
        {
            /// <summary>
            /// Gets or sets the type of module.
            /// </summary>
            public string ModuleType { get; set; }

            /// <summary>
            /// Gets or sets the channel order the firmware was compiled for.
            /// </summary>
            public string ChannelOrder { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the bootloader is supported.
            /// </summary>
            public bool BootloaderSupport { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the firmware was compiled with CHECK_FOR_BOOTLOADER defined.
            /// </summary>
            public bool CheckForBootloader { get; set; }

            /// <summary>
            /// Gets or sets the type of Multi telemetry the firmware was compiled for.
            /// </summary>
            public string MultiTelemetryType { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the firmware was compiled with INVERT_TELEMETRY defined.
            /// </summary>
            public bool InvertTelemetry { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the firmware was compiled with USB Serial support.
            /// </summary>
            public bool UsbSerial { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the firmware was compiled with DEBUG_SERIAL defined.
            /// </summary>
            public bool DebugSerial { get; set; }

            /// <summary>
            /// Gets or sets a value containing the version string for the firmware.
            /// </summary>
            public string Version { get; set; }

            /// <summary>
            /// Gets or sets a value containing the entire signature string.
            /// </summary>
            public string Signature { get; set; }
        }
    }
}
