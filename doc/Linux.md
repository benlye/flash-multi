# Flashing From Linux
A bash script and toolchain is provided for flashing Multiprotocol modules from Linux operating systems.  

Like the Flash Multi Window application, the Linux script will automatically determine which upload method to use, will preserve the EEPROM data during flashes, and will write the bootloader as needed.

## Installing
1. Download and unzip the latest Linux release
1. Configure serial device permissions by running `tools/install.sh` as root to update the udev rules and add the current user to the dialout group, e.g.:
   
   `sudo ./tools/install.sh`

If you prefer you can configure the permissions manually with these commands, for example:
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
