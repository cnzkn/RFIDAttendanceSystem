namespace CloudProject.Business.Dto;

public class CourseDto
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? Id { get; set; }
    
    public int Code { get; set; }
    public string Name { get; set; }
}
