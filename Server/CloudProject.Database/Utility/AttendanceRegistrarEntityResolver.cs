namespace CloudProject.Database.Utility;

public class AttendanceRegistrarEntityResolver : IEntityResolver<IAttendanceRegistrar, Guid>
{
    private readonly DatabaseContext _context;
    
    public AttendanceRegistrarEntityResolver(DatabaseContext context)
    {
        _context = context;
    }

    public (Guid, string) GetProperties(IAttendanceRegistrar instance)
    {
        switch (instance)
        {
            case DeviceModel device:
                return (device.Id, nameof(DeviceModel));
            
            case UserModel user:
                return (user.Id, nameof(UserModel));
            
            default:
                throw new InvalidOperationException($"Requested type {instance.GetType()} does not match any known types.");
        }
    }

    public async Task<IAttendanceRegistrar?> ResolveAsync(Guid key, string type)
    {
        switch (type)
        {
            case nameof(UserModel):
                return await _context.Users.FindAsync(key);
            
            case nameof(DeviceModel):
                return await _context.Devices.FindAsync(key);
            
            default:
                throw new InvalidOperationException($"Requested type {type} does not match any known types.");
        }
    }
}
