namespace CloudAPI.Models;

public class DeviceModel : IAttendanceRegistrar
{
    /// <summary>
    /// Unique identifier of this device.
    /// </summary>
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    /// <summary>
    /// ID of the classroom that this device is assigned to.
    /// </summary>
    [Required]
    public Guid AssignedClassroomId { get; set; }
    
    /// <summary>
    /// Certificate fingerprint of the device.
    /// </summary>
    [Required]
    public byte[] Fingerprint { get; set; }
    
    
    [ForeignKey(nameof(AssignedClassroomId))] // C# auto-map
    public virtual ClassroomModel AssignedClassroom { get; set; }
}
