namespace CloudAPI.Dto;

public class TimetableDto
{
    public Guid Id { get; set; }
    public SectionDto Section { get; set; }
    public ClassroomDto Classroom { get; set; }
    public TimeslotModel Timeslot { get; set; }
}
