// -------------------------------------------------------------------------------
// <copyright file="EepromUtils.cs" company="Ben Lye">
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
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    class EepromUtils
    {
        private static int PageSize = 1024;

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

                Debug.WriteLine(BitConverter.ToString(eepromData));

            }

            return eepromData;
        }

        internal static int FindValidPage(byte[] eepromData)
        {
            int pageBase0 = 0;
            int pageBase1 = 1024;
            int eepromErased = 0xffff;
            int eepromValidPage = 0x0000;

            // Get the status page bytes
            byte[] status0Bytes = { eepromData[pageBase0 + 1], eepromData[pageBase0] };
            byte[] status1Bytes = { eepromData[pageBase1 + 1], eepromData[pageBase1] };

            // Convert the bytes to integers
            int status0 = System.Convert.ToInt32(BitConverter.ToString(status0Bytes).Replace("-", string.Empty), 16);
            int status1 = System.Convert.ToInt32(BitConverter.ToString(status1Bytes).Replace("-", string.Empty), 16);

            // Compare the page status values to determine the valid page
            if (status0 == eepromValidPage && status1 == eepromErased)
            {
                return pageBase0;
            }

            if (status0 == eepromErased && status1 == eepromValidPage)
            {
                return pageBase1;
            }

            Debug.WriteLine($"No valid EEPROM page found!");
            return -1;
        }

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

        internal static int ReadGlobalId(byte[] eepromData)
        {
            int id = 0;

            for (int i = 3; i >= 0; i--)
            {
                int var = ReadEepromVariable(10 + i, eepromData);
                Debug.WriteLine(var);
                id <<= 8;
                id |= var;
            }

            return id;
        }
    }
}
