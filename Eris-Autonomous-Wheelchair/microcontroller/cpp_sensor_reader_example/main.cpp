#include "Packetizer.hpp"


using std::cout;
using std::endl;


#define IMU_DESCRIPTOR_BYTE 0x00
#define SONAR_DESCRIPTOR_BYTE 0x01
#define IMU_CAL_DESCRIPTOR_BYTE 0x02


float bytes2float(uint8_t *buffer)
{
	union float_bytes {
		float value;
		uint8_t bytes[4];
	}ufloat;

	ufloat.bytes[3] = buffer[0];
	ufloat.bytes[2] = buffer[1];
	ufloat.bytes[1] = buffer[2];
	ufloat.bytes[0] = buffer[3];

	return ufloat.value;
}

int bytes2int(uint8_t *buffer)
{

	union int_bytes {
		int value;
		uint8_t bytes[4];
	}union_int;


	union_int.bytes[3] = buffer[0];
	union_int.bytes[2] = buffer[1];
	union_int.bytes[1] = buffer[2];
	union_int.bytes[0] = buffer[3];

	return union_int.value;

}

int main (int argc, char **argv)
{

	if(argc < 2)
	{
		cout << "must supply port to open" << endl;
		exit(1);
	}

	Packetizer p(argv[1], 0x0A);
	uint8_t buffer[128];
	int size;
	char tmp[32];

	cout << "float: " << sizeof(float) << endl;

	while(1)
	{
		size = p.get(buffer);
		switch(buffer[0])
		{
			case IMU_DESCRIPTOR_BYTE:
				cout << "imu packet" << endl;
				cout << "x: " << bytes2float(&buffer[1]);
				cout << "\ty: " << bytes2float(&buffer[5]);
				cout << "\tz: " << bytes2float(&buffer[9]) << endl;
				break;
			case SONAR_DESCRIPTOR_BYTE:
				sprintf(tmp, "%d", buffer[1]);
				cout << "sonar packet[" << tmp << ']' << endl;
				cout << "distance (cm): " << bytes2int(&buffer[2]) << endl;
				break;
			case IMU_CAL_DESCRIPTOR_BYTE:
				cout << "calibration packet" << endl;
				cout << "sys: " << bytes2int(&buffer[1]) << endl;
				break;
			default:
				cout << "unrecognized packet" << endl;
		}
		cout << endl;
	}

	return 0;
}
