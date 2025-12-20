namespace CloudProject.API.Controllers;

[ApiController]
[Route("[controller]")]
public class HistoryController : ControllerEx
{
    private readonly ILogger<HistoryController> _logger;
    
    public HistoryController(ILogger<HistoryController> logger)
    {
        _logger = logger;
    }
    
    // TODO: Implement history endpoints
}
