# Flashing From Linux
A bash script is provided for flashing Multiprotocol modules from Linux operating systems.

## Installing
1. Download
1. Configure serial device permissions by running `tools/install.sh` as root to update the udev rules and add the current user to the `dialout` group, e.g. `sudo ./tools/install.sh`

## Use
1. Download the latest pre-compiled firmware
1. Plug your Multiprotocol module in
1. Run the flash-multi shell script, specifiying the serial device and firmware file

   `flash-multi -f [firmware file] -p [serial device]`
