#pragma once

#include "Cert.h"
#include <WebSocketsClient.h>

const int PKT_REQUESTINFO = 0x49514552;
const int PKT_SUBMITSCAN = 0x41514552;
const int PKT_PING = 0x474E4950;
const int PKT_PONG = 0x474E4F50;

const int RSP_MODULEINFO = 0x464E494D;
const int RSP_SCANRESULT = 0x53455253;

bool webInit();
void webTick();

bool isServiceConnected();
void setClock();

void requestModuleData();
void sendCardRead(uint8_t* id, uint8_t length);
