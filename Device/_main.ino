#include "API.h"
#include "Audio.h"
#include "LCD.h"
#include "RFID.h"
#include "Web.h"

void setup() {
  apiInit();

  serialPrint("Initializing LCD...");
  if (!lcdInit()) {
    halt();
  }

  serialPrint("Initializing RC522...");
	if (!rfidInit()) {
    halt();
  }

  serialPrint("Initializing audio...");
  if (!audioInit()) {
    halt();
  }

  serialPrint("Initializing web services...");
  if (!webInit()) {
    halt();
  }

  serialPrint("Initialized successfully.");
}

void loop() {
  apiTick();
  rfidTick();
  webTick();
  audioTick();
  // logicTick();
}


