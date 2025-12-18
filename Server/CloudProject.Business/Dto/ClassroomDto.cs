namespace CloudProject.Business.Dto;

public class ClassroomDto
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? Id { get; set; }
    
    public string Name { get; set; }
}
