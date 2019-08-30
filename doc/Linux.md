# Flashing From Linux
A bash script is provided for flashing Multiprotocol modules from Linux operating systems.

## Installing
1. Download
1. Configure serial device permissions
   1. Automatically, by running `tools/install.sh` as root to update the udev rules and add the current user to the `dialout` group, e.g.:
   
      `sudo ./tools/install.sh`
   
   1. Manually by running these command:
   
      ```
      sudo cp -v ./tools/45-maple.rules /etc/udev/rules.d/45-maple.rules
      sudo chown root:root /etc/udev/rules.d/45-maple.rules
      sudo chmod 644 /etc/udev/rules.d/45-maple.rules
      sudo udevadm control --reload-rules
      sudo usermod -a -G plugdev $USER
      sudo usermod -a -G dialout $USER
      ```
## Use
1. Download the latest pre-compiled firmware
1. Plug your Multiprotocol module in
1. Run the flash-multi shell script, specifiying the serial device and firmware file

   `flash-multi -f [firmware file] -p [serial device]`
