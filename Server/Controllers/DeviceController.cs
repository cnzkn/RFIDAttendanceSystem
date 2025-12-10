namespace CloudAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = "Administrator")]
public class DeviceController : Controller
{
    private readonly DatabaseContext _context;

    public DeviceController(DatabaseContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var devices = await _context.Devices
            .Include(d => d.AssignedClassroom)
            .ToListAsync();
            
        return Ok(devices.Select(d => d.ToDto()));
    }

    public class CreateDeviceRequest
    {
        public string Fingerprint { get; set; }
        public Guid ClassroomId { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDeviceRequest request)
    {
        byte[] fingerprintBytes;
        try
        {
            fingerprintBytes = Convert.FromHexString(request.Fingerprint);
        }
        catch
        {
            return BadRequest("Invalid fingerprint format. Must be a valid Hex string.");
        }

        var classroom = await _context.Classrooms.FindAsync(request.ClassroomId);
        if (classroom == null) return NotFound("Classroom not found.");

        if (await _context.Devices.AnyAsync(d => d.Fingerprint == fingerprintBytes))
        {
            return BadRequest("Device with this fingerprint already exists.");
        }

        var device = new DeviceModel
        {
            Fingerprint = fingerprintBytes,
            AssignedClassroomId = request.ClassroomId
        };

        await _context.Devices.AddAsync(device);
        await _context.SaveChangesAsync();

        // Reload to get navigation property
        await _context.Entry(device).Reference(d => d.AssignedClassroom).LoadAsync();

        return Ok(device.ToDto());
    }

    public class UpdateDeviceRequest
    {
        public string Fingerprint { get; set; }
        public Guid ClassroomId { get; set; }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDeviceRequest request)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device == null) return NotFound("Device not found.");

        byte[] fingerprintBytes;
        try
        {
            fingerprintBytes = Convert.FromHexString(request.Fingerprint);
        }
        catch
        {
            return BadRequest("Invalid fingerprint format. Must be a valid Hex string.");
        }

        // Check uniqueness if fingerprint changed
        if (!device.Fingerprint.SequenceEqual(fingerprintBytes))
        {
            if (await _context.Devices.AnyAsync(d => d.Fingerprint == fingerprintBytes))
            {
                return BadRequest("Another device with this fingerprint already exists.");
            }
        }

        var classroom = await _context.Classrooms.FindAsync(request.ClassroomId);
        if (classroom == null) return NotFound("Classroom not found.");

        device.Fingerprint = fingerprintBytes;
        device.AssignedClassroomId = request.ClassroomId;

        await _context.SaveChangesAsync();
        await _context.Entry(device).Reference(d => d.AssignedClassroom).LoadAsync();

        return Ok(device.ToDto());
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device == null) return NotFound();

        _context.Devices.Remove(device);
        await _context.SaveChangesAsync();

        return Ok();
    }
}
