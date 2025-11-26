#include <SPI.h>
#include <MFRC522.h>
#include "API.h"
#include "RFID.h"

bool rfidConnected = false;
bool rfidAntenna = false;
MFRC522* mfrc522 = nullptr; 

unsigned char checkRfid() {
  if (mfrc522 == nullptr) return false;
  unsigned char res = mfrc522->PCD_ReadRegister(MFRC522::VersionReg);
  return (res != 0x00 && res != 0xFF) ? (unsigned char)res : 0x00;
}

bool rfidInit() {
  SPI.begin();

  mfrc522 = new MFRC522(SS_PIN, RST_PIN);
  
  int begin = millis();
  mfrc522->PCD_Init();

  // TODO: May add delay here if RFID module does not work.
  unsigned char ver = 0x0;
  while (millis() - begin < 3000) {
    ver = checkRfid();
    if (ver) {
      break;
    }
    delay(1);
  }

  if (!ver) {
    serialPrint("Communication to RFID module failed.");
    return false;
  }

  mfrc522->PCD_AntennaOff();

  serialPrint("RFID module version: 0x%02X", ver);
  rfidConnected = true;
  rfidAntenna = false;
  return true;
}

void rfidTick() {
  static int lastCheck = 0;

  if (mfrc522 == nullptr) return;
  if (millis() - lastCheck > (rfidConnected ? 15000 : 1000)) {
    unsigned char ver = mfrc522->PCD_ReadRegister(MFRC522::VersionReg);
    if (rfidConnected && !ver) {
      serialPrint("Connection to RFID module lost.");
      rfidConnected = false;
    } else if (!rfidConnected && ver) {
      serialPrint("Connection to RFID module restored.");
      rfidConnected = true;
    }
    lastCheck = millis();
  }
}

bool rfidActive() {
  return rfidConnected && rfidAntenna;
}

void rfidEnable() {
  if (mfrc522 == nullptr) return;

  serialPrint("RFID antenna is on.");
  mfrc522->PCD_AntennaOn();
  rfidAntenna = true;
}

void rfidDisable() {
  if (mfrc522 == nullptr) return;

  serialPrint("RFID antenna is off.");
  mfrc522->PCD_AntennaOff();
  rfidAntenna = false;
}

RFIDReadStatus rfidRead(byte* id, byte* length) {
  if (mfrc522 == nullptr) return READ_NOTINIT;

  if (!mfrc522->PICC_IsNewCardPresent()) {
		return READ_NONE;
	}

	if (!mfrc522->PICC_ReadCardSerial()) {
		return READ_FAILURE;
	}

  *length = mfrc522->uid.size;
  for(int i = 0; i < mfrc522->uid.size; ++i) {
    id[i] = mfrc522->uid.uidByte[i];
  }

  return READ_SUCCESS;
}