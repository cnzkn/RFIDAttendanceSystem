namespace CloudProject.Business;

public class TimetableManager
{
    private readonly ILogger<TimetableManager> _logger;
    private readonly IRepository<TimetableModel> _timetableRepository;
    private readonly SemesterManager _semesterManager;
    private readonly ClassroomManager _classroomManager;
    private readonly UserManager _userManager;

    public TimetableManager(ILogger<TimetableManager> logger, IRepository<TimetableModel> timetableRepository, SemesterManager semesterManager, ClassroomManager classroomManager, UserManager userManager)
    {
        _logger = logger;
        _timetableRepository = timetableRepository;
        _semesterManager = semesterManager;
        _classroomManager = classroomManager;
        _userManager = userManager;
    }

    public async Task<int> GetCurrentWeekAsync(CancellationToken token = default)
    {
        var sem = await _semesterManager.GetCurrentSemesterAsync(token);
        if (sem == null)
        {
            return 0;
        }
        
        var start = sem.StartDate;
        var today = DateOnly.FromDateTime(DateTime.Now);
        return ((today.DayNumber - start.DayNumber) / 7) + 1;
    }
    
    public async Task<int> GetTotalWeeksInCurrentSemesterAsync(CancellationToken token = default)
    {
        var sem = await _semesterManager.GetCurrentSemesterAsync(token);
        if (sem == null)
        {
            return 0;
        }

        var totalDays = (sem.EndDate.DayNumber - sem.StartDate.DayNumber) + 1;
        return (int)Math.Ceiling(totalDays / 7.0);
    }
    
    public async Task<TimeslotModel?> GetCurrentTimeslotAsync(CancellationToken token = default)
    {
        var sem = await _semesterManager.GetCurrentSemesterAsync(token);
        if (sem == null)
        {
            return null;
        }
        
        var utcStart = TimeSpan.FromMinutes(400);
        
        var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.Local);
        var localStart = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow + utcStart, TimeZoneInfo.Local);
        
        var slot = (int)(localNow - localStart).TotalHours;

        if (slot is < 0 or > 14)
        {
            return null;
        }
        
        return new TimeslotModel
        {
            DayOfWeek = localNow.DayOfWeek,
            TimeslotNumber = slot
        };
    }
    
    internal async Task<TimetableModel?> InternalGetTimetableByIdAsync(Guid timetableId, CancellationToken token = default)
    {
        return await _timetableRepository.GetByIdAsync(timetableId, token);
    }
    
    internal async Task<TimetableModel[]> InternalGetClassroomTimetableAsync(Guid classroomId, CancellationToken token = default)
    {
        var results = await _timetableRepository.WhereAsync(x => x.ClassroomId == classroomId, token);
        return results.ToArray();
    }
    
    public async Task<TimetableDto[]> GetClassroomTimetableAsync(Guid classroomId, CancellationToken token = default)
    {
        var result = await InternalGetClassroomTimetableAsync(classroomId, token);
        return result.Select(x => x.ToDto())
            .ToArray();
    }
        
    public async Task<TimetableDto?> GetClassroomCurrentTimetableAsync(Guid classroomId, CancellationToken token = default)
    {
        if (await GetCurrentTimeslotAsync(token) is not { } timeslot)
        {
            throw new InvalidOperationException("Not currently in a valid timeslot.");
        }
        
        var result = await InternalGetClassroomTimetableAsync(classroomId, token);
        return result.FirstOrDefault(x => x.Timeslot == timeslot)?.ToDto();
    }
    
    public async Task<List<TimetableModel>> GetTimetableBySectionAsync(Guid sectionId, CancellationToken token = default)
    {
        var result = await _timetableRepository.WhereAsync(x => x.SectionId == sectionId, token);
        return result.ToList();
    }

    internal async Task<List<TimetableModel>> GetTimetablesByCourseIdAsync(Guid courseId, CancellationToken token = default)
    {
        var result = await _timetableRepository.WhereAsync(x => x.CourseSection.CourseId == courseId, token);
        return result.ToList();
    }
        
    public async Task<List<TimetableDto>> GetTimetableBySectionAsync(int code, string section, CancellationToken token = default)
    {
        var result = await _timetableRepository.WhereAsync(x => x.CourseSection.Course.Code == code && (x.CourseSection.SectionType + x.CourseSection.SectionId) == section, token);

        return result.Select(x => x.ToDto())
            .ToList();
    }
    
    public async Task ChangeClassroomAsync(Guid userId, Guid timetableId, Guid newClassroomId, CancellationToken token = default)
    {
        var timetable = await InternalGetTimetableByIdAsync(timetableId, token);
        if (timetable == null)
        {
            throw new ObjectNotFoundException("Timetable not found.");
        }

        if (await _classroomManager.InternalGetClassroomByIdAsync(newClassroomId, token) is not { } classroom)
        {
            throw new ObjectNotFoundException("Classroom could not be found.");
        }

        if (await _userManager.InternalGetByIdAsync(userId, token) is not { } user)
        {
            throw new ObjectNotFoundException("User not found.");
        }

        if (user.Role == UserRole.Administrator || (user.Role == UserRole.Instructor && timetable.CourseSection.UserId == userId))
        {
            timetable.ClassroomId = classroom.Id;
            await _timetableRepository.UpdateAsync(timetable, token);   
        }
        else
        {
            throw new UnauthorizedAccessException("User does not have permission to change the classroom for this timetable.");
        }
    }
    
    public async Task<TimetableDto> GetTimetableByIdAsync(Guid timetableId, CancellationToken token = default)
    {
        var timetable = await InternalGetTimetableByIdAsync(timetableId, token);
        if (timetable == null)
        {
            throw new ObjectNotFoundException("Timetable not found.");
        }

        return timetable.ToDto();
    }
}
