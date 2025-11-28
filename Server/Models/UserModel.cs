namespace CloudAPI.Models;

public enum UserRole
{
    Instructor,
    Administrator
}

public class UserModel : IdentityUser<Guid>, IAttendanceRegistrar
{
    /// <summary>
    /// Full name of this user.
    /// </summary>
    [Required]
    public string FullName { get; set; }
    
    /// <summary>
    /// Role of this user.
    /// </summary>
    [Required]
    public UserRole Role { get; set; }

    
    public UserDto ToDto()
    {
        return new UserDto()
        {
            UserName = UserName,
            Role = Role
        };
    }
}
