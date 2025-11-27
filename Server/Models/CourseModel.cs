namespace CloudAPI.Models;

public class CourseModel
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    public int Code { get; set; }
    
    [Required]
    public string Name { get; set; }
}
