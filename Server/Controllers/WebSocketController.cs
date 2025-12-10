namespace CloudAPI.Controllers;

[ApiController]
[Route("ws")]
public class WebSocketController : ControllerBase
{
    private readonly ICertificateValidator _validator;
    private readonly IModuleHandler _moduleHandler;
    private readonly IClientHandler _clientHandler;
    private readonly UserManager<UserModel> _userManager;

    public WebSocketController(ICertificateValidator validator, IModuleHandler moduleHandler, IClientHandler clientHandler, UserManager<UserModel> userManager)
    {
        _validator = validator;
        _moduleHandler = moduleHandler;
        _clientHandler = clientHandler;
        _userManager = userManager;
    }

    [HttpGet("device")]
    public async Task<IActionResult> DeviceWebSocket()
    {
        // Ensure client certificate is present
        var cert = HttpContext.Connection.ClientCertificate;
        if (cert == null)
            return Unauthorized("Client certificate required.");

        // Validate it against our database
        if (!_validator.IsValid(cert))
            return Forbid("Invalid client certificate.");

        if (!HttpContext.WebSockets.IsWebSocketRequest)
            return BadRequest("Expected WebSocket request.");

        using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        Console.WriteLine($"[WS] Device connected: {cert.Thumbprint}");

        _ = _moduleHandler.HandleModuleAsync(cert.Thumbprint, socket);
        return new EmptyResult();
    }

    [Authorize, HttpGet("client")]
    public async Task<IActionResult> ClientWebSocket()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
            return BadRequest("Expected WebSocket request.");
            
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        Console.WriteLine($"[WS] Client connected: {userId}");

        await _clientHandler.HandleClientAsync(Guid.Parse(userId), socket);
        return new EmptyResult();
    }
}
