using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CloudAPI.Models;

public class TimetableModel
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    public Guid CourseSectionId { get; set; }
    
    [Required]
    public Guid ClassroomId { get; set; }
    
    [Required]
    public TimeslotModel Timeslot { get; set; } // Embedded object
    
    
    [ForeignKey("CourseSectionId")] // C# auto-map
    public SectionModel CourseSection { get; set; }
    
    [ForeignKey("ClassroomId")] // C# auto-map
    public ClassroomModel Classroom { get; set; }
}
