namespace CloudAPI.Dto;

public enum AttendanceRegisterType
{
    Absent,
    Present
}

public class AttendanceRegisterDto
{
    public AttendeeDto Attendee { get; set; }
    
    public AttendanceRegisterType Attendance { get; set; }
    
    [JsonConverter(typeof(TimeSpanJsonSerializer))]
    public TimeSpan? Offset { get; set; }
}
