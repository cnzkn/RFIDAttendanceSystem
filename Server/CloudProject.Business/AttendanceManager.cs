namespace CloudProject.Business;

public class AttendanceManager
{
    private readonly ILogger<AttendanceManager> _logger;
    private readonly IRepository<AttendanceLogModel> _attendanceRepository;
    private readonly AttendeeManager _attendeeManager;
    private readonly DeviceManager _deviceManager;
    private readonly TimetableManager _timetableManager;

    public AttendanceManager(ILogger<AttendanceManager> logger, IRepository<AttendanceLogModel> attendanceRepository, AttendeeManager attendeeManager, DeviceManager deviceManager, TimetableManager timetableManager)
    {
        _logger = logger;
        _attendanceRepository = attendanceRepository;
        _attendeeManager = attendeeManager;
        _deviceManager = deviceManager;
        _timetableManager = timetableManager;
    }

    public async Task<AttendanceStatus> RecordAttendanceAsync(byte[] deviceFingerprint, byte[] cardId, CancellationToken token = default)
    {
        var device = await _deviceManager.FindByFingerprintAsync(deviceFingerprint, token);
        if (device is null)
        {
            throw new InvalidOperationException("Device not found.");
        }
        
        var attendee = await _attendeeManager.InternalGetByCardAsync(Convert.FromHexString(cardId), token);
        if (attendee is null)
        {
            return AttendanceStatus.UnrecognizedId;
        }

        var weekNumber = await _timetableManager.GetCurrentWeekAsync(token);
        var currentTimeslot = await _timetableManager.GetCurrentTimeslotAsync(token);
        if (currentTimeslot is null)
        {
            // No timeslot means there can't be any lecture right now.
            return AttendanceStatus.NoLecture;
        }
        
        var timetable = await _timetableManager.InternalGetClassroomTimetableAsync(device.AssignedClassroomId, token);
        if (timetable.FirstOrDefault(x => x.Timeslot != currentTimeslot) is not { } slot)
        {
            return AttendanceStatus.NoLecture;
        }

        if (!slot.CourseSection.AttendeeIds.Contains(attendee.Id))
        {
            return AttendanceStatus.NotRegistered;
        }
        
        var existingLog = await _attendanceRepository.FirstOrDefaultAsync(x =>
            x.AttendeeId == attendee.Id &&
            x.TimetableId == slot.Id &&
            DateOnly.FromDateTime(x.Date) == DateOnly.FromDateTime(DateTime.UtcNow), token);

        if (existingLog is { IsPresent: true })
        {
            // Maybe return AlreadyScanned if MarkedByType == UserModel too? Don't let devices override student scans?
            return AttendanceStatus.AlreadyScanned;
        }
        
        var attendanceLog = new AttendanceLogModel
        {
            Id = Guid.NewGuid(),
            AttendeeId = attendee.Id,
            TimetableId = slot.Id,
            Date = DateTime.UtcNow,
            WeekNumber = weekNumber,
            IsPresent = true,
            MarkedById = device.Id,
            MarkedByType = nameof(DeviceModel)
        };

        // Don't cancel at this point.
        // ReSharper disable once MethodSupportsCancellation
        await _attendanceRepository.AddAsync(attendanceLog);
        
        return AttendanceStatus.Success;
    }
    
    public async Task UpdateAttendanceAsync(Guid userId, AttendanceUpdateRequestDto request, CancellationToken token = default)
    {
        var timetable = await _timetableManager.InternalGetTimetableByIdAsync(request.TimetableId, token);
        if (timetable is null)
        {
            throw new InvalidOperationException("Timetable not found.");
        }
        
        var attendee = await _attendeeManager.InternalGetByIdAsync(request.AttendeeId, token);
        if (attendee is null)
        {
            throw new InvalidOperationException("Attendee not found.");
        }
        
        var weekNumber = await _timetableManager.GetCurrentWeekAsync(token);
        if (weekNumber == 0)
        {
            throw new InvalidOperationException("Not in a semester.");
        }
        
        var attendanceLog = new AttendanceLogModel
        {
            Id = Guid.NewGuid(),
            AttendeeId = request.AttendeeId,
            TimetableId = request.TimetableId,
            Date = DateTime.UtcNow,
            WeekNumber = weekNumber,
            IsPresent = true,
            MarkedById = userId,
            MarkedByType = nameof(UserModel)
        };
        
        // Don't cancel at this point.
        // ReSharper disable once MethodSupportsCancellation
        await _attendanceRepository.AddAsync(attendanceLog);
    }
    
    public async Task UpdateAttendanceAsync(Guid userId, List<AttendanceUpdateRequestDto> requests, CancellationToken token = default)
    {
        var oldAutoCommit = _attendanceRepository.AutoCommit;
        _attendanceRepository.AutoCommit = false;

        var tasks = new List<Task>();
        foreach (var request in requests)
        {
            tasks.Add(UpdateAttendanceAsync(userId, request, token));
        }
        
        await Task.WhenAll(tasks);
        await _attendanceRepository.CommitAsync(token);
        
        _attendanceRepository.AutoCommit = oldAutoCommit;
    }
    
    public async Task<List<AttendanceHistoryDto>> GetAttendanceHistoryAsync(Guid courseId, CancellationToken token = default)
    {
        var logs = (await _attendanceRepository.GetAllAsync(token))
            .ToList();
        
        if (logs.Count == 0)
        {
            return [];
        }

        var result = new List<AttendanceHistoryDto>();

        var logsByTimetable = logs.Where(x => x.Timetable.CourseSection.CourseId == courseId).GroupBy(x => x.TimetableId);
        
        foreach (var timetableGroup in logsByTimetable)
        {
            var timetable = await _timetableManager.InternalGetTimetableByIdAsync(timetableGroup.Key, token);
            if (timetable is null)
            {
                continue;
            }

            var sessions = new List<AttendanceHistorySessionDto>();
            var attendeeIds = timetable.CourseSection.AttendeeIds;
            var attendees = timetable.CourseSection.Attendees;
            var dict = new Dictionary<Guid, AttendeeDto>();
            var weekNumbers = await _timetableManager.GetTotalWeeksInCurrentSemesterAsync(token);
            
            for (int i = 1; i <= weekNumbers; ++i)
            {
                var weekLogs = timetableGroup.Where(x => x.WeekNumber == i)
                    .OrderByDescending(x => x.Date)
                    .ToList();

                List<AttendeeDto> presentList = new(), absentList = new();

                foreach (var attendeeId in attendeeIds)
                {
                    var present = false;
                    if (weekLogs.FirstOrDefault(x => x.AttendeeId == attendeeId) is { } item)
                    {
                        present = item.IsPresent;
                    }

                    if (!dict.TryGetValue(attendeeId, out var dto)) // simple caching
                    {
                        dto = attendees.FirstOrDefault(x => x.Id == attendeeId)!.ToDto();
                        dict[attendeeId] = dto;
                    }
                    
                    (present ? presentList : absentList).Add(dto);
                }

                var attendanceDict = new Dictionary<AttendanceRegisterType, List<AttendeeDto>>
                {
                    { AttendanceRegisterType.Present, presentList },
                    { AttendanceRegisterType.Absent, absentList }
                };

                sessions.Add(new AttendanceHistorySessionDto(i, timetable.Timeslot, attendanceDict));
            }
            
            result.Add(new AttendanceHistoryDto
            {
                Section = timetable.CourseSection.ToDto(),
                Sessions = sessions
            });
        }

        return result;
    }

}
