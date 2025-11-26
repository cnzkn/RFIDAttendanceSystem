using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CloudAPI.Models;

public class SectionModel
{
    [Required] // TODO: Map as composite key with property below.
    public Guid CourseId { get; set; }
    
    [Required] // TODO: Map as composite key with property above.
    public int SectionID { get; set; }
    
    [Required]
    public List<Guid> AttendeeIds { get; set; }
    
    
    [ForeignKey("CourseId")]
    public CourseModel Course { get; set; }
    
    [ForeignKey("AttendeeIds")]
    public List<AttendeeModel> Attendees { get; set; }
}
