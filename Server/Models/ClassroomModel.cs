namespace CloudAPI.Models;

public class ClassroomModel
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    public string Name { get; set; }
}
