namespace CloudProject.API.Controllers;

[ApiController]
[Route("/")]
public class MainController : ControllerEx
{
    private readonly ILogger<MainController> _logger;
    
    public MainController(ILogger<MainController> logger)
    {
        _logger = logger;
    }
    
    [HttpGet]
    public IActionResult Get()
    {
        return Redirect("https://umutsen2662.github.io/RFIDAttendanceSystemPages/");
    }
}
