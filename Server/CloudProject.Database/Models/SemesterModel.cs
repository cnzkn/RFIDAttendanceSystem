namespace CloudProject.Database.Models;

public class SemesterModel : IEntity
{
    /// <inheritdoc />
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    /// <summary>
    /// Name of this semester.
    /// </summary>
    [Required, MaxLength(50)]
    public string Name { get; set; }
    
    /// <summary>
    /// Date when this semester starts.
    /// </summary>
    [Required]
    public DateOnly StartDate { get; set; }
    
    /// <summary>
    /// Date when this semester ends.
    /// </summary>
    [Required]
    public DateOnly EndDate { get; set; }
    
    /// <summary>
    /// List of course IDs associated with this semester.
    /// </summary>
    [Required]
    public List<Guid> CourseIds { get; set; }
    
    
    [ForeignKey(nameof(CourseIds))] // C# auto-map
    public virtual List<CourseModel> Courses { get; set; }
}
