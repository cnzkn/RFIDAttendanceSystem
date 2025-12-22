#pragma once

#define UTC_OFFSET_WINTER 7200
#define UTC_OFFSET_SUMMER 10800

#define MIN(a, b) (((a) < (b)) ? (a) : (b))

typedef enum : uint8_t {
    // Attendance registered successfully.
    SCAN_SUCCESS = 0,
    
    // The card for this session is already registered.
    SCAN_ALREADY_SCANNED = 1,
    
    // There's no upcoming/current lecture in the classroom.
    SCAN_NO_LECTURE = 2,
    
    // Student not registered in current lecture.
    SCAN_NOT_REGISTERED = 3,
    
    // Provided ID card is not recognized.
    SCAN_UNRECOGNIZED_ID = 4,
    
    // An error occurred during attendance registration.
    SCAN_ERROR = 5,

    SCAN_INVALID = 0xFF // Denotes an invalid value.
} AttendanceStatus;

void apiInit();
void apiTick();

void halt();

void serialPrint(const char* text, ...);

const char* getClassroom();
const char* getLecture();
void setClassroom(const char* classroom);
void setLecture(const char* lecture);
bool moduleDataChanged();
void clearModuleDataChanged();
bool hasScanResult();
AttendanceStatus getScanResult();
int getScanName(char* out);
void setScanResult(uint8_t status, char* name);
