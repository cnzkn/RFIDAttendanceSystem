using CloudProject.Business;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudProject.API.Controllers;

[ApiController]
[Route("[controller]")]
public class DebugController : Controller
{
    private readonly DatabaseContext _context;

    public DebugController(DatabaseContext context)
    {
        _context = context;
    }

    [HttpGet("sections")]
    public async Task<IActionResult> GetSections()
    {
        var sections = await _context.Sections
            .Include(x => x.Course)
            .Include(x => x.Attendees)
            .ToListAsync();

        var result = sections.Select(s => new
        {
            CourseName = s.Course.Name,
            Section = s.SectionType + s.SectionId,
            AttendeeIdsColumn = s.AttendeeIds ?? new List<Guid>(),
            AttendeeIdsColumnCount = s.AttendeeIds?.Count ?? 0,
            AttendeesNavigationCount = s.Attendees?.Count ?? 0
        });

        return Ok(result);
    }
}
