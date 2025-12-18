using CloudProject.Business;

namespace CloudAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ClassroomController : Controller
{
    private readonly ILogger<ClassroomController> _logger;
    private readonly ClassroomManager _classroomManager;
    private readonly TimetableManager _timetableManager;

    public ClassroomController(ILogger<ClassroomController> logger, ClassroomManager classroomManager, TimetableManager timetableManager)
    {
        _logger = logger;
        _classroomManager = classroomManager;
        _timetableManager = timetableManager;
    }

    [Authorize, HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken token)
    {
        return Ok(await _classroomManager.GetAllAsync(token));
    }
    
    [Authorize, HttpGet("{name}")]
    public async Task<IActionResult> GetTimetable(string name, CancellationToken token)
    {
        if (await _classroomManager.GetIdByNameAsync(name, token) is not { } classroomId)
        {
            return NotFound();
        }
        
        return Ok(await _timetableManager.GetClassroomTimetableAsync(classroomId, token));
    }
}
