#include <Arduino.h>
#include <WiFi.h>
#include <WiFiClientSecure.h>
#include <WebSocketsClient.h>
#include "API.h"
#include "Web.h"

const char* ssid = "{WIFI_NET_SSID}"; // Automatically attached at build
const char* password = "{WIFI_NET_PASSWORD}"; // Automatically attached at build
const char* server = "cng495.canozkan.com.tr";
const int port = 443;
const char* path = "/ws/device";

bool wifiConnected = false;
bool wsConnected = false;
bool syncingClock = false;

WebSocketsClient webSocket;
WiFiClientSecure client;

void webSocketEvent(WStype_t type, uint8_t* payload, size_t length);
void setClock();

bool webInit() {
  WiFi.begin(ssid, password);

  webSocket.beginSslWithCA(server, port, path, root_ca);
  webSocket.setSSLClientCertKey(client_cert, client_key);

  webSocket.onEvent(webSocketEvent);
  webSocket.setReconnectInterval(3000);

  return true;
}

void webTick() {
  if (WiFi.status() != WL_CONNECTED) {
    if (wifiConnected) {
      serialPrint("Disconnected from Wi-Fi. Attempting to reconnect...");
      wifiConnected = false;
    }
    return;
  }

  if (!wifiConnected) {
    serialPrint("Connected to Wi-Fi.");
    setClock();
    wifiConnected = true;
  }

  if (syncingClock) {
    time_t nowSecs = time(nullptr);
    if (nowSecs > 8 * 3600 * 2) {
      struct tm timeinfo;
      gmtime_r(&nowSecs, &timeinfo);
      serialPrint("Clock synchronization complete. Current time: %s", asctime(&timeinfo));
      syncingClock = false;
    }
  }
}

void webSocketEvent(WStype_t type, uint8_t* payload, size_t length) {
  switch(type) {
    case WStype_CONNECTED:
      Serial.println("Connected to server.");
      webSocket.sendBIN((uint8_t*)&PKT_REQUESTINFO, 4);
      break;
    case WStype_BIN:
      if (length >= 4) {
        uint32_t hdr = *(uint32_t*)payload;
        switch(hdr) {
          case RSP_MODULEINFO: {
            // setClassroom(payload + 4);
            // int len = strlen(payload + 4) + 1;
            // setLecture(payload + 4 + len);
            break;
          }

          case RSP_SCANRESULT: {
            break;
          }
        }
      }
      Serial.printf("Received: %s\n", payload);
      break;
    case WStype_DISCONNECTED:
      Serial.println("Lost connection to server.");
      break;
  }
}

void setClock() {
  configTime(0, 0, "pool.ntp.org", "time.nist.gov");
  serialPrint("Waiting for NTP time sync...");
  syncingClock = true;
}
