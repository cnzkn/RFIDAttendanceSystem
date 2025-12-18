namespace CloudProject.Business.Dto;

public class UpdateDeviceRequestDto
{
    public byte[]? NewFingerprint { get; set; }
    public Guid? NewClassroomId { get; set; }
}
