namespace CloudProject.Business.Dto;

public class CreateUpdateDeviceRequestDto
{
    public string Fingerprint { get; set; }
    public Guid ClassroomId { get; set; }
}
