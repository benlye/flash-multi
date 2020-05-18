// -------------------------------------------------------------------------------
// <copyright file="EepromUtils.cs" company="Ben Lye">
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

    /// <summary>
    /// Functions for working with the emulated EEPROM of the STM32F103.
    /// </summary>
    internal class EepromUtils
    {
        /// <summary>
        /// EEPROM page size; 1024 bytes on the STM32F103.
        /// </summary>
        private const int PageSize = 0x400;

        /// <summary>
        /// Base address of EEPROM page 0.
        /// </summary>
        private const int PageBase0 = 0;

        /// <summary>
        /// Base address of EEPROM page 1.
        /// </summary>
        private const int PageBase1 = 0x400;

        /// <summary>
        /// Page header of an erased EEPROM page.
        /// </summary>
        private const int EepromPageErased = 0xFFFF;

        /// <summary>
        /// Page header of a valid EEPROM page.
        /// </summary>
        private const int EepromPageValid = 0x0000;

        /// <summary>
        /// Extract the EEPROM data from a backup file.
        /// The EEPROM data is the last 2048 bytes of the file.
        /// </summary>
        /// <param name="filename">The name of the backup file to parse.</param>
        /// <returns>The EEPROM data.</returns>
        internal static byte[] GetEepromDataFromBackup(string filename)
        {
            byte[] eepromData;

            // Read the last 2048 bytes of the binary file
            using (BinaryReader b = new BinaryReader(File.Open(filename, FileMode.Open, FileAccess.Read)))
            {
                long offset = new System.IO.FileInfo(filename).Length - 2048;

                // Seek to our required position.
                b.BaseStream.Seek(offset, SeekOrigin.Begin);

                // Read the next 2048 bytes.
                eepromData = b.ReadBytes(2048);
            }

            return eepromData;
        }

        /// <summary>
        /// Find the valid page in the EEPROM data.
        /// </summary>
        /// <param name="eepromData">A byte array containing the EEPROM data.</param>
        /// <returns>The start address of the valid page.</returns>
        internal static int FindValidPage(byte[] eepromData)
        {
            // Get the status page bytes
            byte[] status0Bytes = { eepromData[PageBase0 + 1], eepromData[PageBase0] };
            byte[] status1Bytes = { eepromData[PageBase1 + 1], eepromData[PageBase1] };

            // Convert the bytes to integers
            int status0 = System.Convert.ToInt32(BitConverter.ToString(status0Bytes).Replace("-", string.Empty), 16);
            int status1 = System.Convert.ToInt32(BitConverter.ToString(status1Bytes).Replace("-", string.Empty), 16);

            // Compare the page status values to determine the valid page
            if (status0 == EepromPageValid && status1 == EepromPageErased)
            {
                return PageBase0;
            }

            if (status0 == EepromPageErased && status1 == EepromPageValid)
            {
                return PageBase1;
            }

            Debug.WriteLine($"No valid EEPROM page found!");
            return -1;
        }

        /// <summary>
        /// Read a single address from the EEPROM data and retun the value.
        /// </summary>
        /// <param name="address">The virtual address to read.</param>
        /// <param name="eepromData">A byte array containing the EEPROM data.</param>
        /// <returns>The variable value or -1 if the address was not found.</returns>
        internal static int ReadEepromVariable(int address, byte[] eepromData)
        {
            int pageBase = FindValidPage(eepromData);

            if (pageBase < 0)
            {
                return -1;
            }

            int pageEnd = pageBase + PageSize - 2;

            for (pageBase += 6; pageEnd >= pageBase; pageEnd -= 4)
            {
                byte[] addressBytes = { eepromData[pageEnd + 1], eepromData[pageEnd] };
                int addressValue = System.Convert.ToInt32(BitConverter.ToString(addressBytes).Replace("-", string.Empty), 16);

                if (addressValue == address)
                {
                    byte[] dataBytes = { eepromData[pageEnd - 1], eepromData[pageEnd - 2] };
                    int dataValue = System.Convert.ToInt32(BitConverter.ToString(dataBytes).Replace("-", string.Empty), 16);
                    return dataValue;
                }
            }

            return -1;
        }

        /// <summary>
        /// Retrieves the Global ID from the EEPROM data.
        /// </summary>
        /// <param name="eepromData">A byte array containing the EEPROM data.</param>
        /// <returns>The Global ID.</returns>
        internal static int ReadGlobalId(byte[] eepromData)
        {
            int id = 0;

            // Read the four EEPROM variables containing the Global ID
            for (int i = 3; i >= 0; i--)
            {
                // Get the variable data
                int var = ReadEepromVariable(10 + i, eepromData);

                // Stored variable is 16 bits but we only need 8
                id <<= 8;

                // OR the variables together
                id |= var;
            }

            return id;
        }
    }
}
