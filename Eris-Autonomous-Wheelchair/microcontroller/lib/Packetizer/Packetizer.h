#ifndef packetizer_h
#define packetizer_h

#include "Arduino.h"

class Packetizer
{
  public:
    Packetizer();
    void begin(int baud_rate);
    int get(byte *buf);
    int send(byte *buf, byte num);

  private:
    static const byte MAX_PACKET_SIZE = 255;
    static const byte control_byte = '\n';
};


#endif
