#pragma once

bool logicInit();
void logicTick();

// Tick functions
void updateScreen();
void pauseScreen();
void resumeScreen();

// Menu display functions
void displayMainScreen();
void displayScanningScreen();
void displayRescanScreen();
void displayComFailScreen();
void displayRfidErrorScreen();
void displayResultScreen(AttendanceStatus status);

// Utility display functions
void printCenteredLine(int lcdLine, const char* text);
void displayModuleInfo();
void displayTime();
void delayScreenUpdate(uint32_t offset);
void delayRfidEnable(uint32_t offset);
