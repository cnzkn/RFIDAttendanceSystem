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
    [Required]
    public int Code { get; set; }
    
    /// <summary>
    /// Title of this course.
    /// </summary>
    [Required]
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
