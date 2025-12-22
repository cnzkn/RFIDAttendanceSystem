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

    [HttpGet("{courseCode}/{section}")]
    public async Task<IActionResult> GetHistory(int courseCode, string section, CancellationToken token)
    {
        var course = await _courseManager.GetByCodeAsync(courseCode, token);
        if (course?.Id == null)
        {
            return NotFound($"Course code '{courseCode}' not found.");
        }

        var history = await _attendanceManager.GetAttendanceHistoryAsync(course.Id.Value, token);
        
        // Filter by section
        var filteredHistory = history.Where(h => h.Section.Section == section).ToList();
        
        return Ok(filteredHistory);
    }
    
    [HttpGet("{courseCode}/{section}/csv")]
    public async Task<IActionResult> ExportHistory(int courseCode, string section, CancellationToken token)
    {
        var course = await _courseManager.GetByCodeAsync(courseCode, token);
        if (course?.Id == null)
        {
            return NotFound($"Course code '{courseCode}' not found.");
        }

        var history = await _attendanceManager.GetAttendanceHistoryAsync(course.Id.Value, token);
        
        // Filter by section
        var filteredHistory = history.Where(h => h.Section.Section == section).ToList();
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Date,CourseId,CourseName,WeekNumber,TimeslotDay,TimeslotTime,StudentId,StudentName,Present");
        
        foreach (var sectionHistory in filteredHistory)
        {
            foreach (var timetable in sectionHistory.Timetables)
            {
                foreach (var session in timetable.Sessions)
                {
                    foreach (var student in sectionHistory.Students)
                    {
                        if (student.Id == null) continue;
                        
                        var present = session.Attendance.ContainsKey(AttendanceRegisterType.Present) && 
                                      session.Attendance[AttendanceRegisterType.Present].Contains(student.Id.Value);
                        
                        sb.AppendLine($"{course.Id},{course.Name},{session.WeekNumber},{timetable.Timeslot.DayOfWeek},{timetable.Timeslot.TimeslotNumber},{student.StudentID},{student.FullName},{present}");
                    }
                }
            }
        }
        
        return File(sb.ToString(), "text/csv", $"{course.Name}_{section}_AttendanceHistory.csv");
    }

    [HttpPost("{courseCode}/{section}")]
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