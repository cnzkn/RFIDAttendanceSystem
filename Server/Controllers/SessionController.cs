using System.Globalization;

namespace CloudAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class SessionController : Controller
{
    private readonly DatabaseContext _context;
    private readonly UserManager<UserModel> _userManager;

    public SessionController(DatabaseContext context, UserManager<UserModel> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public class UpdateRoomRequest
    {
        public string RoomName { get; set; }
    }

    [Authorize, HttpPost("update-room/{sessionId}")]
    public async Task<IActionResult> UpdateSessionRoom(Guid sessionId, [FromBody] UpdateRoomRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        // 1. Find the Session (Timetable)
        var timetable = await _context.Timetables
            .Include(t => t.CourseSection)
            .FirstOrDefaultAsync(t => t.Id == sessionId);

        if (timetable == null) return NotFound("Session not found.");

        // 2. Check Authorization (Must be the instructor)
        if (timetable.CourseSection.UserId != Guid.Parse(userId))
        {
            return Forbid("You are not the instructor for this session.");
        }

        // 3. Find the New Classroom
        var newClassroom = await _context.Classrooms
            .FirstOrDefaultAsync(c => c.Name.ToLower() == request.RoomName.ToLower());

        if (newClassroom == null)
        {
            return NotFound($"Classroom '{request.RoomName}' not found.");
        }

        // 4. Update and Save
        timetable.ClassroomId = newClassroom.Id;
        await _context.SaveChangesAsync();

        return Ok();
    }

    [Authorize, HttpGet("{id}")]
    public async Task<IActionResult> GetSession(Guid id)
    {
        var timetable = await _context.Timetables
            .Include(t => t.CourseSection)
                .ThenInclude(s => s.Course)
            .Include(t => t.Classroom)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (timetable == null)
        {
            return NotFound("Session (Timetable) not found.");
        }

        var today = DateTime.Now;

        // Calculate the actual date of the session for this week
        int daysSinceMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var thisWeekMonday = today.Date.AddDays(-daysSinceMonday);
        int targetDayOffset = ((int)timetable.Timeslot.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var sessionDate = thisWeekMonday.AddDays(targetDayOffset);
        
        // Calculate Week of Year (Semester Week) using the actual session date
        var weekNum = SemesterConfig.GetSemesterWeek(sessionDate);

        var dto = new SessionDto
        {
            AttendanceSessionId = timetable.Id,
            CourseName = timetable.CourseSection.Course.Name,
            CourseCode = timetable.CourseSection.Course.Code.ToString(),
            Section = timetable.CourseSection.SectionType + timetable.CourseSection.SectionId,
            Date = sessionDate,
            Room = timetable.Classroom.Name,
            Week = weekNum,
            Day = (int)timetable.Timeslot.DayOfWeek,
            StartHour = timetable.Timeslot.TimeslotNumber
        };

        return Ok(dto);
    }
}
