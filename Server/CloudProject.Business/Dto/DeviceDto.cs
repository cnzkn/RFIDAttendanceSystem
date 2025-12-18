namespace CloudProject.Business.Dto;

public class DeviceDto
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? Id { get; set; }
    
    public ClassroomDto Classroom { get; set; }
    public string Fingerprint { get; set; }
}
