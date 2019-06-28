using System;
using System.Collections.Generic;
using System.Management;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;

namespace flash_multi
{
    class MapleTools
    {
        /*
        public static void ResetMaple(string port)
        {
            Debug.WriteLine(String.Format("Resetting Maple on {0}", port));
            // Create a new SerialPort object with default settings.
            SerialPort serialPort = new SerialPort(port);
            serialPort.BaudRate = 9600;
            serialPort.DataBits = 8;
            serialPort.Parity = Parity.None;
            serialPort.StopBits = StopBits.One;
            serialPort.Encoding = System.Text.Encoding.ASCII;
            //serialPort.Handshake = Handshake.RequestToSend;

            //serialPort.RtsEnable = true;
            serialPort.DtrEnable = false;

            // Open the serial port
            serialPort.Open();
            if (serialPort.IsOpen)
            {
                Debug.WriteLine("Serial port is open");

            } else
            {
                Debug.WriteLine("Serial port is not open.");
            }

            Debug.WriteLine("Setting DTR to true");
            serialPort.DtrEnable = true;

            Debug.WriteLine("Setting DTR to False");
            serialPort.DtrEnable = false;

            Thread.Sleep(50);
            Debug.WriteLine(String.Format("Sending '1EAF' to {0}", port));
            //var bytes = new byte[] { 0x31, 0x45, 0x41, 0x46 };
            //serialPort.Write(bytes,0,4);

            //serialPort.NewLine = "\0";
            serialPort.Write("1EAF");

            Debug.WriteLine("Closing port");
            serialPort.Close();

        }
        */



        public static FindMapleResult FindMaple()
        {
            FindMapleResult result = new FindMapleResult();
            MapleDevice device = new MapleDevice();

            var usbDevices = GetUSBDevices();

            foreach (var usbDevice in usbDevices)
            {
                switch (usbDevice.PnpDeviceID.Substring(0, 21))
                {
                    case "USB\\VID_1EAF&PID_0003":
                        result.MapleFound = true;
                        device.Mode = "DFU";
                        device.DfuMode = true;
                        device.UsbMode = false;
                        device.DeviceId = "USB\\VID_1EAF&PID_0003";
                        break;
                    case "USB\\VID_1EAF&PID_0004":
                        result.MapleFound = true;
                        device.Mode = "USB";
                        device.DfuMode = false;
                        device.UsbMode = true;
                        device.DeviceId = "USB\\VID_1EAF&PID_0004";
                        break;
                    default:
                        break;
                }
            }

            result.Device = device;

            return result;
        }

        public class FindMapleResult
        {
            public bool MapleFound { get; set; }
            public MapleDevice Device { get; set; }
        }

        public class MapleDevice
        {
            public string DeviceId { get; set; }
            public bool DfuMode { get; set; }
            public bool UsbMode { get; set; }
            public string Mode { get; set; }
        }

        static List<USBDeviceInfo> GetUSBDevices()
        {
            List<USBDeviceInfo> devices = new List<USBDeviceInfo>();

            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_PnPEntity WHERE DeviceID like '%VID_1EAF&%'"))
                collection = searcher.Get();

            foreach (var device in collection)
            {
                devices.Add(new USBDeviceInfo(
                (string)device.GetPropertyValue("DeviceID"),
                (string)device.GetPropertyValue("PNPDeviceID"),
                (string)device.GetPropertyValue("Description"),
                (string)device.GetPropertyValue("Manufacturer"),
                (string)device.GetPropertyValue("Name"),
                (string)device.GetPropertyValue("Status")

                ));
            }

            collection.Dispose();
            return devices;
        }
    }

    class USBDeviceInfo
    {
        public USBDeviceInfo(string deviceID, string pnpDeviceID, string description, string manufacturer, string name, string status)
        {
            this.DeviceID = deviceID;
            this.PnpDeviceID = pnpDeviceID;
            this.Description = description;
            this.Manufacturer = manufacturer;
            this.Name = name;
            this.Status = status;
        }
        public string DeviceID { get; private set; }
        public string PnpDeviceID { get; private set; }
        public string Description { get; private set; }
        public string Manufacturer { get; private set; }
        public string Name { get; private set; }
        public string Status { get; private set; }
    }
}
