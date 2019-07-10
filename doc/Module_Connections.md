# Connecting Your Multiprotocol Module
There are three ways that a Multiprotocol module can be connected to a PC and knowing which you can (or should) use can be confusing.  The purpose of this page is to help you to identify and use your device's connection(s).

The three connection types are:
* Native USB port
* External USB-to-Serial Adapter
* Internal USB-to-Serial Adapter

The connections available to you will depend on the specific module you have. 

**Note:** 'FTDI' is commonly used as a synonym for USB-to-Serial, but is in fact one specific chip used in USB-to-Serial adapters.  For the purpose of the Flash Multi documentation 'FTDI Adapter' means 'USB-to Serial Adapter' anywhere it may be used.

## Native USB Port
The 'native' USB port (also known as the Maple port) is a USB port which interfaces directly with the STM32 micro-controller in the Multiprotocol module.  

Using the native port requires that, at a minimum, the USB bootloader has been flashed to the module.

**Note:** Multiprotocol module manufacturers only recently started shipping their modules with the USB bootloader installed.  You may have a module which has a native USB port, but does not have the bootloader installed.

_**Tip:** Using the native USB port is the fastest way to flash your Multiprotocol module._

### Drivers
The native USB port requires two drivers, Maple Serial and Maple DFU, both of which are included with Flash Multi.

### Modes
The native USB port has two modes, USB and DFU.  DFU, or Device Firmware Update, mode is used when flashing the module. In USB mode the module appears as a serial (COM) port.

Assuming the PC has the drivers installed, and the module has the correct (USB-enabled) firmware installed, when the module is plugged in it will briefly appear as a Maple DFU device and then reappear as a Maple Serial device (COM port).

Flash Multi will automatically switch the module into DFU mode during the firmware flash process.

### How it Appears
The native USB device has a device ID of `VID_1EAF&PID_0003` in DFU mode or `VID_1EAF&PID_0004` in USB mode.  It can appear differently in Device Manager, depending on what firmware, bootloader, and drivers are installed.

#### No Drivers Installed

#### No Bootloader

#### Drivers and Firmware

#### Drivers but no firmware

## External USB-to-Serial Adapter
An _external_ USB-to-Serial adapter is a small USB device which provides a USB-to-serial interface.  The adapter is used to connect to the serial pins on the STM32 micro-controller.  Common USB-to-Serial adapters use well-known chips such as `FTDI`, `FT232RL`, `CH340`, `CP2102`.

Serial support is built into the STM32 and can _always_ be used to program the device.  An external USB-to-Serial adapter is a useful tool to have as it will ensure you can always recover your Multiprotocol module from a bad firmware flash, should one occur.

Using an external USB-to-Serial adapter to flash the module requires connecting a special pin on the board called `BOOT0` to 5V.  There is usually a header, or connections for a header, for this on the board and the connection is made by placing a jumper on two pins to make the connection.

### Drivers
Windows 10 includes drivers for many USB-to-Serial adapters, but not all.  Older version of Windows will typically need drivers installed manually.  You should refer to your USB-to-Serial adapter's instructions.

### How it Appears
With the correct drivers installed, your USB-to-Serial adapter should appear as a serial (COM) port in Device Manager.

## Internal USB-to-Serial Adapter
The _internal_ USB-to-Serial adapter is (at this time) unique to newer Jumper JP4IN1 modules, such as the module shipped with the Jumper T16.

The _internal_ USB-to-Serial adapter in the JP4IN1 is functionally the same as an _external_ USB-to-Serial adapter except that it does not require manually connecting the `BOOT0` pin, because `BOOT0` is hard-wired to the USB port.

### Drivers
The _internal_ USB-to-Serial adapter in the JP4IN1 requires the Silicon Labs CP210x driver to be manually installed on all versions of Windows.  The driver can be downloaded from https://www.silabs.com/products/development-tools/software/usb-to-uart-bridge-vcp-drivers.

### How it Appears
With the CP210x driver installed, the device will appear as a serial (COM) port in Device Manager.
