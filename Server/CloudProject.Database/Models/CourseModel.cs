namespace CloudProject.Database.Models;

public class CourseModel : IEntity
{
    /// <inheritdoc />
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    /// <summary>
    /// Code for this course.
    /// </summary>
    [Required, Range(0, 134217727)] // 27-bits
    public int Code { get; set; }
    
    /// <summary>
    /// Title of this course.
    /// </summary>
    [Required, MaxLength(256)]
    public string Name { get; set; }
}
