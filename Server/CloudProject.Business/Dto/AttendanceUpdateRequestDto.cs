namespace CloudProject.Business.Dto;

public class AttendanceUpdateRequestDto
{
    public Guid TimetableId { get; set; }
    public Guid AttendeeId { get; set; }
    public AttendanceRegisterType Status { get; set; }
}
