#pragma once

#define LCD_WIDTH 20
#define LCD_HEIGHT 4

bool lcdInit();
void lcdPrint(int line, const char* text);
void lcdSeek(int line, int col);
void lcdWrite(const char* text);
void lcdResetLine(int line);
void lcdReset();