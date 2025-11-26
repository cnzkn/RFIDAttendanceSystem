using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CloudAPI.Models;

public class AttendanceLogModel
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    [Required]
    public Guid SectionId { get; set; }
    
    [Required]
    public Guid AttendeeId { get; set; }
    
    // TODO: Need to hold who marked the attendance (Device/Instructor)
    
    [Required]
    public bool IsPresent { get; set; }
    
    
    [ForeignKey("SectionId")] // C# auto-map
    public SectionModel Section { get; set; }
    
    [ForeignKey("AttendeeId")] // C# auto-map
    public AttendeeModel Attendee { get; set; }
}
