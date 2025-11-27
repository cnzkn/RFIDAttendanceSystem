namespace CloudAPI.Models;

public class TimetableModel
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    public Guid SectionId { get; set; }
    
    [Required]
    public Guid ClassroomId { get; set; }
    
    [Required]
    public TimeslotModel Timeslot { get; set; } // Embedded object
    
    
    [ForeignKey(nameof(SectionId))] // C# auto-map
    public virtual SectionModel CourseSection { get; set; }
    
    [ForeignKey(nameof(ClassroomId))] // C# auto-map
    public virtual ClassroomModel Classroom { get; set; }
}
