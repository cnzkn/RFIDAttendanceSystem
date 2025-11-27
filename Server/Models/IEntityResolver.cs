namespace CloudAPI.Models;

public interface IEntityResolver<T, K>
{
    Task<T?> ResolveAsync(K key, string type);
}

public class AttendanceRegistrarEntityResolver : IEntityResolver<IAttendanceRegistrar, Guid>
{
    private readonly DatabaseContext _context;
    
    public AttendanceRegistrarEntityResolver(DatabaseContext context)
    {
        _context = context;
    }
    
    public async Task<IAttendanceRegistrar?> ResolveAsync(Guid key, string type)
    {
        switch (type)
        {
            case "User":
                return await _context.Users.FindAsync(key);
            
            case "Device":
                return await _context.Devices.FindAsync(key);
            
            default:
                throw new InvalidOperationException($"Requested type {type} does not match any known types.");
        }
    }
}
