#include <Arduino.h>
#include <Wire.h>
#include "API.h"
#include "Audio.h"
#include "LCD.h"
#include "RFID.h"
#include "Web.h"
#include "Logic.h"

#define SCREEN_UPDATE_INTERVAL 250

struct {
  bool enableScreenUpdates = true;
  clock_t nextScreenUpdate = 0;
  clock_t rfidEnableClock = 0;
  clock_t nextDataRequest = 0;
  int activeScreen = -1;
} logicState;

bool logicInit() {
  logicState.nextDataRequest = clock() + 60000;
  return true;
}

void logicTick() {
  if (logicState.enableScreenUpdates) {
    updateScreen();
  }

  if (!rfidActive()) {
    if (logicState.rfidEnableClock == 0) {
      rfidEnable();
      if (!rfidActive()) {
        pauseScreen();
        displayRfidErrorScreen();
      } else {
        resumeScreen();
      }
    } else if (clock() >= logicState.rfidEnableClock) {
      resumeScreen();
      rfidEnable();
      logicState.rfidEnableClock = 0; 
    }
  }

  if (rfidActive()) {
    uint8_t id[8];
    uint8_t length;
    RFIDReadStatus status = rfidRead(id, &length);
    if (status != READ_NONE) {
      rfidDisable();

      if (!isServiceConnected()) {
        displayComFailScreen();
        delayScreenUpdate(5000);
        playComFailSound();
        delayRfidEnable(3000);
      } else {
        if (status == READ_FAILURE) {
          displayRescanScreen();
          delayScreenUpdate(5000);
          playBadReadSound();
          delayRfidEnable(2000);
        } else if (status == READ_SUCCESS) {
          displayScanningScreen();
          sendCardRead(id, length);
        }
      }
    }
  }

  if (logicState.activeScreen == 1) {
    pauseScreen();
    if (!isServiceConnected()) {
      displayComFailScreen();
      delayScreenUpdate(5000);
      playComFailSound();
      delayRfidEnable(3000);
      resumeScreen();
    }

    if (hasScanResult()) {
      AttendanceStatus status = getScanResult();
      displayResultScreen(status);
      delayScreenUpdate(3000);
      if (status != SCAN_SUCCESS) {
        playBadReadSound();
      } else {
        playSuccessSound();
      }
      delayRfidEnable(1000);
      resumeScreen();
    } else {
      delayRfidEnable(10000);
    }
  }

  if (isServiceConnected()) {
    if (clock() > logicState.nextDataRequest) {
      logicState.nextDataRequest = clock() + 60000;
      requestModuleData();
    }
  }
}

void updateScreen() {
  if (clock() >= logicState.nextScreenUpdate) {
    displayMainScreen();
    logicState.nextScreenUpdate = clock() + SCREEN_UPDATE_INTERVAL;
  }
}

void pauseScreen() {
  logicState.enableScreenUpdates = false;
}

void resumeScreen() {
  logicState.enableScreenUpdates = true;
}


// Screen ID: 0
void displayMainScreen() {
  if (logicState.activeScreen == 0) {
    displayTime();

    if (moduleDataChanged()) {
      displayModuleInfo();
      clearModuleDataChanged();
    }
    return;
  }

  displayTime();
  lcdPrint(1, "    Scan ID card    ");
  lcdResetLine(2);
  displayModuleInfo();

  logicState.activeScreen = 0;
}

// Screen ID: 1
void displayScanningScreen() {
  if (logicState.activeScreen == 1) {
    return;
  }

  lcdReset();
  lcdPrint(1, "   Registering...   ");

  logicState.activeScreen = 1;
}

// Screen ID: 2
void displayRescanScreen() {
  if (logicState.activeScreen == 2) {
    return;
  }

  lcdReset();
  lcdPrint(1, "       Please       ");
  lcdPrint(2, "     scan again     ");

  logicState.activeScreen = 2;
}

// Screen ID: 3
void displayComFailScreen() {
  if (logicState.activeScreen == 3) {
    return;
  }

  lcdReset();
  lcdPrint(1, "      Server is     ");
  lcdPrint(2, "     unavailable    ");

  logicState.activeScreen = 3;
}

// Screen ID: 4
void displayRfidErrorScreen() {
  if (logicState.activeScreen == 4) {
    return;
  }

  lcdReset();
  lcdPrint(1, "     RFID ERROR     ");

  logicState.activeScreen = 4;
}

// Screen ID: 5
void displayResultScreen(AttendanceStatus status) {
  if (logicState.activeScreen == 5) {
    return;
  }

  lcdReset();
  
  if (status == 0) {
    char name[256];
    getScanName(name);

    char nameLines[4][21];
    int lineCount = 0;
    
    char tempName[256];
    strcpy(tempName, name);
    
    char* word = strtok(tempName, " ");
    char currentLine[21] = {0};
    
    while (word != NULL && lineCount < 4) {
      int wordLen = strlen(word);
      int currentLen = strlen(currentLine);
      
      if (currentLen == 0) {
        if (wordLen <= 20) {
          strcpy(currentLine, word);
        } else {
          strncpy(nameLines[lineCount++], word, 20);
          nameLines[lineCount-1][20] = '\0';
        }
      } else {
        if (currentLen + 1 + wordLen <= 20) {
          strcat(currentLine, " ");
          strcat(currentLine, word);
        } else {
          strcpy(nameLines[lineCount++], currentLine);
          currentLine[0] = '\0';
          
          if (wordLen <= 20) {
            strcpy(currentLine, word);
          } else {
            strncpy(nameLines[lineCount++], word, 20);
            nameLines[lineCount-1][20] = '\0';
          }
        }
      }
      
      word = strtok(NULL, " ");
    }
    
    if (strlen(currentLine) > 0 && lineCount < 4) {
      strcpy(nameLines[lineCount++], currentLine);
    }
    
    if (lineCount <= 2) {
      lcdResetLine(0);
      lcdPrint(1, "      Welcome,      ");
      for (int i = 0; i < lineCount; i++) {
        printCenteredLine(2 + i, nameLines[i]);
      }
      if (lineCount < 2) {
        lcdPrint(3, "                    ");
      }
    } else {
      lcdPrint(0, "      Welcome,      ");
      for (int i = 0; i < lineCount && i < 3; i++) {
        printCenteredLine(1 + i, nameLines[i]);
      }
    }
  } else if (status == SCAN_ALREADY_SCANNED) {
    lcdPrint(1, "Already scanned card");
  } else if (status == SCAN_NO_LECTURE) {
    lcdPrint(1, "   No lecture now   ");
  } else if (status == SCAN_NOT_REGISTERED) {
    lcdPrint(1, " Not registered in  ");
    lcdPrint(2, "     this lecture   ");
  } else if (status == SCAN_UNRECOGNIZED_ID) {
    lcdPrint(1, " Unrecognized card  ");
  } else if (status == SCAN_ERROR) {
    lcdPrint(1, "    Registration    ");
    lcdPrint(2, "       failed       ");
  }
  
  logicState.activeScreen = 5;
}

void printCenteredLine(int lcdLine, const char* text) {
  char displayLine[21];
  int textLen = strlen(text);
  int leftPad = (20 - textLen) / 2;
  int rightPad = 20 - textLen - leftPad;
  sprintf(displayLine, "%*s%s%*s", leftPad, "", text, rightPad, "");
  lcdPrint(lcdLine, displayLine);
}

void displayModuleInfo() {
  lcdResetLine(3);
  lcdSeek(3, 0);  
  lcdWrite(getClassroom());

  const char* lecture = getLecture();
  int len = strlen(lecture);
  lcdSeek(3, LCD_WIDTH - len);
  lcdWrite(lecture);
}

void displayTime() {
  struct tm timeinfo;
  if (!getLocalTime(&timeinfo)) {
    serialPrint("Failed to get local time for LCD display.");
    lcdPrint(0, "  /  /        :  :  ");
    return;
  }

  char dateTimeStr[21];
  sprintf(dateTimeStr, "%02d/%02d/%04d  %02d:%02d:%02d", 
          timeinfo.tm_mday, timeinfo.tm_mon + 1, timeinfo.tm_year + 1900,
          timeinfo.tm_hour, timeinfo.tm_min, timeinfo.tm_sec);
  lcdSeek(0, 0);
  lcdWrite(dateTimeStr);
  lcdSeek(0, 0);
}

void delayScreenUpdate(uint32_t offset) {
  logicState.nextScreenUpdate = clock() + offset;
}

void delayRfidEnable(uint32_t offset) {
  rfidDisable();
  logicState.rfidEnableClock = clock() + offset;
}
