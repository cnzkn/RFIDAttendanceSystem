namespace CloudProject.Business.Dto;

public class TimetableDto
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? Id { get; set; }
    
    public SectionDto Section { get; set; }
    public ClassroomDto Classroom { get; set; }
    public TimeslotModel Timeslot { get; set; }
}
