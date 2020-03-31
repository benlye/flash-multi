# Flashing From Linux or Mac
A bash script and toolchain is provided for flashing Multiprotocol modules from Linux and Mac operating systems.  

Like the Flash Multi Window application, the Linux and Mac script will automatically determine which upload method to use, will preserve the EEPROM data during flashes, and will write the bootloader as needed.

## Installing
1. Download and extract the latest Linux release, for example:

   ```
   wget https://github.com/benlye/flash-multi/releases/download/0.2.2/flash-multi-0.2.2.tar.gz
   tar -xvzf flash-multi-0.2.2.tar.gz
   ```
   
1. Change to the flash-multi directory, for example:

   `cd flash-multi-0.2.2`
   
1. Configure serial device permissions by running `tools/install.sh` as root to update the udev rules and add the current user to the dialout group, e.g.:
   
   `sudo ./tools/install.sh`

If you prefer you can configure the permissions manually, for example:
```
sudo cp -v ./tools/45-maple.rules /etc/udev/rules.d/45-maple.rules
sudo chown root:root /etc/udev/rules.d/45-maple.rules
sudo chmod 644 /etc/udev/rules.d/45-maple.rules
sudo udevadm control --reload-rules
sudo usermod -a -G plugdev $USER
sudo usermod -a -G dialout $USER
```
### Mac
macOS requires the libusb library to be installed. The easiest way to install the library is using the Homebrew package manager for macOS by running the two commands below in a Terminal.

1. Install Homebrew

   `/usr/bin/ruby -e "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/master/install)"`

1. Use Homebrew to install libusb

   `brew install libusb`

## Use
1. Download the latest pre-compiled firmware from https://github.com/pascallanger/DIY-Multiprotocol-TX-Module/releases
1. Plug in your Multiprotocol module
1. Run the flash-multi shell script, specifiying the serial device and firmware file:

   `./flash-multi -f [firmware file] -p [serial device]`

   For example:
   
   `./flash-multi -f /tmp/multi-stm-opentx-aetr-inv-v1.2.1.85.bin -p /dev/ttyUSB0 `
   
   *Note: On OSX the device might be `/dev/tty.SLAB_USBtoUART`
   
For firmware compiled from recent source code the ouput will look similar to what is shown below, firmware from older source will not display information about the firmware configuration:

```
ben@ben-ubuntu:~/flash-multi-0.2.2$ ./flash-multi -f /tmp/multi-stm-opentx-aetr-inv-v1.2.1.85.bin -p /dev/ttyUSB0
flash-multi 0.2.2

This program is Free Sofware and has NO WARRANTY.
https://github.com/benlye/flash-multi/

Multi Firmware Version:    1.2.1.85 (STM32)
Expected Channel Order:    AETR
Multi Telemetry Type:      OpenTX
Invert Telemetry Enabled:  True
Flash From Radio Enabled:  True
Bootloader Enabled:        True
Serial Debug Enabled:      False

Proceed with firmware update? [Y]es or [N]o:y

Attempting serial upload using stm32flash

[1/3] Erasing flash...
./tools/64bit/stm32flash -o -S 0x8000000:129024 -b 115200 "/dev/ttyUSB0"
stm32flash Arduino_STM32_0.9

http://github.com/rogerclarkmelbourne/arduino_stm32

Interface serial_posix: 115200 8E1
Version      : 0x22
Option 1     : 0x00
Option 2     : 0x00
Device ID    : 0x0410 (Medium-density)
- RAM        : 20KiB  (512b reserved by bootloader)
- Flash      : 128KiB (sector size: 4x1024)
- Option RAM : 16b
- System RAM : 2KiB
Erasing flash

[2/3] Writing bootloader...
./tools/64bit/stm32flash -v -e 0 -g 0x8000000 -b 115200 -w "./bootloader/StmMulti4in1.bin" "/dev/ttyUSB0"
stm32flash Arduino_STM32_0.9

http://github.com/rogerclarkmelbourne/arduino_stm32

Using Parser : Raw BINARY
Interface serial_posix: 115200 8E1
Version      : 0x22
Option 1     : 0x00
Option 2     : 0x00
Device ID    : 0x0410 (Medium-density)
- RAM        : 20KiB  (512b reserved by bootloader)
- Flash      : 128KiB (sector size: 4x1024)
- Option RAM : 16b
- System RAM : 2KiB
Write to memory
Wrote and verified address 0x08001ff0 (100.00%) Done.

Starting execution at address 0x08000000... done.

[3/3] Writing firmware...
./tools/64bit/stm32flash -v -s 8 -e 0 -g 0x8002000 -b 115200 -w "/tmp/multi-stm-opentx-aetr-inv-v1.2.1.85.bin" "/dev/ttyUSB0"
stm32flash Arduino_STM32_0.9

http://github.com/rogerclarkmelbourne/arduino_stm32

Using Parser : Raw BINARY
Interface serial_posix: 115200 8E1
Version      : 0x22
Option 1     : 0x00
Option 2     : 0x00
Device ID    : 0x0410 (Medium-density)
- RAM        : 20KiB  (512b reserved by bootloader)
- Flash      : 128KiB (sector size: 4x1024)
- Option RAM : 16b
- System RAM : 2KiB
Write to memory
Wrote and verified address 0x08018954 (100.00%) Done.

Starting execution at address 0x08002000... done.

Firmware flashed successfully.

```
