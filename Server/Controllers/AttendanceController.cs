namespace CloudAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AttendanceController : Controller
{
    private readonly DatabaseContext _context;
    private readonly UserManager<UserModel> _userManager;
    private readonly IClientHandler _clientHandler;

    public AttendanceController(DatabaseContext context, UserManager<UserModel> userManager, IClientHandler clientHandler)
    {
        _context = context;
        _userManager = userManager;
        _clientHandler = clientHandler;
    }

    public class AttendanceUpdateRequest
    {
        public Guid SessionId { get; set; }
        public Guid StudentId { get; set; }
        public string Status { get; set; } // "present" or "absent"
    }

    [Authorize, HttpPost("update")]
    public async Task<IActionResult> UpdateAttendance([FromBody] AttendanceUpdateRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        // Validate Session (Timetable)
        var timetable = await _context.Timetables
            .Include(t => t.CourseSection)
            .FirstOrDefaultAsync(t => t.Id == request.SessionId);

        if (timetable == null) return NotFound("Session not found.");

        // Check Access (User must be the instructor of the section)
        if (timetable.CourseSection.UserId != Guid.Parse(userId))
        {
            return Forbid("You are not the instructor for this session.");
        }

        // Validate Student
        var attendee = await _context.Attendee.FindAsync(request.StudentId);
        if (attendee == null) return NotFound("Student not found.");

        // Update Logic
        var currentWeek = SemesterConfig.GetSemesterWeek(DateTime.Now);

        var existingLog = await _context.AttendanceLogs
            .FirstOrDefaultAsync(l => l.TimetableId == request.SessionId && 
                                      l.AttendeeId == request.StudentId && 
                                      l.WeekNumber == currentWeek);

        bool isPresent = request.Status.ToLower() == "present";

        if (existingLog != null)
        {
            existingLog.IsPresent = isPresent;
            existingLog.MarkedById = Guid.Parse(userId);
            existingLog.MarkedByType = "User"; // Manual update
            existingLog.Date = DateTime.UtcNow; // Update timestamp to now
        }
        else
        {
            var newLog = new AttendanceLogModel
            {
                TimetableId = request.SessionId,
                AttendeeId = request.StudentId,
                MarkedById = Guid.Parse(userId),
                MarkedByType = "User",
                IsPresent = isPresent,
                Date = DateTime.UtcNow,
                WeekNumber = currentWeek
            };
            await _context.AttendanceLogs.AddAsync(newLog);
        }

        await _context.SaveChangesAsync();

        // Broadcast update
        await _clientHandler.BroadcastUpdateAsync(request.SessionId, new
        {
            type = "student_updated",
            studentId = request.StudentId.ToString(),
            status = isPresent ? "present" : "absent",
            timestamp = DateTime.Now.ToString("o"),
            isManual = true
        });

        return Ok();
    }

    public class HistoryUpdateItem
    {
        public Guid StudentId { get; set; }
        public int Week { get; set; }
        public int Day { get; set; } // 1-based index matching column order
        public string Status { get; set; } // "present" or "absent"
    }

    public class HistoryUpdateRequest
    {
        public List<HistoryUpdateItem> Updates { get; set; } = new();
    }

    [Authorize, HttpPost("history/update/{courseCode}/{section}")]
    public async Task<IActionResult> UpdateCourseHistory(int courseCode, string section, [FromBody] HistoryUpdateRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        // 1. Resolve Section
        var userSections = await _context.Sections
            .Include(s => s.Course)
            .Where(s => s.UserId == Guid.Parse(userId!) && s.Course.Code == courseCode)
            .ToListAsync();

        var targetSection = userSections
            .FirstOrDefault(s => (s.SectionType + s.SectionId) == section);

        if (targetSection == null) return NotFound("Section not found or access denied.");

        // 2. Get Timetables to map DayIndex -> Timetable
        var timetables = await _context.Timetables
            .Where(t => t.SectionId == targetSection.Id)
            .ToListAsync();

        var sortedTimetables = timetables
            .OrderBy(t => t.Timeslot.DayOfWeek)
            .ThenBy(t => t.Timeslot.TimeslotNumber)
            .ToList(); // Index 0 is Day 1

        // 3. Process Updates
        var currentYear = DateTime.Now.Year;

        foreach (var update in request.Updates)
        {
            if (update.Day < 1 || update.Day > sortedTimetables.Count) continue; // Invalid day index

            var timetable = sortedTimetables[update.Day - 1];
            
            // Find existing log
            var log = await _context.AttendanceLogs
                .FirstOrDefaultAsync(l => l.TimetableId == timetable.Id &&
                                          l.AttendeeId == update.StudentId &&
                                          l.WeekNumber == update.Week);

            if (update.Status.ToLower() == "pending")
            {
                // Pending means "remove record"
                if (log != null)
                {
                    _context.AttendanceLogs.Remove(log);
                }
                continue;
            }

            bool isPresent = update.Status.ToLower() == "present";

            if (log != null)
            {
                if (log.IsPresent != isPresent)
                {
                    log.IsPresent = isPresent;
                    log.MarkedById = Guid.Parse(userId);
                    log.MarkedByType = "User";
                }
            }
            else
            {
                // Create new
                var logDate = System.Globalization.ISOWeek.ToDateTime(currentYear, SemesterConfig.GetIsoWeek(update.Week), timetable.Timeslot.DayOfWeek);
                logDate = DateTime.SpecifyKind(logDate, DateTimeKind.Utc); 

                log = new AttendanceLogModel
                {
                    TimetableId = timetable.Id,
                    AttendeeId = update.StudentId,
                    WeekNumber = update.Week,
                    MarkedById = Guid.Parse(userId),
                    MarkedByType = "User",
                    IsPresent = isPresent,
                    Date = logDate
                };
                await _context.AttendanceLogs.AddAsync(log);
            }
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    [Authorize, HttpGet("history/{courseCode}/{section}")]
    public async Task<IActionResult> GetCourseHistory(int courseCode, string section)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var userSections = await _context.Sections
            .Include(s => s.Course)
            .Include(s => s.Attendees)
            .Where(s => s.UserId == Guid.Parse(userId!) && s.Course.Code == courseCode)
            .ToListAsync();

        var targetSection = userSections
            .FirstOrDefault(s => (s.SectionType + s.SectionId) == section);

        if (targetSection == null)
        {
            return NotFound("Section not found or you do not have access.");
        }

        // Get Timetables for this section to determine days
        var timetablesRaw = await _context.Timetables
            .Where(t => t.SectionId == targetSection.Id)
            .ToListAsync();

        var timetables = timetablesRaw
            .OrderBy(t => t.Timeslot.DayOfWeek)
            .ThenBy(t => t.Timeslot.TimeslotNumber)
            .ToList();

        if (!timetables.Any())
        {
            // Even if no schedule, return empty history
            return Ok(new HistoryDto
            {
                CourseName = targetSection.Course.Name,
                CourseCode = targetSection.Course.Code.ToString(),
                Section = section,
                Students = new List<HistoryStudentDto>(),
                Weeks = 14,
                DaysPerWeek = 0
            });
        }

        // Map TimetableId to Day Index (1-based)
        var timetableIndexMap = new Dictionary<Guid, int>();
        for (int i = 0; i < timetables.Count; i++)
        {
            timetableIndexMap[timetables[i].Id] = i + 1;
        }

        // Fetch logs
        var logs = await _context.AttendanceLogs
            .Where(l => l.Timetable.SectionId == targetSection.Id)
            .ToListAsync();

        // Build Student List
        var historyStudents = new List<HistoryStudentDto>();

        foreach (var attendee in targetSection.Attendees)
        {
            var studentLogs = logs.Where(l => l.AttendeeId == attendee.Id).ToList();
            var attendanceMap = new Dictionary<string, string>();

            foreach (var log in studentLogs)
            {
                if (timetableIndexMap.TryGetValue(log.TimetableId, out int dayIndex))
                {
                    // Key format: "w{Week}-{DayIndex}"
                    string key = $"w{log.WeekNumber}-{dayIndex}";
                    attendanceMap[key] = log.IsPresent ? "present" : "absent";
                }
            }

            historyStudents.Add(new HistoryStudentDto
            {
                Id = attendee.Id.ToString(),
                Name = attendee.FullName,
                Attendance = attendanceMap
            });
        }

        return Ok(new HistoryDto
        {
            CourseName = targetSection.Course.Name,
            CourseCode = targetSection.Course.Code.ToString(),
            Section = section,
            Students = historyStudents,
            Weeks = 14, // Fixed for now, could be dynamic
            DaysPerWeek = timetables.Count
        });
    }
}
