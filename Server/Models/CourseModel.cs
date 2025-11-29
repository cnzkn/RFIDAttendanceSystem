namespace CloudAPI.Models;

public class CourseModel
{
    /// <summary>
    /// Unique identifier of this model.
    /// </summary>
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


    public CourseDto ToDto()
    {
        return new CourseDto()
        {
            Code = Code,
            Name = Name
        };
    }
}
