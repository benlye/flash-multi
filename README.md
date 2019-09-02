# Flash-Multi
<img src=https://img.shields.io/github/downloads/benlye/flash-multi/total.svg> <a href="https://scan.coverity.com/projects/benlye-flash-multi"><img alt="Coverity Scan Build Status" src="https://img.shields.io/coverity/scan/18725.svg"/></a>

Tool for flashing pre-compiled firmware to a Jumper, iRangeX, Vantac, 'Banggood', or DIY STM32-based Multiprotocol TX module.  A GUI application is available for Windows and a shell script for Linux.

Firmware upload can be performed using the built-in USB connection or via an external USB-to-serial adapter.

Just three steps are required to update your Jumper, iRangeX, or DIY Multiprotocol TX module (full instructions are below):
1. [Download and install](#installing-flash-multi) Flash Multi and any required [device drivers](#additional-drivers)
1. Download the appropriate [Multiprotocol firmware file](https://github.com/pascallanger/DIY-Multiprotocol-TX-Module/releases)
1. [Flash](#using-flash-multi) the new firmware to your module

<p align="center">
  <img src="img/flash-multi.jpg">
</p>

# Documentation Index
1. [Supported Modules](#supported-modules)
1. [Flash Multi Requirements](#flash-multi-requirements)
1. [Installing Flash Multi](#installing-flash-multi)
   1. [Linux](#linux)
   1. [Using the Installer](#using-the-installer)
   1. [Manual Installation](#manual-installation)
   1. [Additional Drivers](#additional-drivers)
1. [Using Flash Multi](#using-flash-multi)
   1. [Writing the Bootloader](#writing-the-bootloader)
   1. [Upload Output](#upload-output)
1. [Multiprotocol Firmware](#multiprotocol-firmware)
1. [Troubleshooting](#troubleshooting)
1. [More Information](#more-information)
1. [Warranty](#warranty)

# Supported Modules
All Multiprotocol modules based on the STM32 microcontroller are supported.  Modules based on the ATmega328 microcontoller are _not_ supported.

Supported modules include:
* Jumper JP4IN1
* iRangeX IRX4, IRX4 Plus, and IRX4 Lite
* Vantac MPM Lite
* 'Banggood' STM32 Multiprotocol TX Modules
* DIY STM32 Multiprotocol TX Modules

# Flash Multi Requirements
Flash Multi requires .Net Framework 4.5 or newer.  You probably already have a compatible version installed, but if not, the latest version can be downloaded from (Microsoft)[https://dotnet.microsoft.com/download/dotnet-framework].  
> _Tip: When you launch Flash Multi it will tell you if you need to install or upgrade your .Net Framework._

# Installing Flash Multi
## Linux
Please refer to the [Linux instructions](doc/Linux.md).

## Using the Installer
**Note:** Make sure your Multiprotocol module is not plugged in when you run the Flash Multi installer, or when you run the manual Maple USB driver installer.  This is to avoid potential problems with the driver installation.

1. Download the latest installer (`flash-multi-[version].exe`) from the [Releases](https://github.com/benlye/flash-multi/releases) page
1. Run the installer to install the application and the Maple USB device drivers - **the Maple USB device drivers must be selected for installation at least once**. They do not need to be installed on subsequent reinstalls or upgrades (but reinstalling them won't do any harm).
1. Launch 'Flash Multi' from the Start menu

## Manual Installation
1. Download the latest .zip archive (`flash-multi-[version].zip`) from the [Releases](https://github.com/benlye/flash-multi/releases) page
1. Unzip the archive to a convenient location
1. Run `\drivers\install_drivers.bat` to manually install the Maple USB device drivers (only needed once)
1. Launch the application by running `flash-multi.exe`

## Additional Drivers
**Depending on your Multiprotocol module and version of Windows you may need to install additional drivers manually.**

The driver for the Jumper JP4IN1 module, the Silicon Labs CP210x driver, is **not** included with Flash Multi and can be downloaded from here:
https://www.silabs.com/products/development-tools/software/usb-to-uart-bridge-vcp-drivers

Other drivers may be needed if you are using an external USB-to-serial adapter. Consult the documentation for your adapter.

*Windows 10 includes drivers for many common serial devices, including many USB-to-serial adapters, so check Device Manager to see if your device is recognised.*

# Using Flash Multi
**Note:** The first time you flash your module you may need to connect it with an external USB-to-serial adapter in order to flash the bootloader. The bootloader is required in order for the native USB port work, and it can only be written with a USB-to-serial adapter.  This does not apply to newer Jumper 4IN1 modules, which have an _internal_ USB-to-serial adapter.

**Note for external USB-to-serial connections:** When using an external USB-to-serial adapter, the `BOOT0` pin on the board must be connected to 3.3V, usually by installing a jumper on the `BOOT0` header pins.

1. If the module is installed in the radio and you are connecting to the module's USB port, ensure the radio is powered **off**
1. Launch **Flash Multi**
1. Connect your module to the computer using the module's USB port or via an external USB-to-serial adapter, as appropriate
   > _Tip: Flash Multi will automatically select the new COM port, if it's running when the module is plugged in._
1. Click the **Browse** button and select a compiled firmware file
1. If it wasn't automatically selected, select the appropriate COM port
1. Click the **Upload** button

## Writing the Bootloader
The bootloader enables flashing the Multiprotocol module firmware from a radio which supports this (see [here](https://github.com/pascallanger/DIY-Multiprotocol-TX-Module/blob/master/docs/Flash_from_Tx.md)).  It also enables the native USB port on Multiprotocol modules which have one, facilitating firmware updates via native USB rather than an external USB-to-serial adapter.

When flashing with a USB-to-serial adapter the bootloader will be written automatically if the selected firmware file was compiled with support for it.

When flashing via the native USB port the firmware being flashed _must be compiled with support for the bootloader_ otherwise, to avoid rendering the module inoperable, Flash Multi will display an error and stop the flash attempt.

## Upload Output
The output will vary depending on the type of module being flashed.

Modules connected via an external USB-to-serial adapter, and the Jumper JP4IN1 module (which has an *internal* USB-to-serial adapter behind the USB port) will see output like this:
```
Starting Multimodule update
[1/3] Erasing flash memory... done
[2/3] Writing bootloader... done
[3/3] Writing Multimodule firmware... done

Multimodule updated sucessfully
```

> _Note: Step 2, writing the bootloader, is not performed when writing firmware which was not compiled with support for the native USB / Flash from TX bootloader._

Modules connected via USB (except the Jumper JP4IN1) will see output like this:
```
Maple device found in USB mode
Starting Multimodule update
Switching Multimodule into DFU mode ... done
Waiting for DFU device ... got it
Writing firmware to Multimodule ... done

Multimodule updated sucessfully
```

For both methods, if the 'Show Verbose Output' box is checked the actual output from each of the flash proceses will be shown. If the flash fails for any reason the verbose messages are a good place to look for more details.

# Multiprotocol Firmware
Pre-compiled Multiprotocol firmware can be downloaded from the Multiprotocol TX Module [Releases](https://github.com/pascallanger/DIY-Multiprotocol-TX-Module/releases).

The Multiprotocol firmware can also be [compiled from source](https://github.com/pascallanger/DIY-Multiprotocol-TX-Module/blob/master/docs/Compiling_STM32.md) relatively easily.

# Troubleshooting
See the dedicated [Troubleshooting page](doc/Troubleshooting.md).

# More Information
Much more information about flashing a Multiprotocol module, including how to connect a USB-to-serial adapter and connect the `BOOT0` pin, can be found in the documentation for the DIY Multiprotocol Module at https://github.com/pascallanger/DIY-Multiprotocol-TX-Module/blob/master/README.md.

# Warranty
Flash Multi is free software and comes with ABSOLUTELY NO WARRANTY.
