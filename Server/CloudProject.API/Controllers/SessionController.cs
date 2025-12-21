using CloudProject.Business;
using CloudProject.Business.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudProject.API.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class SessionController : ControllerEx
{
    private readonly TimetableManager _timetableManager;
    private readonly ClassroomManager _classroomManager;

    public SessionController(TimetableManager timetableManager, ClassroomManager classroomManager)
    {
        _timetableManager = timetableManager;
        _classroomManager = classroomManager;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSession(Guid id, CancellationToken token)
    {
        try
        {
            var session = await _timetableManager.GetTimetableByIdAsync(id, token);
            var currentWeek = await _timetableManager.GetCurrentWeekAsync(token);
            return Ok(new { session, currentWeek });
        }
        catch (ObjectNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    public class UpdateRoomRequest
    {
        public string RoomName { get; set; }
    }

    [HttpPost("update-room/{id}")]
    public async Task<IActionResult> UpdateRoom(Guid id, [FromBody] UpdateRoomRequest request, CancellationToken token)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var classroomId = await _classroomManager.GetIdByNameAsync(request.RoomName, token);
        if (classroomId == null)
        {
            return BadRequest($"Classroom '{request.RoomName}' not found.");
        }

        try
        {
            await _timetableManager.ChangeClassroomAsync(userId.Value, id, classroomId.Value, token);
            return Ok();
        }
        catch (ObjectNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}
