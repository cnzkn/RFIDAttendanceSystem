using CloudProject.Business.Dto;

namespace CloudProject.API.Controllers;

[ApiController]
[Route("[controller]")]
public class AttendanceController : ControllerEx
{
    private readonly ILogger<AttendanceController> _logger;
    private readonly AttendanceManager _attendanceManager;
    private readonly TimetableManager _timetableManager;
    
    public AttendanceController(ILogger<AttendanceController> logger, AttendanceManager attendanceManager, TimetableManager timetableManager)
    {
        _logger = logger;
        _attendanceManager = attendanceManager;
        _timetableManager = timetableManager;
    }

    [Authorize, HttpPatch]
    public async Task<IActionResult> UpdateAttendanceRecord([FromBody]AttendanceUpdateRequestDto request, CancellationToken token)
    {
        try
        {
            await _attendanceManager.UpdateAttendanceAsync(GetCurrentUserId().Value, request, token);
            return Accepted();
        }
        catch (ObjectNotFoundException oex)
        {
            return NotFound(oex.Message);
        }
        catch (InvalidOperationException iex)
        {
            return BadRequest(iex.Message);
        }
    }
}
