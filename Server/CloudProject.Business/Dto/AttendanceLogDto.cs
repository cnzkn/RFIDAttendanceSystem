namespace CloudProject.Business.Dto;

public class AttendanceLogDto
{
    public Guid? Id { get; set; }
    public DateTime Date { get; set; }
    public int WeekNumber { get; set; }
    public AttendeeDto Attendee { get; set; }
    public TimetableDto Timetable { get; set; }
    public IAttendanceRegistrar Registrar { get; set; }
    public bool IsPresent { get; set; }
}
