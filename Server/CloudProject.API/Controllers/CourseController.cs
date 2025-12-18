using CloudProject.Business;

namespace CloudAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class CourseController : Controller
{
    private readonly ILogger<CourseController> _logger;
    private readonly CourseManager _courseManager;
    private readonly TimetableManager _timetableManager;
    
    public CourseController(ILogger<CourseController> logger, CourseManager courseManager, TimetableManager timetableManager)
    {
        _logger = logger;
        _courseManager = courseManager;
        _timetableManager = timetableManager;
    }

    [Authorize, HttpGet] // /course
    public async Task<IActionResult> GetAll(CancellationToken token)
    {
        return Ok(await _courseManager.GetAllAsync(token));
    }

    [Authorize(Roles = nameof(UserRole.Administrator)), HttpDelete]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        try
        {
            await _courseManager.DeleteCourseAsync(id, token);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
        
        return NoContent();
    }
    
    [Authorize, HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute]int id, CancellationToken token)
    {
        return Ok(await _courseManager.GetAllSectionsAsync(id, token));
    }
    
    [Authorize, HttpGet("{id}/{section}")]
    public async Task<IActionResult> GetTimetable(int id, string section, CancellationToken token)
    {
        return Ok(await _timetableManager.GetTimetableBySectionAsync(id, section, token));
    }
}
