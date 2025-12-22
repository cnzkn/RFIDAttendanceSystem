#pragma once

bool audioInit();
void audioTick();
void playTone(int freq, int duration);
void playSuccessSound();
void playBadReadSound();
void playComFailSound();