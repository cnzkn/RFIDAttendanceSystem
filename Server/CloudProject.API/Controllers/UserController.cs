using System.Net;
using CloudProject.Business.Dto;

namespace CloudProject.API.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerEx
{
    private readonly ILogger<UserController> _logger;
    private readonly SignInManager<UserModel> _signInManager;
    private readonly UserManager<UserModel> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly DatabaseContext _context;
    
    public UserController(ILogger<UserController> logger, SignInManager<UserModel> signInManager, UserManager<UserModel> userManager, RoleManager<IdentityRole<Guid>> roleManager, DatabaseContext context)
    {
        _logger = logger;
        _signInManager = signInManager;
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    [Authorize, HttpGet]
    public async Task<IActionResult> Me()
    {
        var user = await GetCurrentUserAsync();
        return Ok(user!.ToDto());
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto login)
    {
        if (_signInManager.IsSignedIn(User))
        {
            return Ok();
        }
        
        if (await _userManager.FindByNameAsync(login.UserName) is not { } user)
        {
            return Unauthorized();
        }

        if (!await _signInManager.CanSignInAsync(user))
        {
            return Forbid();
        }
        
        var result = await _signInManager.PasswordSignInAsync(user, login.Password, false, false);

        if (!result.Succeeded)
        {
            return Unauthorized();
        }
            
        return Ok();
    }

    [Authorize, HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok();
    }

    [Authorize(Roles = nameof(UserRole.Instructor)), HttpGet("sections")]
    public async Task<IActionResult> GetSections()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId!);

        var result = await _context.Sections.Where(x => x.UserId == user!.Id).ToListAsync();
        return Ok(result.Select(x => x.ToDto()));
    }
    
    [Authorize, HttpGet("timetable")]
    public async Task<IActionResult> GetTimetable()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId!);

        var sections = await _context.Sections.Where(x => x.UserId == user!.Id).ToListAsync();
        var sectionIds = sections.Select(x => x.Id).ToList();
        
        var timetable = await _context.Timetables.Where(x => sectionIds.Contains(x.SectionId)).ToListAsync();
        return Ok(timetable.Select(x => x.ToDto()));
    }
}
