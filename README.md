# RFID-based Attendance System
Source code for the RFID-based Attendance System project. The project aims to simplify and automate the process of taking attendance using student ID cards with an NFC reader device, a cloud server to process data, and two distinct web-based user interfaces for instructors and system administrators.  
This project is developed by the students of Middle East Technical University, Northern Cyprus Campus on 2025-2026 Fall Semester for CNG 495 Cloud Computing course.



# Components

## Device
Contains the source code for our hardware, responsible for scanning ID cards and transmit information to the server.


## Server
Source code for our back-end service. Communicates with devices, provides an API for our front-end and coordinates project functionality. 


## Frontend
Source code for the user interface of the project. Offers real-time attendance status along with administrator & instructor interfaces.
Live deployed at https://umutsen2662.github.io/RFIDAttendanceSystemPages/


## Documents
Sources for the reports we've written and submitted. Each document has its own sub-folder in the folder.



# Building

## Device
This project requires **Arduino IDE**.
Please visit https://www.arduino.cc/en/software/ to download the IDE.

### Compiling
Following libraries are required to compile (Verify) the project:
- https://github.com/espressif/arduino-esp32
- https://github.com/johnrickman/LiquidCrystal_I2C
- https://github.com/miguelbalboa/rfid
- https://github.com/Links2004/arduinoWebSockets

You also need to generate a CA certificate, device certificate and a private key for the module and include them in "Cert.cpp". Using OpenSSL, following commands generate all the necessary files. Note that commands may ask for additional information.
```
openssl genrsa -aes256 -out ca.key 4096
openssl req -x509 -new -key ca.key -days 7300 -out ca.crt
openssl req -new -nodes -out esp32-certificate.csr -newkey rsa:4096 -keyout esp32-certificate.key
openssl x509 -req -in esp32-certificate.csr -CA ca.crt -CAkey ca.key -CAcreateserial -out ca.crt -days 3650 -sha256
```
Then from generated files using commands above, contents of `ca.crt` should be added as `root_ca` to "Cert.cpp" as a string. Similarly, `esp32-certificate.crt` should be added as `client_cert` and `esp32-certificate.key` as `client_key` in "Cert.cpp". In Linux based systems, this can be done using `xxd -i (file name)` in a terminal and appending a null (0x00) character at the end of the generated array. Additionally, server's certificate CA root must also be added as `root_ca_server` for server certificate validation, otherwise WebSocket connection will fail.

After completing steps above, simply click "Verify" button to compile the project.
To install the project on a real hardware, you will need an ESP32 board based on a ESP32-WROOM-32 SoC module. In the demonstration, an ESP32-DevKit-V1 was used.


## Server
This project requires **ASP.NET Runtime** with **.NET 10**.  
Please refer to https://learn.microsoft.com/en-us/dotnet/core/install/ if it's not installed already.

### Compiling
After cloning the project, open a terminal in "Server" directory, and run the following command to produce binaries:
```
dotnet publish -o ./PublishDirectory
```
Where "PublishDirectory" is the folder where you'd like the binaries to be placed.

### Running
To run the published application, navigate to the directory as you specified above, and:
- **on Windows**: Run CloudProject.API.exe
- **on Linux**: In a terminal window, run `dotnet CloudProject.API.dll` or `./CloudProject.API`.

The application requires following environment variables to be set in order to perform correctly, which can be defined in an `.env` file in the same directory as the compiled binary or OS settings:
- **DB_HOST**: Location of a PostgreSQL database. Can be an IP address or a host name.
- **DB_PORT**: Port on which database is accessible.
- **DB_USER**: User name for database access.
- **DB_PASS**: Password for database authorization.
- **DB_NAME**: Name of the database to be used.
- **SEED_ADMIN_USERNAME**: User name of the default administrator account. This user has access to everything on the system, such as managing devices.
- **SEED_ADMIN_PASSWORD**: Password for the default administrator account.
- **SEED_INSTRUCTOR_USERNAME**: User name of the default instructor. All course sections will be assigned to this instructor.
- **SEED_INSTRUCTOR_PASSWORD**: Password for the default instructor.
- **SEED_CLASSROOMS**: A JSON string mapping from int to string, where int is the classroom ID, and string is the classroom name.
- **SEED_TIMETABLES**: An array of JSON objects that have following properties: day, time and course.
- **SEED_SEMESTER_NAME**: Name of the semester. 
- **SEED_SEMESTER_START**: Starting date of the semester in Unix timestamp seconds.
- **SEED_SEMESTER_END**: Ending date of the semester in Unix timestamp seconds.

The application does not support HTTPS, but is designed to live behind a proxy. So, it must be proxied through a web server such as nginx or Caddy, and the application requires client certificate fingerprints to be passed in request header with the key `X-Client-Cert-Fingerprint` so that devices can be verified.


## Frontend
This project requires **Node.js** and **npm**. 
If you don't already have them installed, download and install Node.js (which includes npm) from the official website: https://nodejs.org/

### Running the Application
After installing the dependencies, start the development server with:
```
npm install
npm run dev
```

### Building For Production
To build this application for production:
```bash
npm run build
```


## Documents
Sources for the documents were imported to our repository from [Overleaf](https://www.overleaf.com), which is one of the simplest platforms to work with LaTeX. It can be used to compile the document sources to PDFs. Note that an account is needed to use the platform.
