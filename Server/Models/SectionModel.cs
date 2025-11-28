namespace CloudAPI.Models;

public class SectionModel
{
    /// <summary>
    /// Unique identifier of this model.
    /// </summary>
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
    [Required]
    public int SectionId { get; set; }
    
    /// <summary>
    /// Type of this section, such as a lecture, laboratory, recitation.
    /// </summary>
    [Required]
    public string SectionType { get; set; }
    
    /// <summary>
    /// IDs of all attendees attending this section.
    /// </summary>
    [Required]
    public List<Guid> AttendeeIds { get; set; }
    
    
    [ForeignKey(nameof(UserId))]
    public virtual UserModel User { get; set; }
    
    [ForeignKey(nameof(CourseId))]
    public virtual CourseModel Course { get; set; }
    
    [ForeignKey(nameof(AttendeeIds))]
    public virtual List<AttendeeModel> Attendees { get; set; }


    public SectionDto ToDto()
    {
        return new SectionDto()
        {
            Course =  Course.ToDto(),
            SectionID = SectionId
        };
    }
}
