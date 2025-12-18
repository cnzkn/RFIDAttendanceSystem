namespace CloudProject.Database.Models;

public class ClassroomModel : IEntity
{
    /// <inheritdoc />
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    /// <summary>
    /// Name of this classroom.
    /// </summary>
    [Required, MaxLength(32)]
    public string Name { get; set; }
}
