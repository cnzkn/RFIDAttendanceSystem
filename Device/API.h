#pragma once

#define min(a, b) (((a) < (b)) ? (a) : (b))

void apiInit();
void apiTick();

void halt();

void serialPrint(const char* text, ...);

const char* getClassroom();
const char* getLecture();
void setClassroom(const char* classroom);
void setLecture(const char* lecture);
