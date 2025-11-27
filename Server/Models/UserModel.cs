namespace CloudAPI.Models;

public enum UserRole
{
    Instructor,
    Administrator
}

public class UserModel : IdentityUser<Guid>, IAttendanceRegistrar
{
    [Required]
    public string FullName { get; set; }
    
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
