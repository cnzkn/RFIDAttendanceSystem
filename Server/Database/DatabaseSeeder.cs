using System.Text;
using System.Text.RegularExpressions;
using CloudAPI.Exceptions;

namespace CloudAPI.Database;

public class DatabaseSeeder
{
    // Local DTO class to match the environment variable structure including CardUID
    private class SeedAttendeeDto
    {
        public string StudentID { get; set; }
        public string FullName { get; set; }
        public string CardUID { get; set; }
    }

    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly DatabaseContext _context;
    private readonly UserManager<UserModel> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    
    public DatabaseSeeder(ILogger<DatabaseSeeder> logger, DatabaseContext context, UserManager<UserModel> userManager, RoleManager<IdentityRole<Guid>> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Seeding database...");

        try
        {
            // Seed user roles.
            await SeedRolesAsync();
            await _context.SaveChangesAsync();
            
            // Seed administrator account.
            await SeedAdministratorAsync();
            await _context.SaveChangesAsync();
            
            // Seed instructor account.
            await SeedInstructorAsync();
            await _context.SaveChangesAsync();
            
            // Seed classrooms.
            await SeedClassroomsAsync();
            await _context.SaveChangesAsync();
            
            // Seed courses, sections and their timetables.
            await SeedTimetablesAsync();
            await _context.SaveChangesAsync();

            // Seed extra attendees from Environment Variable
            await SeedAttendeesFromEnvAsync();
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new AggregateException("An error occurred while seeding the database.", ex);
        }
    }

    private async Task SeedAttendeesFromEnvAsync()
    {
        var attendeesJson = Environment.GetEnvironmentVariable("SEED_ATTENDEES");
        if (string.IsNullOrWhiteSpace(attendeesJson)) return;

        _logger.LogInformation("Seeding attendees from environment variable...");

        try 
        {
            var dtos = JsonSerializer.Deserialize<List<SeedAttendeeDto>>(attendeesJson);
            if (dtos == null) return;

            var newAttendees = new List<AttendeeModel>();

            foreach (var dto in dtos)
            {
                if (int.TryParse(dto.StudentID, out int studentId))
                {
                    if (!await _context.Attendee.AnyAsync(a => a.StudentID == studentId))
                    {
                        // Convert Hex/String CardUID to byte[]
                        var cardBytes = Encoding.UTF8.GetBytes(dto.CardUID ?? "0000");
                        if (cardBytes.Length > 4) cardBytes = cardBytes[..4]; // Truncate
                        else if (cardBytes.Length < 4) 
                        {
                            var temp = new byte[4];
                            cardBytes.CopyTo(temp, 0);
                            cardBytes = temp; // Pad
                        }

                        var attendee = new AttendeeModel 
                        { 
                            FullName = dto.FullName, 
                            StudentID = studentId,
                            CardUID = cardBytes 
                        };
                        
                        await _context.Attendee.AddAsync(attendee);
                        newAttendees.Add(attendee);
                        _logger.LogInformation($"Created attendee: {dto.FullName}");
                    }
                }
            }

            await _context.SaveChangesAsync();

            // Enroll them in the first available section so they appear in the UI
            if (newAttendees.Any())
            {
                var firstSection = await _context.Sections.Include(s => s.Attendees).FirstOrDefaultAsync();
                if (firstSection != null)
                {
                    foreach (var attendee in newAttendees)
                    {
                        if (!firstSection.Attendees.Any(a => a.Id == attendee.Id))
                        {
                            firstSection.Attendees.Add(attendee);
                        }
                    }
                    _logger.LogInformation($"Enrolled {newAttendees.Count} new attendees into section {firstSection.SectionType}{firstSection.SectionId}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to seed attendees from env: " + ex.Message);
        }
    }

    private async Task SeedRolesAsync()
    {
        _logger.LogDebug("Seeding roles...");

        foreach (var roleName in Enum.GetNames(typeof(UserRole)))
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                if (!roleResult.Succeeded)
                {
                    throw new IdentityErrorException($"Creation of role {roleName} failed.", roleResult.Errors);
                }
            }   
        }
    }

    private async Task SeedAdministratorAsync()
    {
        _logger.LogDebug("Seeding administrator account...");
        
        var username = Environment.GetEnvironmentVariable("SEED_ADMIN_USERNAME");
        var password = Environment.GetEnvironmentVariable("SEED_ADMIN_PASSWORD");

        ArgumentException.ThrowIfNullOrWhiteSpace(username, "SEED_ADMIN_USERNAME");
        ArgumentException.ThrowIfNullOrWhiteSpace(password, "SEED_ADMIN_PASSWORD");
        
        var user = await _userManager.FindByNameAsync(username);
        if (user is null)
        {
            user = new UserModel
            {
                UserName = username,
                FullName = "Administrator",
                Role = UserRole.Administrator
            };
            
            var createResult = await _userManager.CreateAsync(user, password);

            if (!createResult.Succeeded)
            {
                throw new IdentityErrorException($"Creation of administrator account failed.", createResult.Errors);
            }
            
            var roleResult = await _userManager.AddToRoleAsync(user, nameof(UserRole.Administrator));

            if (!roleResult.Succeeded)
            {
                throw new IdentityErrorException($"Assigning roles to administrator account failed.", roleResult.Errors);
            }
        }
        else
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, password);
            
            if (!result.Succeeded)
            {
                throw new IdentityErrorException($"Resetting password for administrator account failed.", result.Errors);
            }
        }
        
        await _context.SaveChangesAsync();
    }

    private async Task SeedInstructorAsync()
    {
        _logger.LogDebug("Seeding instructor account...");
        
        var username = Environment.GetEnvironmentVariable("SEED_INSTRUCTOR_USERNAME");
        var password = Environment.GetEnvironmentVariable("SEED_INSTRUCTOR_PASSWORD");

        ArgumentException.ThrowIfNullOrWhiteSpace(username, "SEED_INSTRUCTOR_USERNAME");
        ArgumentException.ThrowIfNullOrWhiteSpace(password, "SEED_INSTRUCTOR_PASSWORD");
        
        var user = await _userManager.FindByNameAsync(username);
        if (user is null)
        {
            user = new UserModel
            {
                UserName = username,
                FullName = "Instructor",
                Role = UserRole.Instructor
            };
            
            var createResult = await _userManager.CreateAsync(user, password);

            if (!createResult.Succeeded)
            {
                throw new IdentityErrorException($"Creation of instructor account failed.", createResult.Errors);
            }
            
            var roleResult = await _userManager.AddToRoleAsync(user, nameof(UserRole.Instructor));

            if (!roleResult.Succeeded)
            {
                throw new IdentityErrorException($"Assigning roles to instructor account failed.", roleResult.Errors);
            }
        }
        else
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, password);

            if (!result.Succeeded)
            {
                throw new IdentityErrorException($"Resetting password for instructor account failed.", result.Errors);
            }
        }
        
        await _context.SaveChangesAsync();
    }

    private async Task SeedClassroomsAsync()
    {
        _logger.LogDebug("Seeding classrooms...");
                
        /*
         * The following snippet is used to scrape classrooms off of CET's "View Schedules" page in JSON, and only certain items are picked for demonstration purposes only:
         * console.log(JSON.stringify(
         *    Object.fromEntries([...document.querySelectorAll("#classrooms option")]
         *      .map(o => [parseInt(o.value), o.textContent.trim()])),
         *    null, 2
         *  ));
         */

        // The data is included as an environment variable, as CET system is not accessible without a proper authorization and including them here poses a security risk.
        string? classroomsSeed = Environment.GetEnvironmentVariable("SEED_CLASSROOMS");
        ArgumentException.ThrowIfNullOrWhiteSpace(classroomsSeed, "SEED_CLASSROOMS");

        var classrooms = JsonSerializer.Deserialize<Dictionary<int, string>>(classroomsSeed);
        if (classrooms == null)
        {
            throw new ApplicationException("Classrooms could not be deserialized.");
        }

        foreach (var classroom in classrooms)
        {
            if (!await _context.Classrooms.AnyAsync(x => x.Name == classroom.Value))
            {
                await _context.Classrooms.AddAsync(new ClassroomModel { Name = classroom.Value });
                await _context.SaveChangesAsync();
            }
        }
    }
    private async Task SeedTimetablesAsync()
    {
        _logger.LogDebug("Seeding timetables...");
                
        /*
         * Timetable data from the CET system was pulled by an API call the webpage uses. It is not detailed here for security reasons.
         * Among the data, only certain items are picked for demonstration purposes only.
         */

        // The data is included as an environment variable, as CET system is not accessible without a proper authorization and including them here poses a security risk.
        string? timetablesSeed = Environment.GetEnvironmentVariable("SEED_TIMETABLES");
        ArgumentException.ThrowIfNullOrWhiteSpace(timetablesSeed, "SEED_TIMETABLES");

        var instructorName  = Environment.GetEnvironmentVariable("SEED_INSTRUCTOR_USERNAME");
        ArgumentException.ThrowIfNullOrWhiteSpace(instructorName, "SEED_INSTRUCTOR_USERNAME");

        if (await _userManager.FindByNameAsync(instructorName) is not { } instructor)
        {
            throw new ApplicationException("Instructor account could not be found. Perhaps seeding failed?");
        }
        
        JsonElement slots = JsonSerializer.Deserialize<dynamic>(timetablesSeed);

        foreach (var slot in slots.EnumerateArray())
        {
            string timeslotInfo = slot.GetProperty("course").GetString()!;
            
            if (Regex.Match(timeslotInfo, @"^([0-9a-zA-Z ]+)\s+\(([A-Za-z]*)(\d*)\)<br>\[(.+)\]$", RegexOptions.IgnoreCase) is { Success: true } match) 
            {
                var courseName = match.Groups[1].Value;
                
                if (await _context.Courses.FirstOrDefaultAsync(c => c.Name.ToLower() == courseName.ToLower()) is not { } course)
                {
                    _logger.LogDebug("Course {courseName} not found, creating a new course...", courseName);
                    
                    course = new CourseModel()
                    {
                        Code = 3550000,
                        Name = courseName
                    };
                    
                    await _context.Courses.AddAsync(course);
                    await _context.SaveChangesAsync();
                }
                
                var sectionType = match.Groups[2].Value;
                var sectionId = int.Parse(match.Groups[3].Value);

                if (await _context.Sections.FirstOrDefaultAsync(x => x.CourseId == course.Id && x.SectionType == sectionType && x.SectionId == sectionId) is not { } section)
                {
                    _logger.LogDebug("Section {section} for {course} not found, creating a new section...", sectionType + sectionId, courseName);

                    section = new SectionModel()
                    {
                        CourseId = course.Id,
                        SectionId = sectionId,
                        SectionType = sectionType,
                        UserId = instructor.Id,
                        AttendeeIds = new()
                    };
                    
                    await _context.Sections.AddAsync(section);
                    await _context.SaveChangesAsync();
                }
                
                var classroomName = match.Groups[4].Value;

                if (await _context.Classrooms.FirstOrDefaultAsync(c => c.Name.ToLower() == classroomName.ToLower()) is not { } classroom)
                {
                    throw new ApplicationException($"Classroom {classroomName} not found. Perhaps seeding failed?");
                }

                DayOfWeek dow = (DayOfWeek)slot.GetProperty("day").GetInt32();
                int time = slot.GetProperty("time").GetInt32();

                if (await _context.Timetables.AnyAsync(x => x.SectionId == section.Id && x.ClassroomId == classroom.Id && x.Timeslot.DayOfWeek == dow && x.Timeslot.TimeslotNumber == time))
                {
                    _logger.LogDebug("A timetable entry for {course} {section} at {day} slot {time} already exists. Skipping...", courseName, sectionType + sectionId, dow, time);
                    continue;
                }
                
                var timetable = new TimetableModel
                {
                    ClassroomId = classroom.Id,
                    SectionId = section.Id,
                    Timeslot = new TimeslotModel
                    {
                        DayOfWeek = dow,
                        TimeslotNumber = time
                    }
                };
                
                await _context.Timetables.AddAsync(timetable);
                await _context.SaveChangesAsync();
            }
            else
            {
                _logger.LogDebug("{courseName} did not match the Regex pattern, ignoring...", timeslotInfo);
            }
        }
    }
}
