namespace CloudProject.Business;

public class CourseManager
{
    private readonly ILogger<CourseManager> _logger;
    private readonly IRepository<CourseModel> _courseRepository;
    private readonly IRepository<SectionModel> _sectionRepository;
    
    public CourseManager(ILogger<CourseManager> logger, IRepository<CourseModel> courseRepository, IRepository<SectionModel> sectionRepository)
    {
        _logger = logger;
        _courseRepository = courseRepository;
        _sectionRepository = sectionRepository;
    }
    
    public async Task<CourseDto[]> GetAllAsync(CancellationToken token)
    {
        var courses = await _courseRepository.GetAllAsync(token);

        return courses.Select(x => x.ToDto())
            .ToArray();
    }
    
    public async Task<CourseDto?> GetByCodeAsync(int code, CancellationToken token)
    {
        var course = await _courseRepository.FirstOrDefaultAsync(x => x.Code == code, token);
        return course?.ToDto();
    }

    public async Task<SectionDto[]> GetAllSectionsAsync(int code, CancellationToken token)
    {
        if (await _courseRepository.FirstOrDefaultAsync(x => x.Code == code, token) is not { } course)
        {
            return [];
        }
        
        var sections = await _sectionRepository.WhereAsync(x => x.CourseId == course.Id, token);
        return sections.Select(x => x.ToDto()).ToArray();
    }
    
    public async Task DeleteCourseAsync(Guid id, CancellationToken token)
    {
        if (await _courseRepository.GetByIdAsync(id, token) is not { } course)
        {
            throw new ObjectNotFoundException("Course not found.");
        }

        var sections = await _sectionRepository.WhereAsync(x => x.CourseId == course.Id, token);
        foreach (var section in sections)
        {
            await _sectionRepository.RemoveAsync(section, token);
        }

        int code = course.Code;
        await _courseRepository.RemoveAsync(course, token);
        _logger.LogInformation("Deleted course {CourseCode} and its sections.", code);
    }
}
