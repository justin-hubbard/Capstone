#ifndef __PACKETIZER_H__
#define __PACKETIZER_H__

#include <cstdint>
#include "Serial.hpp"

	class Packetizer
	{
		public:
			Packetizer(const char *port_name, uint8_t control_byte);
			~Packetizer();
			int get(uint8_t *buf);
			int send(uint8_t *buf, uint8_t num);
            int queryBuffer();

		private:
			void get_lock(void);

			Serial *serial;
			char control_byte;

            
            static const int MAX_PACKET_SIZE = 255;
	};

#endif
