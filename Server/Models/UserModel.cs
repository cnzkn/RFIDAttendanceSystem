namespace CloudAPI.Models;

public enum UserRole
{
    Instructor,
    Administrator
}

public class UserModel : IdentityUser<Guid>, IAttendanceRegistrar
{
    public UserRole Role { get; set; }
}
