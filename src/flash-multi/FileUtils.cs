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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    /// <summary>
    /// Utilities for firmware files.
    /// </summary>
    internal class FileUtils
    {
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

            Regex regexSerialProgress = new Regex(@"^multi-(avr|stm|orx)-([a-z]{5})-(\d{8}$)");
            Match match = regexSerialProgress.Match(signature);
            if (match.Success)
            {
                FirmwareFile file = new FirmwareFile();
                file.Signature = signature;
                file.ModuleType = match.Groups[1].Value == "avr" ? "AVR" : match.Groups[1].Value == "stm" ? "STM32" : match.Groups[1].Value == "orx" ? "OrangeRX" : "Unkown";
                file.BootloaderSupport = match.Groups[2].Value.Substring(0, 1) == "b" ? true : false;
                file.CheckForBootloader = match.Groups[2].Value.Substring(1, 1) == "c" ? true : false;
                file.MultiTelemetryType = match.Groups[2].Value.Substring(2, 1) == "t" ? "OpenTX" : match.Groups[2].Value.Substring(2, 1) == "s" ? "erskyTx" : "Undefined";
                file.InvertTelemetry = match.Groups[2].Value.Substring(3, 1) == "i" ? true : false;
                file.DebugSerial = match.Groups[2].Value.Substring(4, 1) == "d" ? true : false;
                file.Version = match.Groups[3].Value.Substring(0, 2).TrimStart('0') + "." + match.Groups[3].Value.Substring(2, 2).TrimStart('0') + "." + match.Groups[3].Value.Substring(4, 2).TrimStart('0') + "." + match.Groups[3].Value.Substring(6, 2).TrimStart('0');
                return file;
            }
            else
            {
                return null;
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
