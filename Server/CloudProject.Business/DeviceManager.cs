namespace CloudProject.Business;

public class DeviceManager
{
    private readonly ILogger<DeviceManager> _logger;
    private readonly IRepository<DeviceModel> _deviceRepository;
    private readonly ClassroomManager _classroomManager;

    public DeviceManager(ILogger<DeviceManager> logger, IRepository<DeviceModel> deviceRepository, ClassroomManager classroomManager)
    {
        _logger = logger;
        _deviceRepository = deviceRepository;
        _classroomManager = classroomManager;
    }

    public async Task<List<DeviceDto>> GetAllDevicesAsync(CancellationToken token = default)
    {
        var devices = await _deviceRepository.GetAllAsync(token);
        return devices.Select(x => x.ToDto()).ToList();
    }

    public async Task<DeviceDto?> GetDeviceByIdAsync(Guid id, CancellationToken token = default)
    {
        var device = await _deviceRepository.GetByIdAsync(id, token);
        return device?.ToDto();
    }
    
    internal async Task<DeviceModel?> FindByFingerprintAsync(byte[] fingerprint, CancellationToken token = default)
    {
        return await _deviceRepository.FirstOrDefaultAsync(x => fingerprint.SequenceEqual(x.Fingerprint), token);
    }
    
    public async Task<DeviceDto> CreateDeviceAsync(Guid classroomId, byte[] fingerprint, CancellationToken token = default)
    {
        if (await _deviceRepository.FirstOrDefaultAsync(x => fingerprint.SequenceEqual(x.Fingerprint), token) is { } device)
        {
            throw new InvalidOperationException("Device with the same fingerprint already exists.");
        }

        if (await _classroomManager.InternalGetClassroomByIdAsync(classroomId, token) is not { } classroom)
        {
            throw new ObjectNotFoundException("Classroom could not be found.");
        }
        
        device = new DeviceModel
        {
            AssignedClassroomId = classroomId,
            Fingerprint = fingerprint,
        };

        await _deviceRepository.AddAsync(device, token);
        _logger.LogInformation("Created new device {DeviceId} for classroom {Classroom}.", device.Id, classroom.Name);

        return device.ToDto();
    }

    public async Task<DeviceDto> UpdateDeviceAsync(byte[] fingerprint, UpdateDeviceRequestDto request, CancellationToken token = default)
    {
        if (await _deviceRepository.FirstOrDefaultAsync(x => fingerprint.SequenceEqual(x.Fingerprint), token) is not { } existingDevice)
        {
            throw new ObjectNotFoundException("Device not found.");
        }

        if (request.NewFingerprint is not null)
        {
            existingDevice.Fingerprint = request.NewFingerprint;   
        }

        if (request.NewClassroomId.HasValue)
        {
            var val = request.NewClassroomId.Value;
            if (await _classroomManager.InternalGetClassroomByIdAsync(val, token) is not { } classroom)
            {
                throw new ObjectNotFoundException("Classroom could not be found.");
            }
            
            existingDevice.AssignedClassroomId = classroom.Id;
        }
        
        await _deviceRepository.UpdateAsync(existingDevice, token);
        _logger.LogInformation("Updated device fingerprint of {DeviceId}.", existingDevice.Id);

        return existingDevice.ToDto();
    }
    
    public async Task DeleteDeviceAsync(byte[] fingerprint, CancellationToken token = default)
    {
        if (await _deviceRepository.FirstOrDefaultAsync(x => fingerprint.SequenceEqual(x.Fingerprint), token) is not { } existingDevice)
        {
            throw new ObjectNotFoundException("Device not found.");
        }

        await _deviceRepository.RemoveAsync(existingDevice, token);
        _logger.LogInformation("Deleted device {DeviceId}.", existingDevice.Id);
    }
}
