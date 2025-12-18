namespace CloudProject.Business;

public class SemesterManager
{
    private readonly ILogger<SemesterManager> _logger;
    private readonly IRepository<SemesterModel> _semesterRepository;

    public SemesterManager(ILogger<SemesterManager> logger, IRepository<SemesterModel> semesterRepository)
    {
        _logger = logger;
        _semesterRepository = semesterRepository;
    }
    
    internal async Task<SemesterModel?> GetCurrentSemesterAsync(CancellationToken token = default)
    {
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var semesters = await _semesterRepository.WhereAsync(x => x.StartDate <= now && x.EndDate >= now, token);
        var semester = semesters.FirstOrDefault();
        
        if (semester == null)
        {
            _logger.LogWarning("No current semester found for date {Date}.", now.ToString("yyyy-MM-dd"));
            return null;
        }

        return semester;
    }
}
