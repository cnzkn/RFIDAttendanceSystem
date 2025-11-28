namespace CloudAPI.Models;

public class ClassroomModel
{
    /// <summary>
    /// Unique identifier of this model.
    /// </summary>
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    /// <summary>
    /// Name of this classroom.
    /// </summary>
    [Required]
    public string Name { get; set; }


    public ClassroomDto ToDto()
    {
        return new ClassroomDto()
        {
            Name = Name
        };
    }
}
