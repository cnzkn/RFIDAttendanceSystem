#include <Arduino.h>
#include <string.h>
#include "API.h"

struct {
  char assignedClassroom[8];
  char currentLecture[9];
  bool moduleDataChanged = false;
  int time;
  AttendanceStatus lastScanStatus = SCAN_INVALID;
  char lastScanName[256];
} moduleStatus;

void apiInit() {
	Serial.begin(9600);
	while (!Serial);

  moduleStatus.assignedClassroom[0] = '-';
  moduleStatus.assignedClassroom[1] = '-';
  moduleStatus.assignedClassroom[2] = '-';
  moduleStatus.assignedClassroom[3] = 0;
  moduleStatus.currentLecture[0] = '-';
  moduleStatus.currentLecture[1] = '-';
  moduleStatus.currentLecture[2] = '-';
  moduleStatus.currentLecture[3] = 0;
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
  int length = MIN(strlen(text), 7);
  strncpy(moduleStatus.assignedClassroom, text, length);
  moduleStatus.assignedClassroom[length] = 0;
  moduleStatus.moduleDataChanged = true;
}

void setLecture(const char* text) {
  int length = MIN(strlen(text), 8);
  strncpy(moduleStatus.currentLecture, text, length);
  moduleStatus.currentLecture[length] = 0;
  moduleStatus.moduleDataChanged = true;
}

bool moduleDataChanged() {
  return moduleStatus.moduleDataChanged;
}

void clearModuleDataChanged() {
  moduleStatus.moduleDataChanged = false;
}

bool hasScanResult() {
  return moduleStatus.lastScanStatus != SCAN_INVALID;
}

AttendanceStatus getScanResult() {
  AttendanceStatus val = moduleStatus.lastScanStatus;
  moduleStatus.lastScanStatus = SCAN_INVALID;
  return val;
}

int getScanName(char* out) {
  int len = MIN(strlen(out), 255);
  strncpy(out, moduleStatus.lastScanName, len);
  return len;
}

void setScanResult(uint8_t status, char* name) {
  if (hasScanResult()) {
    serialPrint("WARNING! A new scan result arrived before the previous one could be handled. Overwriting the old result... (This may potentially cause issues)");
  }

  moduleStatus.lastScanStatus = (AttendanceStatus)status;
  if (name == 0) {
    moduleStatus.lastScanName[0] = 0;
  } else {
    strncpy(moduleStatus.lastScanName, name, MIN(strlen(name), 255));
  }
}