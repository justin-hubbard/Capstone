#export PYTHONPATH = 'a'
import serial
from time import sleep
from sys import exit

class _Getch:
    """Gets a single character from standard input.  Does not echo to the
screen."""
    def __init__(self):
        try:
            self.impl = _GetchWindows()
        except ImportError:
            self.impl = _GetchUnix()

    def __call__(self): return self.impl()


class _GetchUnix:
    def __init__(self):
        import tty, sys

    def __call__(self):
        import sys, tty, termios
        fd = sys.stdin.fileno()
        old_settings = termios.tcgetattr(fd)
        try:
            tty.setraw(sys.stdin.fileno())
            ch = sys.stdin.read(1)
        finally:
            termios.tcsetattr(fd, termios.TCSADRAIN, old_settings)
        return ch


class _GetchWindows:
    def __init__(self):
        import msvcrt

    def __call__(self):
        import msvcrt
        return msvcrt.getch()



if __name__ == "__main__":
    #sets up serial port to talk across
    port_to_use = input("Please enter the port to use (looks like /dev/ttyXXXX): ")
    try:
        ser = serial.Serial(
            port=port_to_use,
            baudrate=9600,
            parity=serial.PARITY_NONE,
            bytesize=serial.EIGHTBITS,
            xonxoff = True,
            rtscts = True,
            dsrdtr = True,
            timeout = 1
        )
    except:
        print( "Port mismatch, please try again")
        exit()

    #ser.open()
    ser.isOpen()
    print("Press 'c' to exit\n\
Press 'w' for full forward\n\
Press 's' for full backward\n\
Press 'a' for full left\n\
Press 'd' for full right\n\
Press anything else for stop\n\
NOTE: DOES NOT AUTOMATICALLY RESET TO ZERO WILL CONTINUE AT LAST BUTTON PRESS\n\
May require pressing a key a few times intially\n")
    while True:
        getch = _Getch().impl()
        key = 250
        xval = 0
        yval = 0
        if(getch == 'w'):
            yval = 200
        elif(getch == 's'):
            yval = 0
        else:
            yval = 100
        if(getch == 'a'):
            xval = 0
        elif(getch == 'd'):
            xval = 200
        else:
            xval = 100
        if (getch == 'c'):
            break
        print('Xval: '+str(xval))
        print('Yval: '+str(yval))

        #Sets up frame to send to the arduino in bytes
        frame = bytearray()
        frame.append(250)
        frame.append(xval)
        frame.append(yval)
        frame.append('z')

        try:
            print('Number of bytes written: '+str(ser.write(frame)))
        except:
            print("Unable to communicate to Arduino")
        sleep(.25)
        outframe = bytearray()
        try:
            outframe.append(ser.read(1))
            #to read back more bytes, uncomment/add more of the below lines (1 for each byte)
            #outframe.append(ser.read(1))
            #outframe.append(ser.read(1))
            print( 'Output in hex: '+''.join(format(x, '02x') for x in outframe))
        except:
            continue
