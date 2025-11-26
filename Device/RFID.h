#pragma once

#define RST_PIN 13
#define SS_PIN 5

enum RFIDReadStatus {
  READ_NONE = -1,
  READ_SUCCESS,
  READ_FAILURE,
  READ_NOTINIT
};

bool rfidInit();
void rfidTick();
bool rfidActive();

void rfidEnable();
void rfidDisable();
RFIDReadStatus rfidRead(byte* id, byte* length);