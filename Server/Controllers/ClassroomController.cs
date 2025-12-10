namespace CloudAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = "Administrator,Instructor")]
public class ClassroomController : Controller
{
    private readonly DatabaseContext _context;

    public ClassroomController(DatabaseContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var classrooms = await _context.Classrooms
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Ok(classrooms.Select(c => c.ToDto()));
    }
}