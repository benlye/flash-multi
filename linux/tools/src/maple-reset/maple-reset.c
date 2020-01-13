/* Copyright (C) 2019 Ben Lye
 * 
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * Utility to send the reset sequence on RTS and DTR and chars
 * which resets the libmaple and causes the bootloader to be run
 */

#include <stdio.h>
#include <stdlib.h>
#include <termios.h>
#include <unistd.h>  
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <sys/ioctl.h>
#include <stdbool.h>

/* Function prototypes (belong in a seperate header file) */
int   openserial(char *devicename);
void  closeserial(void);
int   setDTR(unsigned short level);
int   setRTS(unsigned short level); 

/* Two globals for use by this module only */
static int fd;
static struct termios oldterminfo;

/* Opens the serial port and sets RTS and DTR to LOW */
/* Returns 1 on success or 0 on failure */
int openserial(char *devicename) 
{
     struct termios attr;

     if ((fd = open(devicename, O_RDWR)) == -1) return 0; /* Error */ 
     atexit(closeserial);

     if (tcgetattr(fd, &oldterminfo) == -1) return 0; /* Error */
     attr = oldterminfo;
     attr.c_cflag |= CRTSCTS | CLOCAL;
     attr.c_oflag = 0;
     if (tcflush(fd, TCIOFLUSH) == -1) return 0; /* Error */
     if (tcsetattr(fd, TCSANOW, &attr) == -1) return 0; /* Error */ 

     return setRTS(0) && setDTR(0);
}

/* Closes the serial port */
void closeserial(void)
{
	tcsetattr(fd, TCSANOW, &oldterminfo);
	close(fd);
}

/* Sets RTS to LOW or HIGH */
/* Returns 1 on success or 0 on failure */
int setRTS(unsigned short level)
{
  int status;

  if (ioctl(fd, TIOCMGET, &status) == -1) {
    perror("setRTS(): TIOCMGET");
    return 0;
  }
  
  if (level) status |= TIOCM_RTS;
  else status &= ~TIOCM_RTS;
  
  if (ioctl(fd, TIOCMSET, &status) == -1) {
    perror("setRTS(): TIOCMSET");
    return 0;
  }
  return 1;
}

/* Sets RTS to LOW or HIGH */
/* Returns 1 on success or 0 on failure */
int setDTR(unsigned short level)
{
    int status;

	if (ioctl(fd, TIOCMGET, &status) == -1) {
		perror("setDTR(): TIOCMGET");
		return 0;
	}
	if (level) status |= TIOCM_DTR;
	else status &= ~TIOCM_DTR;
	if (ioctl(fd, TIOCMSET, &status) == -1) {
		perror("setDTR: TIOCMSET");
		return 0;
	}
	return 1;
}

/* Main function of the program */
int main(int argc, char *argv[])
{
  printf("maple-reset 0.1\n\n");
  printf("This program is Free Sofware and has NO WARRANTY\n\n");

	if (argc != 2)
	{
		printf("Usage: maple-reset [serial device]\n\n");
		return 1;
	}

 	if (openserial(argv[1]))
	{
		if (setDTR(true) == 0)
    {
      fprintf(stderr, "Failed to set DTR\n\n");
      return 1;
    }

		usleep(50000L);
    
		if (setDTR(false) == 0)
    {
      fprintf(stderr, "Failed to set DTR\n\n");
      return 2;
    }
		usleep(50000L);
    
		if(!write(fd,"1EAF",4))
    {
      fprintf(stderr, "Could not send string to device.\n\n");
      return 3;
    }
		
    closeserial();
    fprintf(stdout, "Reset sequence sent to %s\n\n", argv[1]);
    return 0;
	}
	else
	{
    fprintf(stderr, "Could not open %s\n\n", argv[1]);
		return -1;
	}
}
