// -------------------------------------------------------------------------------
// <copyright file="FileUtils.cs" company="Ben Lye">
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
    using System.IO;
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
        /// The binary firmware file will contain the strings 'Maple' and 'LeafLabs' if it was compiled with support for the USB / Flash from TX bootloader.
        /// </summary>
        /// <param name="filename">The path to the firmware file.</param>
        /// <returns>A boolean value indicatating whether or not the firmware supports USB.</returns>
        internal static bool CheckForUsbSupport(string filename)
        {
            bool usbSupportEnabled = false;

            byte[] byteBuffer = File.ReadAllBytes(filename);
            string byteBufferAsString = System.Text.Encoding.ASCII.GetString(byteBuffer);
            int offset = byteBufferAsString.IndexOf("M\0a\0p\0l\0e\0\u0012\u0003L\0e\0a\0f\0L\0a\0b\0s\0\u0012\u0001");

            if (offset > 0)
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

            // If the file is very large we don't want to check for USB support so throw a generic error
            if (length > 256000)
            {
                MessageBox.Show("Selected firmware file is too large.", "Firmware File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // If the file is smaller we can check if it has USB support and throw a more specific error
            int maxFileSize = CheckForUsbSupport(filename) ? 120832 : 129024;

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

            // Read the last 24 bytes of the binary file so we can see if it contains a signature string
            using (var reader = new StreamReader(filename))
            {
                if (reader.BaseStream.Length > 24)
                {
                    reader.BaseStream.Seek(-24, SeekOrigin.End);
                }

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    signature = line;
                }
            }

            // Parse the entire file if we didn't find the signature in the last 24 bytes
            if (signature != string.Empty && signature.Substring(0, 6) != "multi-")
            {
                byte[] byteBuffer = File.ReadAllBytes(filename);
                string byteBufferAsString = System.Text.Encoding.ASCII.GetString(byteBuffer);
                int offset = byteBufferAsString.IndexOf("multi-");

                if (offset > 0)
                {
                    signature = byteBufferAsString.Substring(offset, 24);
                }
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

                    // Create the firmware file signatre and return it
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
                catch (Exception ex)
                {
                    // Throw a warning if we fail to parse the signature
                    MessageBox.Show("Unable to read the details from the firmware file - the signature could not be parsed.", "Firmware Signature", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            // Didn't find a signature in either format, return null
            return null;
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
