#!/bin/sh

if sudo [ -w /etc/udev/rules.d ]; then
    echo "Copying Maple-specific udev rules..."
    sudo cp -v $(dirname "$0")/45-maple.rules /etc/udev/rules.d/45-maple.rules
    sudo chown root:root /etc/udev/rules.d/45-maple.rules
    sudo chmod 644 /etc/udev/rules.d/45-maple.rules
    echo "Reloading udev rules"
    sudo udevadm control --reload-rules
    echo "Adding current user to dialout and plugdev groups"
    sudo usermod -a -G plugdev $USER
    sudo usermod -a -G dialout $USER
else
    echo "Couldn't copy to /etc/udev/rules.d/; Did you run the script as root? Does your distribution of Linux include udev?"
fi
