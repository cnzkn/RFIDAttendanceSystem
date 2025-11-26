using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CloudAPI.Models;

public class DeviceModel
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    public Guid AssignedClassroomId { get; set; }
    
    [Required]
    public byte[] Fingerprint { get; set; }
    
    
    [ForeignKey("AssignedClassroomId")] // C# auto-map
    public ClassroomModel AssignedClassroom { get; set; }
}
