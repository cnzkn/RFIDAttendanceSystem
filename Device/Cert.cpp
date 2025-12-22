#include <Arduino.h>
#include "Cert.h"

const char* root_ca = "{CERT_AUTHORITY}"; // Automatically attached at build

const char* root_ca_server = "{CERT_AUTHORITY_SERVER}"; // Automatically attached at build

const char* client_cert = "{CLIENT_CERTIFICATE}"; // Automatically attached at build

const char* client_key = "{CLIENT_KEY}"; // Automatically attached at build
