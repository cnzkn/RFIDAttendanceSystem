namespace CloudProject.Business.Dto;

public class AttendeeDto
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? Id { get; set; }
    
    public int StudentID { get; set; }
    public string FullName { get; set; }
}
