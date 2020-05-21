// -------------------------------------------------------------------------------
// <copyright file="AtmegaEepromUtils.cs" company="Ben Lye">
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
    /// Functions for working with the EEPROM of an Atmega328p-based MULTI-Module.
    /// </summary>
    internal class AtmegaEepromUtils
    {
        /// <summary>
        /// EEPROM page size; 1024 bytes on the STM32F103.
        /// </summary>
        private const int EepromSize = 0x400;

        /// <summary>
        /// Extract the EEPROM data from an EEPROM backup file.
        /// </summary>
        /// <param name="filename">The name of the backup file to parse.</param>
        /// <returns>The EEPROM data.</returns>
        internal static byte[] GetEepromDataFromBackup(string filename)
        {
            byte[] eepromData;

            // Get the file size
            long length = new System.IO.FileInfo(filename).Length;

            // File is not the right size to contain EEPROM data
            if (length != 1024)
            {
                return null;
            }

            // Read the binary file
            using (BinaryReader b = new BinaryReader(File.Open(filename, FileMode.Open, FileAccess.Read)))
            {
                eepromData = b.ReadBytes(EepromSize);
            }

            return eepromData;
        }

        /// <summary>
        /// Read a single address from the EEPROM data and retun the value.
        /// </summary>
        /// <param name="address">The virtual address to read.</param>
        /// <param name="eepromData">A byte array containing the EEPROM data.</param>
        /// <returns>The variable value or -1 if the address was not found.</returns>
        internal static int ReadEepromVariable(int address, byte[] eepromData)
        {
            if (address >= 0 && address < EepromSize)
            {
                return eepromData[address];
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Retrieves the Global ID from the EEPROM data.
        /// </summary>
        /// <param name="eepromData">A byte array containing the EEPROM data.</param>
        /// <returns>The Global ID.</returns>
        internal static uint ReadGlobalId(byte[] eepromData)
        {
            uint id = 0;

            // Read the four EEPROM variables containing the Global ID
            for (int i = 3; i >= 0; i--)
            {
                // Get the variable data
                int var = ReadEepromVariable(10 + i, eepromData);
                if (var < 0)
                {
                    return 0;
                }

                // Shift the ID left 8 bits for the next value
                id <<= 8;

                // OR the variables together
                id |= (uint)var;
            }

            return id;
        }
    }
}
