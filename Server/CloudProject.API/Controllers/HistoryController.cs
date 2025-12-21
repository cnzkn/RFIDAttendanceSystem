using CloudProject.Business;
using CloudProject.Business.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudProject.API.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class HistoryController : ControllerEx
{
    private readonly ILogger<HistoryController> _logger;
    private readonly CourseManager _courseManager;
    private readonly AttendanceManager _attendanceManager;

    public HistoryController(ILogger<HistoryController> logger, CourseManager courseManager, AttendanceManager attendanceManager)
    {
        _logger = logger;
        _courseManager = courseManager;
        _attendanceManager = attendanceManager;
    }

    [HttpGet("/Attendance/history/{courseCode}/{section}")]
    public async Task<IActionResult> GetHistory(int courseCode, string section, CancellationToken token)
    {
        var course = await _courseManager.GetByCodeAsync(courseCode, token);
        if (course == null || course.Id == null)
        {
            return NotFound($"Course code '{courseCode}' not found.");
        }

        var history = await _attendanceManager.GetAttendanceHistoryAsync(course.Id.Value, token);
        
        // Filter by section
        var filteredHistory = history.Where(h => h.Section.Section == section).ToList();
        
        return Ok(filteredHistory);
    }

    [HttpPost("/Attendance/history/update/{courseCode}/{section}")]
    public async Task<IActionResult> UpdateHistory(int courseCode, string section, [FromBody] BulkAttendanceUpdateRequestDto request, CancellationToken token)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        try
        {
            await _attendanceManager.UpdateAttendanceAsync(userId.Value, request.Updates, token);
            return Ok();
        }
        catch (ObjectNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}