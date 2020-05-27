# Troubleshooting
Fortunately it is nearly impossible to 'brick' an STM32, so whatever state your Multiprotocol module is in, it should be possible to recover it.

1. [Re-installing the Maple DFU device drivers](#re-installing-the-maple-dfu-device-drivers)
1. [Flashing fails with JP4IN1 in the radio](#flashing-fails-when-flashing-a-jp4in1-in-the-radio)
1. [Module stuck in DFU mode after flashing](#module-stuck-in-dfu-mode-after-flashing-and-cannot-be-re-flashed)

## Re-installing the Maple DFU device drivers
If reading or writing your MULTI-Module fails, and the verbose output is like the example below, you need to replace the Maple DFU device driver with a different one.

```
Filter on vendor = 0x1eaf product = 0x0003
Opening DFU capable USB device... ID 1eaf:0003
Run-time device DFU version 0110
Found DFU: [1eaf:0003] devnum=0, cfg=1, intf=0, alt=2, name="UNDEFINED"
Claiming USB DFU Interface...
Cannot claim interface
```
### Solution
1. Download and extract or install the latest version  of Zadig from https://zadig.akeo.ie/
1. Put the MULTI-Module into DFU mode
   1. Open a **Command Prompt**
   1. Change to the directory where you have installed or extracted Flash Multi
   1. Plug your MULTI-Module in, noting the COM port which appears in Device Manager
   1. Run this command, substituting `COMX` for your COM port
   
      `.\tools\maple-reset.exe COMX`
      
      ![Maple Reset Output](/img/maple-reset.jpg)
      
   1. Device Manager should now show the Maple DFU device
      
      ![Maple DFU  Device](/img/maple-dfu.jpg)
1. Use Zadig to replace the driver
   1. Run **Zadig** from the location where you installed or extracted it
   1. Click **Options** -> **List all Devices**
   1. In the drop-down device list select `Maple 003` or `Maple DFU`, whichever is listed
   1. Set the right-hand driver selector to `libusbK (v3.0.7.0)`
   1. Click the **Replace Driver** button
      
      ![Zadig](/img/zadig.jpg)

## Flashing fails when flashing a JP4IN1 in the radio
If the module is in the radio, make sure that the radio is **switched off**.  The JP4IN1 will only go into 'flashing mode' (i.e. BOOT0 mode) when it is powered up from the USB port.

## Module stuck in DFU mode after flashing and cannot be re-flashed
If your module isn't working, it shows up in Device Manager as **Maple DFU**, and attempting to re-flash it via the native USB port fails with verbose output like this:
```
.\tools\dfu-util.exe -R -a 2 -d 1EAF:0003 -D "C:\Temp\Multi-STM_FTDI_INV_OPENTX-v1.2.1.67.bin"

Lost Device after reset, assuming prod_id was incremented by oneNo DFU capable USB device found
dfu-util - (C) 2007-2008 by OpenMoko Inc.

This program is Free Software and has ABSOLUTELY NO WARRANTY
```

Most likely you have flashed a non-USB enabled firmware over the USB port, making the USB port (and the module) inoperable.

Luckily, the USB bootloader always starts the module up briefly in DFU mode, and we can take advantage of that to re-flash it.

### Solution
1. Download or compile the correct, **USB-enabled**, firmware
1. Open a **Command Prompt** and change to the 'tools' sub-folder in the Flash Multi folder
   
   E.g.:
   `cd "C:\Program Files (x86)\FlashMulti\tools"`

1. Prepare the `dfu-util.exe` flash command, but don't run it yet:

   `dfu-util.exe -R -a 2 -d 1EAF:0003 -D "[firmware_file]"`
   
   E.g.: `dfu-util.exe -R -a 2 -d 1EAF:0003 -D "C:\Temp\Multi-STM_TXFLASH_INV_OPENTX-v1.2.1.67.bin"`

1. Unplug the module
1. Plug the module in and watch the red LED - it blinks very rapidly, then blinks slightly slower, then goes out
1. Unplug the module again
1. Plug the module in and as soon as the red LED is in the second phase, hit **[Enter]** in the Command Prompt to run the flash command

You have about 0.5s to run the `dfu-util.exe` flash command while the red LED is in the second phase.

You may need to try it a few times, but if you can hit enter in the command prompt as soon as the red LED changes to the second phase you will be able to re-flash the module and it will be back to normal.  Once you've got the hang of the timing you'll be able to do it easily.

<p align=center><img src="../img/dfu-recovery.png"></p>

**Note:** `dfu-util.exe` always gives this error at the end:

`error resetting after download: usb_reset: could not reset device, win error: The system cannot find the file specified.`

You can ignore it.
