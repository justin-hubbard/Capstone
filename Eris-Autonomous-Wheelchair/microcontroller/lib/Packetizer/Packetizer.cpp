#include "Packetizer.h"

Packetizer::Packetizer(void)
{
}

void Packetizer::begin(int baud_rate)
{
  Serial.begin(baud_rate);
}

int Packetizer::get(byte *buf)
{
  byte header[2];
  bool success = false;
  int size = -1;

  while(!success)
  {
    if(Serial.available() < 2) //check if there are enough bytes to construct a header
      return -1;

    Serial.readBytes(header, 2); //read the header
    if(header[0] != control_byte)
    {

    }

  }



  return size;
}

int Packetizer::send(byte *buf, byte num)
{
  byte send_buf[MAX_PACKET_SIZE+2];

  send_buf[0] = control_byte;
  send_buf[1] = num;

  memcpy(send_buf+2, buf, num);

  Serial.write(send_buf, num+2);

  return 0;
}
