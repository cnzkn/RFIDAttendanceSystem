#include <Arduino.h>
#include <Wire.h>
#include <LiquidCrystal_I2C.h>
#include "LCD.h"

LiquidCrystal_I2C* lcd = nullptr;

bool lcdInit() {
  Wire.begin(21, 22);

  byte lcdAddress = 0;
  for (byte addr = 1; addr < 127; addr++) {
    Wire.beginTransmission(addr);
    if (Wire.endTransmission() == 0) {
      if (!lcdAddress) lcdAddress = addr; // first device found assumed LCD
    }
  }

  if (lcdAddress == 0) {
    Serial.println("No I2C devices found!");
    return false;
  }
  
  Serial.print("Using LCD at address 0x");
  Serial.println(lcdAddress, HEX);

  lcd = new LiquidCrystal_I2C(lcdAddress, 20, 4);
  lcd->init();
  lcd->backlight();
  lcd->setCursor(0,0);
  lcd->print("Boot");

  return true;
}

void lcdPrint(int line, const char* text) {
  lcdResetLine(line);
  lcdSeek(line, 0);
  lcdWrite(text);
}

void lcdSeek(int line, int col) {
  lcd->setCursor(col, line);
}

void lcdWrite(const char* text) {
  lcd->print(text);
}

void lcdResetLine(int line) {
  lcdSeek(line, 0);
  lcdWrite("                    ");
}

void lcdReset() {
  lcdResetLine(0);
  lcdResetLine(1);
  lcdResetLine(2);
  lcdResetLine(3);
  lcdSeek(0, 0);
}