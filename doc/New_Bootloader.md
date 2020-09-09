# New MULTI-Module Bootloader

If you have:
* A radio with an internal MULTI-Module (e.g. T16, T18, TX16S), or
* A Jumper JP4IN1-SE or RadioBoss JP4IN1 MULTI-Module, or
* Any other MULTI-Module which is identified as a CP2102 device when you plug it, or
* Any Atmega32p-based MULTI-Module

You can stop reading now - this does not matter to you.

**If you have a Jumper JP4IN1, iRangeX IRX4, Banggood, or any other STM32 MULTI-Module, please read this information - if you use the USB port on your module, you will need to take action!**

If you want to cut to the chase, skip to the [what you need to do](#what-you-need-to-do) section.

## Introduction
The latest version of Flash Multi, v0.5.0, gives you the ability to flash a new bootloader to your MULTI-Module.  

The new bootloader changes the behaviour of the USB port on your MULTI-module when you plug the module into a computer.

| Action | Old Bootloader (COM Port Mode) | New Bootloader (Sticky DFU Mode) |
| --- | --- | --- |
| Connect the module to the computer, radio is off | Module starts in DFU mode then switches to COM port mode | Module starts in DFU mode and stays in DFU mode |
| Connect the module to the computer, radio is on | Computer will detect a USB COM port | Computer will detect an unidentified device |

## Reason for Change
Historically, the MULTI-Module firmware has always included code to make the USB COM port work when the module is running.  However, the USB COM port is only used in two ways:
* Switching the module to firmware update (DFU) mode when it is being flashed by using Flash Multi or the Arduino IDE
* Debug output when running a debug-enabled build (typically only used by developers)

And, there is never need for an internal MULTI-Module to have the USB COM port code, but it was always included.  When this decision was made the firmware was around 80KB, and we had plenty of free space in the 128KB available flash on the STM32 MCU.

Fast-forward three years and we have all but run out of space in the STM32.  In an effort to free some space up, we are removing USB support from the firmware.  **This change saves 5KB of flash.**  This may not seem like much, but it's room for several more protocols.

**Firmware without USB support will start to be released later in 2020.**

Because we're removing the mechanism that would allow flashing tools to put the module into DFU mode, we have to provide a different way to do that.  

This is where the new bootloader comes in - it keeps the module in DFU mode when it is plugged in via USB, removing the need to use the COM port to switch it into DFU mode.

## What you need to do
**You will need to use Flash Multi to upgrade the bootloader on your MULTI-Module _before_ you flash firmware which does not have USB support.**

### Upgrading to the new bootloader
There is a one-time process to update the bootloader on the module.  After the bootloader upgrade you will need to flash new firmware to your module.

1. Launch Flash Multi
1. Tell Flash Multi to use the new bootloader:
   1. Click **Advanced** -> **Settings** -> **USB Port Mode** -> **Sticky DFU Mode (New)**
1. Plug your module in
1. Ensure that the correct COM port is selected
1. Flash the new bootloader to your module:
   1. Click **Advanced** -> **Actions** -> **Flash Module Bootloader**
   1. Wait for the red LED to go out
   1. Unplug the module

**You must pay attention to the instructions.  DO NOT unplug the module until the red LED has gone out for at least two seconds.**

You will now need to write new MULTI-Module firmware to the module in the normal way:
1. Check that the COM port is set to **DFU Device**
1. Select the firmware file
1. Click **Write Module**

You will get a warning reminding you that you needed to update the bootloader, you may check the box to stop the message showing again.

Once you have written new firmware to your module, if you unplug it and plug it back in, the module should stay in DFU mode with the red LED blinking continuously.

## Frequently Asked Questions
## When should I update the bootloader?
You can do it any time after the release of Flash Multi v0.5.0.  The new bootloader works with all previous MULTI-Module firmware releases.

### If I only ever flash my MULTI-Module from the radio do I have to do the bootloader update?
No, you don't have to do it.  You can also wait and do it later if you want to.

### How do I know which bootloader my MULTI-Module has?
Plug it in via USB
* If the red LED blinks rapidly then starts to blink slowly and the green LED comes on you have the old bootloader
* If the red LED blinks rapidly continuously you have the new bootloader
* If the green LED comes on and the red LED stays off you have a module which does not use the bootloader for the USB port

### I flashed new firmware without USB support from the radio and now my USB port doesn't work, how do I fix it?
You have the old bootloader.  Use Flash Multi to flash the module bootloader, as explained above.  You will have to unplug and replug the module when instructed.

### I have to unplug and re-plug my module every time I flash it, how do I fix it?
You have the old bootloader.  Use Flash Multi to flash the module bootloader, as explained above.  You will have to unplug and replug the module when instructed.

### Why doesn't this apply to JP4IN1-SE or Radioboss modules?
They use a different USB interface which does not use DFU mode to flash firmware.  They will work happily with the old or new bootloaders and do not require updating this way.
