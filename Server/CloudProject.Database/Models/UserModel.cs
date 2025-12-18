namespace CloudProject.Database.Models;

public class UserModel : IdentityUser<Guid>, IEntity, IAttendanceRegistrar
{
    /// <summary>
    /// Full name of this user.
    /// </summary>
    [Required, MaxLength(256)]
    public string FullName { get; set; }
    
    /// <summary>
    /// Role of this user.
    /// </summary>
    [Required]
    public UserRole Role { get; set; }
}
