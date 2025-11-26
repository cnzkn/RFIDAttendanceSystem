#pragma once

#include "Cert.h"

const int PKT_REQUESTINFO = 0x49514552;
const int PKT_SUBMITSCAN = 0x41514552;

const int RSP_MODULEINFO = 0x464E494D;
const int RSP_SCANRESULT = 0x53455253;

bool webInit();
void webTick();
