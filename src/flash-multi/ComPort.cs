// -------------------------------------------------------------------------------
// <copyright file="ComPort.cs" company="Ben Lye">
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
    internal class ComPort
    {
        /// <summary>
        /// Delay in ms to wait for a Maple device to go from DFU to USB mode.
        /// Prevents a DFU device appearing momentarily before the Maple USB device.
        /// </summary>
        private const int DfuDelay = 0;

        /// <summary>
        /// Gets the name of the port.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the description of the port.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets the displayname of the port (name + description).
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Enumerates the available COM ports.
        /// </summary>
        /// <returns>Returns an ordered array of COM port names.</returns>
        public static string[] EnumeratePorts()
        {
            // Get the list of COM port names
            List<string> comPorts = SerialPort.GetPortNames().ToList();

            // Sort the list of ports
            comPorts = comPorts.OrderBy(c => c.Length).ThenBy(c => c).ToList();

            // Short pause to give a DFU device time to finish showing up
            Thread.Sleep(DfuDelay);

            // Check if we there's a Maple device plugged in
            if (MapleDevice.FindMaple().DeviceFound)
            {
                comPorts.Add("DFU Device");
            }

            // Return an array of COM port names
            return comPorts.ToArray();
        }

        /// <summary>
        /// Enumerates the available COM ports without using WMI.
        /// </summary>
        /// <returns>Returns an ordered list of ports <see cref="ComPort"/>.</returns>
        public static List<ComPort> EnumeratePortList()
        {
            DateTime start = DateTime.Now;
            Debug.WriteLine("Enumerating COM ports");

            List<ComPort> comPorts = new List<ComPort>();

            // Get all the COM ports
            string[] comPortNames = SerialPort.GetPortNames();

            // Add all available to the list
            foreach (string portName in comPortNames)
            {
                ComPort thisPort = new ComPort
                {
                    Name = portName,
                    Description = portName,
                    DisplayName = portName,
                };
                comPorts.Add(thisPort);
            }

            // Sort the list of ports
            comPorts = comPorts.OrderBy(c => c.Name.Length).ThenBy(c => c.Name).ToList();

            // Short pause to give a DFU device time to finish showing up
            Thread.Sleep(DfuDelay);

            // Check if we there's a Maple device in DFU mode plugged in
            if (MapleDevice.FindMaple().DfuMode)
            {
                ComPort dfuPort = new ComPort
                {
                    Name = "DFU Device",
                    Description = "DFU Device",
                    DisplayName = "DFU Device",
                };
                comPorts.Add(dfuPort);
            }

            DateTime end = DateTime.Now;
            Debug.WriteLine($"COM ports enumerated in {end - start}.");

            // Return a list of COM ports
            return comPorts;
        }

        /// <summary>
        /// Enumerates the available COM ports using WMI, including the device description.
        /// </summary>
        /// <returns>Returns an ordered list of ports <see cref="ComPort"/>.</returns>
        public static List<ComPort> EnumeratePortList2()
        {
            List<ComPort> comPorts = new List<ComPort>();

            // Get all the COM ports using WMI
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                "root\\cimv2",
                "SELECT * FROM Win32_PnPEntity WHERE ClassGuid=\"{4d36e978-e325-11ce-bfc1-08002be10318}\""))
            {
                // Add all available ports to the list
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    string portCaption = queryObj["Caption"].ToString();

                    if (portCaption.Contains("(COM"))
                    {
                        // Get the index number where "(COM" starts in the string
                        int indexOfCom = portCaption.IndexOf("(COM");
                        string portName = portCaption.Substring(indexOfCom + 1, portCaption.Length - indexOfCom - 2);
                        string portDescription = portCaption.Substring(0, indexOfCom - 1);

                        ComPort thisPort = new ComPort
                        {
                            Name = portName,
                            Description = portDescription,
                            DisplayName = $"{portName} ({portDescription})",
                        };

                        comPorts.Add(thisPort);
                    }
                }
            }

            // Sort the list of ports
            comPorts = comPorts.OrderBy(c => c.Name.Length).ThenBy(c => c.Name).ToList();

            // Short pause to give a DFU device time to finish showing up
            Thread.Sleep(DfuDelay);

            // Check if we there's a Maple device in DFU mode plugged in
            if (MapleDevice.FindMaple().DfuMode)
            {
                ComPort dfuPort = new ComPort
                {
                    Name = "DFU Device",
                    Description = "Maple DFU Device",
                    DisplayName = "DFU Device",
                };
                comPorts.Add(dfuPort);
            }

            // Return a list of COM ports
            return comPorts;
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
