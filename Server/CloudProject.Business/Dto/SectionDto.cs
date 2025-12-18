namespace CloudProject.Business.Dto;

public class SectionDto
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? Id { get; set; }
    
    public CourseDto Course { get; set; }
    public string Section { get; set; }
    public UserDto User { get; set; }
}
