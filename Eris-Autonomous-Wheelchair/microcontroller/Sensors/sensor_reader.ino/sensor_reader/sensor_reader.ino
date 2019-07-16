#include <Adafruit_Sensor.h>
#include <Wire.h>
#include <Adafruit_BNO055.h>
#include <NewPing.h>
#include <Packetizer.h>

//constants
#define MAX_SONAR_DISTANCE 500
#define IMU_DESCRIPTOR_BYTE 0x00
#define SONAR_DESCRIPTOR_BYTE 0x01
#define IMU_CAL_DESCRIPTOR_BYTE 0x02
#define NUM_SONARS 6

//global variables
Packetizer packetizer = Packetizer();
NewPing sonar[NUM_SONARS] = {
  NewPing(13, 12, MAX_SONAR_DISTANCE),
  NewPing(11, 10, MAX_SONAR_DISTANCE),
  NewPing(9, 8, MAX_SONAR_DISTANCE),
  NewPing(7, 6, MAX_SONAR_DISTANCE),
  NewPing(5, 4, MAX_SONAR_DISTANCE),
  NewPing(3, 2, MAX_SONAR_DISTANCE)
};
Adafruit_BNO055 bno = Adafruit_BNO055();
imu::Vector<3> euler;
uint8_t system_cal, gyro_cal, accel_cal, mag_cal;
int32_t distance[NUM_SONARS];
byte buffer[32]={0};

void setup() {
    packetizer.begin(9600);
    bno.begin();

    delay(500);
    bno.setExtCrystalUse(true);
}

void loop() {

  //ping the sonar sensor
  for (int i=0; i < NUM_SONARS; ++i)
    distance[i] = sonar[i].ping_cm();

  //read the IMU data
  //data is accessed by calling euler.x(), euler.y(), and euler.z()
  euler = bno.getVector(Adafruit_BNO055::VECTOR_EULER);

  //read the IMU's current calibration status
  bno.getCalibration(&system_cal, &gyro_cal, &accel_cal, &mag_cal);


  //send IMU data up to the computer
  buffer[0] = IMU_DESCRIPTOR_BYTE;
  float2bytes(euler.x(), &buffer[1]);
  float2bytes(euler.y(), &buffer[5]);
  float2bytes(euler.z(), &buffer[9]);
  //uncomment to use ints instead
  // int2bytes((int32_t)euler.x(), &buffer[1]);
  // int2bytes((int32_t)euler.y(), &buffer[5]);
  // int2bytes((int32_t)euler.z(), &buffer[9]);
  packetizer.send(buffer, 13);


  //send sonar data up to the computer
  buffer[0] = SONAR_DESCRIPTOR_BYTE;
  for (int i=0; i < NUM_SONARS; ++i)
  {
    buffer[1] = i; //which sonar sensor
    int2bytes(distance[i], &buffer[2]);
    packetizer.send(buffer, 6);
  }


  //send IMU calibration data up to the computer
  buffer[0] = IMU_CAL_DESCRIPTOR_BYTE;
  int2bytes(system_cal, &buffer[1]);
  int2bytes(gyro_cal, &buffer[5]);
  int2bytes(accel_cal, &buffer[9]);
  int2bytes(mag_cal, &buffer[13]);
  packetizer.send(buffer, 15);

  delay(200);
}

void int2bytes(int32_t mint, byte *buffer)
{
  union int_bytes
  {
    int32_t value;
    byte bytes[4];
  }union_int;

  union_int.value = mint;

  buffer[3] = union_int.bytes[0];
  buffer[2] = union_int.bytes[1];
  buffer[1] = union_int.bytes[2];
  buffer[0] = union_int.bytes[3];

}

void float2bytes(float mfloat, byte *buffer)
{
  union float_bytes
  {
    float value;
    byte bytes[4];
  }ufloat;

  ufloat.value = mfloat;

  buffer[3] = ufloat.bytes[0];
  buffer[2] = ufloat.bytes[1];
  buffer[1] = ufloat.bytes[2];
  buffer[0] = ufloat.bytes[3];

  return;
}
