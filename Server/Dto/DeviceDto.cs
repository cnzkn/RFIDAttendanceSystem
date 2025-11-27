namespace CloudAPI.Dto;

public class DeviceDto
{
    public Guid Id { get; set; }
    public ClassroomDto Classroom { get; set; }
    public string Fingerprint { get; set; }
}
