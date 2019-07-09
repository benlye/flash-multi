// -------------------------------------------------------------------------------
// <copyright file="ComPorts.cs" company="Ben Lye">
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
    using System.IO.Ports;
    using System.Linq;
    using System.Management;
    using System.Threading;

    /// <summary>
    /// Class for dealing with COM ports.
    /// </summary>
    internal class ComPorts
    {
        /// <summary>
        /// Enumerates the available COM ports.
        /// </summary>
        /// <returns>Returns an ordered array of COM port names.</returns>
        public static string[] Enumerate()
        {
            // Get the list of COM port names
            List<string> comPorts = SerialPort.GetPortNames().ToList();

            // Sort the list of ports
            comPorts = comPorts.OrderBy(c => c.Length).ThenBy(c => c).ToList();

            // Short pause to give a DFU device time to show up
            Thread.Sleep(50);

            // Check if we there's a Maple device plugged in
            if (MapleDevice.FindMaple().DeviceFound)
            {
                comPorts.Add("DFU Device");
            }

            // Return an array of COM port names
            return comPorts.ToArray();
        }

        /// <summary>
        /// Checks if the COM port can be opened.
        /// </summary>
        /// /// <param name="port">The name of the port to check.</param>
        /// <returns>
        /// Boolean indicating whether the port could be opened.
        /// </returns>
        public static bool CheckPort(string port)
        {
            // Skip the check and return true if the selected port is 'DFU Device'
            if (port == "DFU Device")
            {
                return true;
            }

            bool result = false;

            // Try to open the serial port, catch an exception if we fail
            SerialPort serialPort = new SerialPort(port);
            try
            {
                serialPort.Open();
                if (serialPort.IsOpen)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                Thread.Sleep(50);
                serialPort.Close();
            }

            return result;
        }
    }
}
