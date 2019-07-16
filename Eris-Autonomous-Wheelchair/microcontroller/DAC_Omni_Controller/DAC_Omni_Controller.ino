/*
  This is designed to accept serial communication from a computer to remotely control an R-Net Omni system used in powered wheelchairs. 
  The DACs used are MCP4921 consisting of 12 bits of accuracy with a maximum output of ~5V. To accomplish the required 4.8-7.2V output, a single supply op-amp system was devised
    to use the 12V available from the Omni directly and minimize the reliance on a large external power supply.
    
  To initial serial conversation with the Arduino, send a byte of value 250. The following bytes to that start byte are: xin, yin (value 100 is a 'zeroed' joy stick +- 100) and then
    any slave request bytes, up to 6 other bytes of information. To finish request to the Arduino, send a byte of char value 'z' (limitation of the readUntil apparently). The Arduino
    will then process and send back any slave request responses.
*/
/*
Setup (Assumes Uno):
Connect pin 13 (SCK) to each of the DACs and to the slave Arduino's pin 13
Connect pin 11 (MOSI) to each of the DACs and to the slave Arduino's pin 11
Connect pin 12 (MISO) to the slave Arduino's pin 12
Connect pin 7, 8, 9 to a respective DAC
Connect pin 5 to the slave Arduino's pins 2 & 10
*/
/*
Approximate Voltage Output DAC values
  6.0V = 100101001111 [12 bits];
  4.8V = 011101110110
  7.2V = 101110010000
  MUST pre-append on 0011 to any of the above for proper DAC operation, else it won't be enabled internally etc
  In decimal, there is about a +- 530 range for 0->100%
  */
#include <SPI.h>

//Omni Output Pins
const int dacChipSelect_A = 9;
const int dacChipSelect_B = 8;
const int dacChipSelect_C = 7;
const int slaveSelect = 5;
const int killSwitch = 3; //kill switch must be high to run normally
   
const int dacSettings = 0b0011000000000000; //The pre-append values for proper DAC operation
const int zero_val1 = 2400;
const int zero_val2 = 2380;

// x,y byte inputs read from Serial.
// Range: 0-200
char serialData[8];
byte returnData[8];
int numBytes = 0;
int xin;
int yin;
int op;
// x,y vals sent.
// Currently x,y voltages multiplied by 10.
// Ex: 2.7V is sent as 27
int xout;
int yout;
int out;
//int out1;
//int out2;

float xpercent;
float ypercent;

void setup(){
  //pinMode(led, OUTPUT);
  //setup digital pins to outputs
  pinMode(dacChipSelect_A, OUTPUT);
  pinMode(dacChipSelect_B, OUTPUT);
  pinMode(dacChipSelect_C, OUTPUT);
  pinMode(slaveSelect, OUTPUT);
  pinMode(killSwitch, INPUT);
  
  //Notify each device on the SPI data bus to be disabled
  digitalWrite(dacChipSelect_A, HIGH);
  digitalWrite(dacChipSelect_B, HIGH);
  digitalWrite(dacChipSelect_C, HIGH);
  digitalWrite(slaveSelect, HIGH);
  
  SPI.begin();
  //might consider changing SPI bus clock speed?
  //SPI.setBitOrder(MSBFIRST);
  //SPI.setDataMode(SPI_MODE3);
  SPI.setClockDivider(SPI_CLOCK_DIV8);
  
  //set all DAC to the 0 value (~3V)
  setDAC(dacChipSelect_A, (dacSettings | zero_val1));
  setDAC(dacChipSelect_B, (dacSettings | zero_val2));
  setDAC(dacChipSelect_C, (dacSettings | zero_val2));
  
  delay(1000); //waits for a second to be sure the values are set properly before any changes
  Serial.begin(9600);
}

void loop(){
  if(digitalRead(killSwitch)!= HIGH)
  {
    setDAC(dacChipSelect_B, (dacSettings | zero_val2));
    setDAC(dacChipSelect_C, (dacSettings | zero_val2));
    //flush input buffer
    flushSerialInput();
  }

  else if (Serial.available() >= 3){
    int in = Serial.read();
    // 250 is used as signal indicating next two bytes are x and y.
    if(in == 250){
      numBytes = Serial.readBytesUntil('z', serialData, 8); //reads until a 'z' is encountered again. Seems to be a limitation of readBytesUntil.
      //digitalWrite(led, HIGH);
      xin = byte(serialData[0]);
      yin = byte(serialData[1]);
      
      //Calculate the value to send the DAC
      xpercent = input_to_percent(xin);
      ypercent = input_to_percent(yin);
      xout = calc_volt(xpercent); //l
      yout = calc_volt(ypercent); //r

      setDAC(dacChipSelect_B, (dacSettings | xout));
      setDAC(dacChipSelect_C, (dacSettings | yout));
      
      if(numBytes > 2){ //there is at least 1 data request
        for(int b = 2; b < numBytes; b++){
          returnData[b-2] = getSlaveData(serialData[b]);
        }
      }

      //Return the setting to the serial caller
      Serial.write(250);
      //Serial.write(xin);
      //Serial.write(yin);
      if(numBytes > 2){
        for(int b = 2; b < numBytes; b++){
          Serial.write(returnData[b-2]);
        }
      }
      numBytes = 0;
      //digitalWrite(led, LOW);
    }
  }
}

//TODO: Figure out where exactly the Omni starts reading the signal. Most likely there is a 10-50% dead zone that is ignored.
int calc_volt(float percent){
  int out = 0;
  float volt = 0.0;
  
  volt = zero_val2 + (530 * percent);
  out = (int) volt;
  return out;
}

// Converts x,y input to -/+100%.
float input_to_percent(int b){
  float val = b;
  val = val - 100;
  val = val / 100;
  return val;
}

//Sets the 'dac' to the value of 'toSend', note toSend should be 16 bits (int), the 4 MSB are used in DAC setup and the other 12 are used for voltage data setup
void setDAC(int dac, int toSend){
  byte send1 = highByte(toSend);
  byte send2 = lowByte(toSend);

  noInterrupts();
  digitalWrite(dac, LOW);
  SPI.transfer(send1);
  SPI.transfer(send2);
  digitalWrite(dac, HIGH);
  interrupts();
}

byte getSlaveData(byte request){
      noInterrupts();
      //IF RESPONSE IS THE SAME AS INPUT, then make sure its wired properly!
      digitalWrite(slaveSelect, LOW);
      requestData(request); //sending, dont care about incoming data
      requestData(request); //dont care about dummy send, only care about in coming [which doesn't seem to work properly]
      byte ret = requestData(0);//for some reason, reliably gets data here
      digitalWrite(slaveSelect, HIGH);
      interrupts();
      return ret;
}

byte requestData(byte request){
  byte ret = SPI.transfer(request);
  delayMicroseconds(100);
  return ret;
}

void flushSerialInput(void){
    while(Serial.available()){
      Serial.read();
    }
}

