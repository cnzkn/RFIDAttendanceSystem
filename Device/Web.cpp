#include <Arduino.h>
#include <WiFi.h>
#include <WiFiClientSecure.h>
#include <WebSocketsClient.h>
#include "API.h"
#include "Web.h"

const char* ssid = "{WIFI_NET_SSID}"; // Automatically attached at build
const char* password = "{WIFI_NET_PASSWORD}"; // Automatically attached at build
const char* server = "cng495-api.canozkan.com.tr";
const int port = 443;
const char* path = "/ws/device";

struct {
  bool wifiConnected = false;
  bool wsConnected = false;
  bool syncingClock = false;

  WebSocketsClient webSocket;
  WiFiClientSecure client;
} webStatus;

void webSocketEvent(WStype_t type, uint8_t* payload, size_t length);

bool webInit() {
  WiFi.begin(ssid, password);
  return true;
}

bool isServiceConnected() {
  return webStatus.wsConnected;
}

void webTick() {
  if (WiFi.status() != WL_CONNECTED) {
    if (webStatus.wifiConnected) {
      serialPrint("Disconnected from Wi-Fi. Attempting to reconnect...");
      webStatus.wifiConnected = false;
      webStatus.wsConnected = false;
    }
    return;
  }

  if (!webStatus.wifiConnected) {
    serialPrint("Connected to Wi-Fi.");
    setClock();
    webStatus.wifiConnected = true;
    
    webStatus.webSocket.setSSLClientCertKey(client_cert, client_key);
    webStatus.webSocket.beginSslWithCA(server, port, path, root_ca_server);
    webStatus.webSocket.onEvent(webSocketEvent);
    webStatus.webSocket.setReconnectInterval(3000);
  }

  webStatus.webSocket.loop();

  if (webStatus.syncingClock) {
    struct tm timeinfo;
    if (getLocalTime(&timeinfo)) {
      serialPrint("Clock synchronization complete. Current time: %s", asctime(&timeinfo));
      webStatus.syncingClock = false;
    }
  }
}

void webSocketEvent(WStype_t type, uint8_t* payload, size_t length) {
  switch(type) {
    case WStype_CONNECTED:
      serialPrint("Connected to server.");
      webStatus.wsConnected = true;
      requestModuleData();
      break;
    case WStype_BIN:
      if (length >= 4) {
        uint32_t hdr = *(uint32_t*)payload;
        switch(hdr) {
          case RSP_MODULEINFO: { // <Classroom:M><NULL><Lecture:N><NULL>
            setClassroom((char*)(payload + 4));
            int len = strlen((char*)(payload + 4)) + 1;
            setLecture((char*)(payload + 4 + len));
            serialPrint("Classroom: %s - Lecture: %s", getClassroom(), getLecture());
            break;
          }

          case RSP_SCANRESULT: { // <Result:8> | <Result:8><Name:N><NULL>
            uint8_t status = *(payload + 4);
            char buf[256];
            buf[0] = 0;

            if (status == 0) {
              int nameLen = MIN(length - 5, 255);
              strncpy(buf, (char*)(payload + 5), nameLen);
              buf[nameLen] = 0;
            }

            setScanResult(status, buf[0] == 0 ? NULL : buf);
            serialPrint("Scan Result: %d | Name: \"%s\"", status, buf[0] == 0 ? "(null)" : buf);
            break;
          }

          case PKT_PING: {
            webStatus.webSocket.sendBIN((uint8_t*)&PKT_PONG, 4);
          }
        }
      }
      break;
    case WStype_DISCONNECTED:
      serialPrint("Lost connection to server.");
      webStatus.wsConnected = false;
      break;
  }
}

void setClock() {
  configTime(UTC_OFFSET_WINTER, UTC_OFFSET_SUMMER, "pool.ntp.org", "time.nist.gov");
  serialPrint("Waiting for NTP time sync...");
  webStatus.syncingClock = true;
}

void requestModuleData() {
  webStatus.webSocket.sendBIN((uint8_t*)&PKT_REQUESTINFO, 4);
}

void sendCardRead(uint8_t* id, uint8_t length) {
  uint8_t packet[13];
  
  *(uint32_t*)packet = PKT_SUBMITSCAN;
  *(uint8_t*)(packet + 4) = length;
  memcpy(packet + 5, id, length);

  webStatus.webSocket.sendBIN(packet, 5 + length);
}
