# Flash-Multi
<img src=https://img.shields.io/github/downloads/benlye/flash-multi/total.svg> <a href="https://scan.coverity.com/projects/benlye-flash-multi"><img alt="Coverity Scan Build Status" src="https://img.shields.io/coverity/scan/18725.svg"/></a>

Tool for flashing pre-compiled firmware to an STM32-based Multiprotocol TX module.  Firmware upload can be performed using the built-in USB connection or via an external FTDI adapter.

<p align="center">
  <img src="img/flash-multi.jpg">
</p>

# Supported Modules
* Jumper JP4IN1
* iRangeX IRX4, IRX4 Plus, and IRX4 Lite
* Vantac MPM Lite
* 'Banggood' STM32 Multiprotocol TX Modules
* DIY STM32 Multiprotocol TX Modules

# Installation
## Using the Installer
1. Download the latest installer (`flash-multi-[version].exe`) from the [Releases](https://github.com/benlye/flash-multi/releases) page
1. Run the installer to install the application and the Maple USB device drivers - **the Maple USB device drivers must be selected for installation at least once**. They do not need to be installed on subsequent reinstalls or upgrades (but reinstalling them won't do any harm).
1. Launch 'Flash Multi' from the Start menu

## Manual Installation
1. Download the latest .zip archive (`flash-multi-[version].zip`) from the [Releases](https://github.com/benlye/flash-multi/releases) page
1. Unzip the archive to a convenient location
1. Run `\drivers\install_drivers.bat` to manually install the Maple USB device drivers (only needed once)
1. Launch the application by running `flash-multi.exe`

## Drivers for Older Versions of Windows
**Users of Windows 7, XP, etc. will need to install additional drivers manually.**

The driver for the Jumper JP4IN1 module, the Silicon Labs CP210x driver, can be downloaded from [here](https://www.silabs.com/products/development-tools/software/usb-to-uart-bridge-vcp-drivers).  This driver is **not** included with Flash Multi.

Other drivers may be needed if you are using an external FTDI adapter. Consult the documentation for your adapter.

*Windows 10 includes drivers for most common serial devices, including FTDI adapters and the USB-to-Serial chip in the Jumper JP4IN1 Module. Additonal drivers shouldn't need to be installed on Windows 10.*

# Use
**Note for iRangeX, Banggood, and DIY modules:** The first time you flash your module you will need to connect it with an external FTDI adapter in order to flash the bootloader. The bootloader is required in order for the USB port work and it can only be written with an FTDI adapter.

**Note for external FTDI connections:** When using an external FTDI adapter the `BOOT0` pin on the board must be connected to 5V, usually by installing a jumper on the `BOOT0` header pins.

1. If the module is installed in the radio, ensure the radio is powered **off**
1. Connect your module to the computer using the USB port or via an external FTDI adapter, as appropriate.  Note the COM port which appears when the device is connected.
1. Launch Flash Multi
1. Click the **Browse** button and locate a compiled firmware file
1. Select the appropriate COM port
1. Select whether or not to write the bootloader - see [Writing the Bootloader](#writing-the-bootloader) below
1. Click the **Upload** button

## Writing the Bootloader
When flashing via serial using either an exterral or internal FTDI adapter you must choose whether or not to write the bootloader.

The bootloader does two things:
1. Enables the native USB port on devices which have it
1. Enables updating the Multiprotocol module's firmware from the radio (on radios which support this)

The **Write Bootloader** option must match the `CHECK_FOR_BOOTLOADER` option in the firmware otherwise the module will not work. 

For pre-compiled firmware from the Github Release page:
* If the file name contains `_TXFLASH_` then **Yes** will be selected automatically.
* If the file name contains `_FTDI_` then **No** will be selected automatically.

For self-compiled firmware:
* If `CHECK_FOR_BOOTLOADER` is enabled (uncommented) in your config you must manually select **Yes**.
* If `CHECK_FOR_BOOTLOADER` is disabled (commented) in your config you must manually  select **No**.

For native USB uploads:
* If a Maple USB device is detected the **Write Bootloader** controls will be disabled and **Yes** will be selected as the bootloader is already installed, and cannot be changed via a native USB upload.

## Upload Output
The output will vary depending on the type of module being flashed.

Modules connected via an external FTDI adapter, and the Jumper JP4IN1 module (which has an *internal* FTDI adapter behind the USB port) will see output like this:
```
Starting Multimodule update
[1/3] Erasing flash memory... done
[2/3] Writing bootloader... done
[3/3] Writing Multimodule firmware... done

Multimodule updated sucessfully
```

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

# More Information
Much more information about flashing a Multiprotocol module, including how to connect an FTDI adapter and connect the `BOOT0` pin, can be found in the documentation for the DIY Multiprotocol Module at https://github.com/pascallanger/DIY-Multiprotocol-TX-Module/blob/master/README.md.

# Warranty
Flash Multi is free software and comes with ABSOLUTELY NO WARRANTY.
