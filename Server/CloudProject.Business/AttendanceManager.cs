namespace CloudProject.Business;

public class AttendanceManager
{
    private readonly ILogger<AttendanceManager> _logger;
    private readonly IRepository<AttendanceLogModel> _attendanceRepository;
    private readonly AttendeeManager _attendeeManager;
    private readonly DeviceManager _deviceManager;
    private readonly TimetableManager _timetableManager;
    private readonly UserManager _userManager;
    private readonly IClientHandler _clientHandler;

    public AttendanceManager(ILogger<AttendanceManager> logger, IRepository<AttendanceLogModel> attendanceRepository, AttendeeManager attendeeManager, DeviceManager deviceManager, TimetableManager timetableManager, UserManager userManager, IClientHandler clientHandler)
    {
        _logger = logger;
        _attendanceRepository = attendanceRepository;
        _attendeeManager = attendeeManager;
        _deviceManager = deviceManager;
        _timetableManager = timetableManager;
        _userManager = userManager;
        _clientHandler = clientHandler;
    }

    public async Task<(AttendanceStatus Status, string? Name)> RecordAttendanceAsync(byte[] deviceFingerprint, byte[] cardId, CancellationToken token = default)
    {
        var device = await _deviceManager.FindByFingerprintAsync(deviceFingerprint, token);
        if (device is null)
        {
            throw new ObjectNotFoundException("Device not found.");
        }
        
        var attendee = await _attendeeManager.InternalGetByCardAsync(cardId, token);
        if (attendee is null)
        {
            return (AttendanceStatus.UnrecognizedId, null);
        }

        var weekNumber = await _timetableManager.GetCurrentWeekAsync(token);
        var currentTimeslot = await _timetableManager.GetCurrentTimeslotAsync(token);
        if (currentTimeslot is null)
        {
            // No timeslot means there can't be any lecture right now.
            return (AttendanceStatus.NoLecture, null);
        }
        
        var timetable = await _timetableManager.InternalGetClassroomTimetableAsync(device.AssignedClassroomId, token);
        if (timetable.FirstOrDefault(x => x.Timeslot != currentTimeslot) is not { } slot)
        {
            return (AttendanceStatus.NoLecture, null);
        }

        if (!slot.CourseSection.AttendeeIds.Contains(attendee.Id))
        {
            return (AttendanceStatus.NotRegistered, null);
        }
        
        var existingLog = await _attendanceRepository.FirstOrDefaultAsync(x =>
            x.AttendeeId == attendee.Id &&
            x.TimetableId == slot.Id &&
            DateOnly.FromDateTime(x.Date) == DateOnly.FromDateTime(DateTime.UtcNow), token);

        if (existingLog is { IsPresent: true })
        {
            // Maybe return AlreadyScanned if MarkedByType == UserModel too? Don't let devices override student scans?
            return (AttendanceStatus.AlreadyScanned, null);
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

        try
        {
            await _clientHandler.BroadcastUpdateAsync(attendanceLog.ToDto(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast attendance update for {attendeeId} in {timetable}.", attendee, timetable);
        }
        
        return (AttendanceStatus.Success, attendee.FullName);
    }
    
    public async Task UpdateAttendanceAsync(Guid userId, AttendanceUpdateRequestDto request, CancellationToken token = default)
    {
        if (await _userManager.GetByIdAsync(userId, token) is not { } user)
        {
            throw new ObjectNotFoundException("User not found.");
        }
        
        var timetable = await _timetableManager.InternalGetTimetableByIdAsync(request.TimetableId, token);
        if (timetable is null)
        {
            throw new ObjectNotFoundException("Timetable not found.");
        }
        
        var attendee = await _attendeeManager.InternalGetByIdAsync(request.AttendeeId, token);
        if (attendee is null)
        {
            throw new ObjectNotFoundException("Attendee not found.");
        }
        
        var weekNumber = request.WeekNumber ?? await _timetableManager.GetCurrentWeekAsync(token);
        if (weekNumber == 0)
        {
            throw new InvalidOperationException("Not in a semester, or invalid week number.");
        }

        if (user.Role == UserRole.Administrator || (user.Role == UserRole.Instructor && timetable.CourseSection.UserId == userId))
        {
            // Check for existing log to update instead of duplicate insert
            var existingLog = await _attendanceRepository.FirstOrDefaultAsync(x => 
                x.TimetableId == request.TimetableId && 
                x.AttendeeId == request.AttendeeId &&
                x.WeekNumber == weekNumber, token);

            if (request.Status == null)
            {
                // Pending state: Remove record if it exists
                if (existingLog != null)
                {
                    await _attendanceRepository.RemoveAsync(existingLog, token);
                }
            }
            else
            {
                bool isPresent = request.Status == AttendanceRegisterType.Present;

                if (existingLog != null)
                {
                    existingLog.IsPresent = isPresent;
                    existingLog.MarkedById = userId;
                    existingLog.MarkedByType = nameof(UserModel);
                    await _attendanceRepository.UpdateAsync(existingLog, token);
                }
                else
                {
                    existingLog = new AttendanceLogModel
                    {
                        Id = Guid.NewGuid(),
                        AttendeeId = request.AttendeeId,
                        TimetableId = request.TimetableId,
                        // Note: Date might be inaccurate if updating past history without a specific date. 
                        // Using UtcNow is "log time", but for history consistency, we rely on WeekNumber.
                        Date = DateTime.UtcNow, 
                        WeekNumber = weekNumber,
                        IsPresent = isPresent,
                        MarkedById = userId,
                        MarkedByType = nameof(UserModel)
                    };
                    
                    await _attendanceRepository.AddAsync(existingLog, token);
                }
                
                try
                {
                    await _clientHandler.BroadcastUpdateAsync(existingLog.ToDto(true));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to broadcast attendance update for {attendeeId} in {timetable}.", attendee, timetable);
                }
            }
        }
        else
        {
            throw new UnauthorizedAccessException("User not authorized to update attendance for this timetable.");
        }
    }
    
    public async Task UpdateAttendanceAsync(Guid userId, List<AttendanceUpdateRequestDto> requests, CancellationToken token = default)
    {
        var oldAutoCommit = _attendanceRepository.AutoCommit;
        _attendanceRepository.AutoCommit = false;

        foreach (var request in requests)
        {
            await UpdateAttendanceAsync(userId, request, token);
        }
        
        await _attendanceRepository.CommitAsync(token);
        
        _attendanceRepository.AutoCommit = oldAutoCommit;
    }
    
    public async Task<List<AttendanceHistoryDto>> GetAttendanceHistoryAsync(Guid courseId, CancellationToken token = default)
    {
        var timetables = await _timetableManager.GetTimetablesByCourseIdAsync(courseId, token);
        var result = new List<AttendanceHistoryDto>();
        
        var logs = (await _attendanceRepository.WhereAsync(x => x.Timetable.CourseSection.CourseId == courseId, token)).ToList();

        // Group by Section
        var timetablesBySection = timetables.GroupBy(t => t.SectionId);

        foreach (var sectionGroup in timetablesBySection)
        {
            // Use the first timetable to get section info (they all belong to same section)
            var sectionInfo = sectionGroup.First().CourseSection;
            
            var attendees = sectionInfo.Attendees ?? [];
            var studentDtos = attendees.Select(a => a.ToDto(true)).ToList();
            
            var weekNumbers = await _timetableManager.GetTotalWeeksInCurrentSemesterAsync(token);
            
            var timetableDtos = new List<AttendanceTimetableDto>();

            foreach (var timetable in sectionGroup)
            {
                var sessions = new List<AttendanceHistorySessionDto>();
                var timetableLogs = logs.Where(x => x.TimetableId == timetable.Id).ToList();

                for (int i = 1; i <= weekNumbers; ++i)
                {
                    var weekLogs = timetableLogs.Where(x => x.WeekNumber == i)
                        .OrderByDescending(x => x.Date)
                        .ToList();

                    List<Guid> presentList = new(), absentList = new();

                    foreach (var attendee in attendees)
                    {
                        if (weekLogs.FirstOrDefault(x => x.AttendeeId == attendee.Id) is { } item)
                        {
                            (item.IsPresent ? presentList : absentList).Add(attendee.Id);
                        }
                        // If no log exists, do NOT add to either list. 
                        // This allows the frontend to interpret "missing from both" as "Pending".
                    }
                    
                    var attendanceDict = new Dictionary<AttendanceRegisterType, List<Guid>>
                    {
                        { AttendanceRegisterType.Present, presentList },
                        { AttendanceRegisterType.Absent, absentList }
                    };

                    sessions.Add(new AttendanceHistorySessionDto(i, attendanceDict));
                }
                
                timetableDtos.Add(new AttendanceTimetableDto
                {
                    Id = timetable.Id,
                    Timeslot = timetable.Timeslot,
                    Sessions = sessions
                });
            }

            result.Add(new AttendanceHistoryDto
            {
                Section = sectionInfo.ToDto(true),
                Students = studentDtos,
                Timetables = timetableDtos
            });
        }

        return result;
    }

}
