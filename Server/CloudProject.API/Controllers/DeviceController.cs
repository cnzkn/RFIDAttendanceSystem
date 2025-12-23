namespace CloudProject.API.Controllers;

[ApiController]
[Route("[controller]")]
public class DeviceController : ControllerEx
{
    private readonly ILogger<DeviceController> _logger;
    private readonly DeviceManager _deviceManager;
    private readonly IModuleHandler _moduleHandler;

    public DeviceController(ILogger<DeviceController> logger, DeviceManager deviceManager, IModuleHandler moduleHandler)
    {
        _logger = logger;
        _deviceManager = deviceManager;
        _moduleHandler = moduleHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDevices(CancellationToken token)
    {
        var devices = await _deviceManager.GetAllDevicesAsync(token);
        
        foreach (var device in devices)
        {
            try 
            {
                // Convert Base64 fingerprint to Hex for lookup
                var bytes = Convert.FromBase64String(device.Fingerprint);
                var hex = Convert.ToHexString(bytes);
                device.IsOnline = _moduleHandler.IsConnected(hex);
            }
            catch
            {
                device.IsOnline = false;
            }
        }

        return Ok(devices);
    }

    [HttpPost]
    public async Task<IActionResult> CreateDevice([FromBody] CreateUpdateDeviceRequestDto request, CancellationToken token)
    {
        try
        {
            // Fingerprint is expected to be a Base64 string
            var fingerprint = Convert.FromBase64String(request.Fingerprint);
            var device = await _deviceManager.CreateDeviceAsync(request.ClassroomId, fingerprint, token);
            return Ok(device);
        }
        catch (FormatException)
        {
            return BadRequest("Invalid fingerprint format. Expected Base64 string.");
        }
        catch (ObjectNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDevice(Guid id, [FromBody] UpdateDeviceRequestDto request, CancellationToken token)
    {
        var deviceDto = await _deviceManager.GetDeviceByIdAsync(id, token);
        if (deviceDto == null)
        {
            return NotFound("Device not found.");
        }

        try
        {
            // We need the original fingerprint to identify the device in Manager
            var fingerprint = Convert.FromBase64String(deviceDto.Fingerprint);
            var result = await _deviceManager.UpdateDeviceAsync(fingerprint, request, token);
            return Ok(result);
        }
        catch (ObjectNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (FormatException)
        {
             // Should not happen if DB data is valid Base64, but good to catch
             return StatusCode(500, "Stored fingerprint format is invalid.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDevice(Guid id, CancellationToken token)
    {
        var deviceDto = await _deviceManager.GetDeviceByIdAsync(id, token);
        if (deviceDto == null)
        {
            return NotFound("Device not found.");
        }

        try
        {
            var fingerprint = Convert.FromBase64String(deviceDto.Fingerprint);
            await _deviceManager.DeleteDeviceAsync(fingerprint, token);
            return NoContent();
        }
        catch (ObjectNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
