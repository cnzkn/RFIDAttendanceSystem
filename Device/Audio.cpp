#include <Arduino.h>
#include "Audio.h"

#define BUZZER_PIN 25
#define AUDIO_CHANNEL 0

struct {
  bool active = false;
  int freq = 0;
  unsigned long stopTime = 0;
  const int (*sequence)[2] = nullptr; // {freq, duration}
  int sequenceLength = 0;
  int currentStep = 0;
  unsigned long nextStepTime = 0;
} audioState;

bool audioInit() {
  ledcAttach(BUZZER_PIN, 2000, 8);
  ledcWriteTone(AUDIO_CHANNEL, 0);
  return true;
}

void audioTick() {
  unsigned long now = millis();

  if (audioState.active && audioState.sequence == nullptr) {
    if (now >= audioState.stopTime) {
      ledcWriteTone(AUDIO_CHANNEL, 0);
      audioState.active = false;
    }
  }

  else if (audioState.sequence != nullptr && audioState.active) {
    if (now >= audioState.nextStepTime) {
      ledcWriteTone(AUDIO_CHANNEL, 0);

      audioState.currentStep++;
      if (audioState.currentStep < audioState.sequenceLength) {
        int freq = audioState.sequence[audioState.currentStep][0];
        int dur  = audioState.sequence[audioState.currentStep][1];
        ledcWriteTone(AUDIO_CHANNEL, freq);
        audioState.nextStepTime = now + dur;
      } else {
        audioState.sequence = nullptr;
        audioState.active = false;
      }
    }
  }
}

void playTone(int freq, int duration) {
  ledcWriteTone(AUDIO_CHANNEL, freq);
  audioState.active = true;
  audioState.sequence = nullptr;
  audioState.freq = freq;
  audioState.stopTime = millis() + duration;
}

void playSuccessSound() {
  static const int successSequence[][2] = {
    {3000, 500}
  };

  audioState.sequence = successSequence;
  audioState.sequenceLength = sizeof(successSequence) / sizeof(successSequence[0]);
  audioState.currentStep = 0;
  audioState.active = true;
  ledcWriteTone(AUDIO_CHANNEL, successSequence[0][0]);
  audioState.nextStepTime = millis() + successSequence[0][1];
}

void playBadReadSound() {
  static const int badSequence[][2] = {
    {1500, 300},
    {0, 100},
    {1500, 300},
  };

  audioState.sequence = badSequence;
  audioState.sequenceLength = sizeof(badSequence) / sizeof(badSequence[0]);
  audioState.currentStep = 0;
  audioState.active = true;
  ledcWriteTone(AUDIO_CHANNEL, badSequence[0][0]);
  audioState.nextStepTime = millis() + badSequence[0][1];
}

void playComFailSound() {
  static const int comFailSequence[][2] = {
    {3000, 100},
    {0, 50},
    {2625, 100},
    {0, 50},
    {2250, 100},
    {0, 50},
    {1875, 100},
    {0, 50},
    {1500, 100}
  };

  audioState.sequence = comFailSequence;
  audioState.sequenceLength = sizeof(comFailSequence) / sizeof(comFailSequence[0]);
  audioState.currentStep = 0;
  audioState.active = true;
  ledcWriteTone(AUDIO_CHANNEL, comFailSequence[0][0]);
  audioState.nextStepTime = millis() + comFailSequence[0][1];
}
