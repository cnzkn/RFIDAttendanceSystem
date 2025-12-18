namespace CloudProject.Core;

public enum AttendanceStatus
{
    /// <summary>
    /// Attendance registered successfully.
    /// </summary>
    Success,
    
    /// <summary>
    /// The card for this session is already registered.
    /// </summary>
    AlreadyScanned,
    
    /// <summary>
    /// There's no upcoming/current lecture in the classroom.
    /// </summary>
    NoLecture,
    
    /// <summary>
    /// Student not registered in current lecture.
    /// </summary>
    NotRegistered,
    
    /// <summary>
    /// Provided ID card is not recognized.
    /// </summary>
    UnrecognizedId,
    
    /// <summary>
    /// An error occurred during attendance registration.
    /// </summary>
    Error
}
