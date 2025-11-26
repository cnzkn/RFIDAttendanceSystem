#include <Arduino.h>
#include <string.h>
#include "API.h"

struct {
  char assignedClassroom[7];
  char currentLecture[8];
  int time;
} moduleStatus;

void apiInit() {
	Serial.begin(9600);
	while (!Serial);

  moduleStatus.assignedClassroom[0] = '-';
  moduleStatus.assignedClassroom[1] = '-';
  moduleStatus.assignedClassroom[2] = '-';
  moduleStatus.assignedClassroom[3] = 0;
  moduleStatus.currentLecture[0] = 0;
}

void apiTick() {
  
}

void halt() {
  Serial.println("System halted.");
  while(1);
}

void serialPrint(const char* text, ...) {
  char buf[256];
  va_list args;
  va_start(args, text);
  vsnprintf(buf, sizeof(buf), text, args);
  va_end(args);

  Serial.print("[");
  Serial.print(millis());
  Serial.print("] ");
  Serial.println(buf);
}

const char* getClassroom() {
  return moduleStatus.assignedClassroom;
}

const char* getLecture() {
  return moduleStatus.currentLecture;
}

void setClassroom(const char* text) {
  strncpy(moduleStatus.assignedClassroom, text, min(strlen(text), 6));
}

void setLecture(const char* text) {
  strncpy(moduleStatus.currentLecture, text, min(strlen(text), 7));
}
