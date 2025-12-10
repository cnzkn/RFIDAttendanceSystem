namespace CloudAPI.Dto;

public class SessionDto
{
    public Guid AttendanceSessionId { get; set; }
    public string CourseName { get; set; }
    public string CourseCode { get; set; }
    public string Section { get; set; }
    public DateTime Date { get; set; }
    public string Room { get; set; }
    public int Week { get; set; }
    public int Day { get; set; }
    public int StartHour { get; set; }
}
