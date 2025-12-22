#include "API.h"
#include "Audio.h"
#include "LCD.h"
#include "RFID.h"
#include "Web.h"
#include "Logic.h"

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

  serialPrint("Initializing logic controller...");
  if (!logicInit()) {
    halt();
  }

  serialPrint("Initialized successfully.");
}

void loop() {
  clock_t start = clock();
  apiTick();
  rfidTick();
  webTick();
  audioTick();
  logicTick();
  clock_t end = clock();
  if (end - start > 100) {
    serialPrint("Last loop took %.2f seconds.", (end - start) / 1000.0f);
  }
}


