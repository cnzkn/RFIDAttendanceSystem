namespace CloudProject.Business.Dto;

public record AttendanceHistorySessionDto(int WeekNumber, TimeslotModel Timeslot, Dictionary<AttendanceRegisterType, List<AttendeeDto>> Attendance);

public class AttendanceHistoryDto
{
    public SectionDto Section { get; set; }
    public List<AttendanceHistorySessionDto> Sessions { get; set; }
}
