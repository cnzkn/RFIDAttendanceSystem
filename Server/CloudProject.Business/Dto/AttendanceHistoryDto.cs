namespace CloudProject.Business.Dto;

public record AttendanceHistorySessionDto(int WeekNumber, Dictionary<AttendanceRegisterType, List<Guid>> Attendance);

public class AttendanceTimetableDto
{
    public Guid Id { get; set; }
    public TimeslotModel Timeslot { get; set; }
    public List<AttendanceHistorySessionDto> Sessions { get; set; }
}

public class AttendanceHistoryDto
{
    public SectionDto Section { get; set; }
    public List<AttendeeDto> Students { get; set; }
    public List<AttendanceTimetableDto> Timetables { get; set; }
}
