#pragma once

bool lcdInit();
void lcdPrint(int line, const char* text);
void lcdSeek(int line, int col);
void lcdWrite(const char* text);
void lcdResetLine(int line);
void lcdReset();