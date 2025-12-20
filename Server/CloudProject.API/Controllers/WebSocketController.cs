namespace CloudProject.API.Controllers;

[ApiController]
[Route("ws")]
public class WebSocketController : ControllerEx
{
    private readonly ICertificateValidator _validator;
    private readonly IModuleHandler _moduleHandler;

    public WebSocketController(ICertificateValidator validator, IModuleHandler moduleHandler)
    {
        _validator = validator;
        _moduleHandler = moduleHandler;
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
}
