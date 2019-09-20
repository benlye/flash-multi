// -------------------------------------------------------------------------------
// <copyright file="UsbDeviceInfo.cs" company="Ben Lye">
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
    using System.Collections.Generic;
    using System.Management;

    /// <summary>
    /// Class for retrieving information about connected USB devices.
    /// </summary>
    internal class UsbDeviceInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UsbDeviceInfo"/> class.
        /// </summary>
        /// <param name="deviceID">The USB device ID.</param>
        /// <param name="pnpDeviceID">The PNP Device ID.</param>
        /// <param name="description">Description of the device.</param>
        /// <param name="manufacturer">Device manufacturer.</param>
        /// <param name="name">Device name.</param>
        /// <param name="status">Device status.</param>
        public UsbDeviceInfo(string deviceID, string pnpDeviceID, string description, string manufacturer, string name, string status)
        {
            this.DeviceID = deviceID;
            this.PnpDeviceID = pnpDeviceID;
            this.Description = description;
            this.Manufacturer = manufacturer;
            this.Name = name;
            this.Status = status;
        }

        /// <summary>
        /// Gets the device ID.
        /// </summary>
        public string DeviceID { get; private set; }

        /// <summary>
        /// Gets the PnP device ID.
        /// </summary>
        public string PnpDeviceID { get; private set; }

        /// <summary>
        ///  Gets the device description.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets the device manufacturer.
        /// </summary>
        public string Manufacturer { get; private set; }

        /// <summary>
        /// Gets the device name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the device status.
        /// </summary>
        public string Status { get; private set; }

        /// <summary>
        /// Gets a list of devices matching the Maple DeviceID.
        /// </summary>
        /// <returns>Returns a list of <see cref="UsbDeviceInfo"/> devices.</returns>
        public static List<UsbDeviceInfo> GetUSBDevices()
        {
            // Create a list to store the output
            List<UsbDeviceInfo> devices = new List<UsbDeviceInfo>();

            // Use WMI to find all the Maple devices
            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_PnPEntity WHERE DeviceID like '%VID_1EAF&%' OR DeviceID like '%VID_16C0%'"))
            using (collection = searcher.Get())
            {
                foreach (var device in collection)
                {
                    devices.Add(new UsbDeviceInfo(
                    (string)device.GetPropertyValue("DeviceID"),
                    (string)device.GetPropertyValue("PNPDeviceID"),
                    (string)device.GetPropertyValue("Description"),
                    (string)device.GetPropertyValue("Manufacturer"),
                    (string)device.GetPropertyValue("Name"),
                    (string)device.GetPropertyValue("Status")));
                }
            }

            // Return the list of devices
            return devices;
        }
    }
}
