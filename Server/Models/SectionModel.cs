namespace CloudAPI.Models;

public class SectionModel
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    public Guid CourseId { get; set; }
    
    [Required]
    public int SectionId { get; set; }
    
    [Required]
    public List<Guid> AttendeeIds { get; set; }
    
    
    [ForeignKey(nameof(CourseId))]
    public virtual CourseModel Course { get; set; }
    
    [ForeignKey(nameof(AttendeeIds))]
    public virtual List<AttendeeModel> Attendees { get; set; }
}
