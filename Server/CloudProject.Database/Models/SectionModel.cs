namespace CloudProject.Database.Models;

public class SectionModel : IEntity
{
    /// <inheritdoc />
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    /// <summary>
    /// ID of the user attending this section as a lecturer.
    /// </summary>
    [Required]
    public Guid UserId { get; set; }
    
    /// <summary>
    /// ID of the course of this section.
    /// </summary>
    [Required]
    public Guid CourseId { get; set; }
    
    /// <summary>
    /// ID of this section.
    /// </summary>
    [Required, Range(1, 100)]
    public int SectionId { get; set; }
    
    /// <summary>
    /// Type of this section, such as a lecture, laboratory, recitation.
    /// </summary>
    [Required, MaxLength(32)]
    public string SectionType { get; set; }
    
    /// <summary>
    /// IDs of all attendees attending this section.
    /// </summary>
    [Required]
    public List<Guid> AttendeeIds { get; set; }
    
    
    [ForeignKey(nameof(UserId))] // C# auto-map
    public virtual UserModel User { get; set; }
    
    [ForeignKey(nameof(CourseId))] // C# auto-map
    public virtual CourseModel Course { get; set; }
    
    [ForeignKey(nameof(AttendeeIds))] // C# auto-map
    public virtual List<AttendeeModel> Attendees { get; set; }
}
