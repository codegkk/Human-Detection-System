# Human-Detection-System
A system that plays adverts ONLY when a human being is detected

## Effective Advertising
*Effective advertising tries to make sure that the target actually sees the advert. It is important to save resources when no human is detected.*

1x Arduino Nano Board
1x Arduino Extension Board
1x Bread Board
3x Light Emitting Diodes
2x Ultrasound Sensors (HC-SR04)
1x Sound Sensor (LM393)
1x Buzzer
1x Button
Jumper Cables

## The Circuit
*The circuit switches on LED1 or LED2 or LED3 or all LEDs depending on which sensor has detected a human being.*

![]()

## The GUI
*When a human being is detected the arduino circuit then triggers a video to be played. If no human is detected then no video will be played.*

![]()

## The Code
```
int ledPin = 13;                // choose the pin for the LED
int inputPin = 1;               // choose the input pin (for PIR sensor)
int pirState = LOW;             // we start, assuming no motion detected
int val = 0;
#define trigPin1 3
#define echoPin1 2
#define trigPin2 4
#define echoPin2 5
//#define trigPin3 7
//#define echoPin3 8

long duration, distance, RightSensor,BackSensor,FrontSensor,LeftSensor;

void setup()
{
Serial.begin (9600);
pinMode(trigPin1, OUTPUT);
pinMode(echoPin1, INPUT);
pinMode(trigPin2, OUTPUT);
pinMode(echoPin2, INPUT);
pinMode(ledPin, OUTPUT);      // declare LED as output
pinMode(inputPin, INPUT);     // declare sensor as input



//pinMode(trigPin3, OUTPUT);
//pinMode(echoPin3, INPUT);
}

void loop() {
 // put your main code here, to run repeatedly:
   val = digitalRead(inputPin);  // read input value
  if (val == HIGH) {            // check if the input is HIGH
    digitalWrite(ledPin, HIGH);  // turn LED ON
    if (pirState == LOW) {
      // we have just turned on
      Serial.println("Motion detected!");
      // We only want to print on the output change, not state
      pirState = HIGH;  
SonarSensor(trigPin1, echoPin1);
RightSensor = distance;
SonarSensor(trigPin2, echoPin2);
LeftSensor = distance;
//SonarSensor(trigPin3, echoPin3);
//FrontSensor = distance;

Serial.print(LeftSensor);
Serial.print(" - ");
//Serial.print(FrontSensor);
//Serial.print(" - ");
Serial.println(RightSensor);

 
}

void SonarSensor(int trigPin,int echoPin)
{
digitalWrite(trigPin, LOW);
delayMicroseconds(20000);
digitalWrite(trigPin, HIGH);
delayMicroseconds(10000);
digitalWrite(trigPin, LOW);
duration = pulseIn(echoPin, HIGH);
distance = (duration/2) / 29.1;

}
```
