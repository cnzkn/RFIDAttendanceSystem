namespace CloudProject.API.Controllers;

[ApiController]
[Route("ws")]
public class WebSocketController : ControllerEx
{
    private readonly ILogger<WebSocketController> _logger;
    private readonly ICertificateValidator _validator;
    private readonly IModuleHandler _moduleHandler;
    private readonly IClientHandler _clientHandler;

    public WebSocketController(ILogger<WebSocketController> logger, ICertificateValidator validator, IModuleHandler moduleHandler, IClientHandler clientHandler)
    {
        _logger = logger;
        _validator = validator;
        _moduleHandler = moduleHandler;
        _clientHandler = clientHandler;
    }

    [HttpGet("device")]
    public async Task<IActionResult> DeviceWebSocket()
    {
        // Ensure client certificate is present
        var fingerprint = Request.Headers["X-Client-Cert-Fingerprint"].ToString();
        if (string.IsNullOrEmpty(fingerprint))
        {
            _logger.LogInformation("Rejecting connection from {IP} due to missing client certificate.", HttpContext.Connection.RemoteIpAddress);
            return Unauthorized("Client certificate required.");
        }

        // Validate it against our database
        if (!await _validator.IsValidAsync(fingerprint))
        {
            _logger.LogInformation("Rejecting connection from {IP} due to invalid client certificate: {Thumbprint}.", HttpContext.Connection.RemoteIpAddress, fingerprint);
            return Forbid("Invalid client certificate.");   
        }

        if (!HttpContext.WebSockets.IsWebSocketRequest)
            return BadRequest("Expected WebSocket request.");

        using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        _logger.LogInformation($"Device connected: {fingerprint}");

        await _moduleHandler.HandleModuleAsync(fingerprint, socket);
        return new EmptyResult();
    }
    
    [Authorize, HttpGet("client")]
    public async Task<IActionResult> ClientWebSocket()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
            return BadRequest("Expected WebSocket request.");

        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized("User authentication required.");

        using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        _logger.LogInformation($"Client connected: {userId}");

        await _clientHandler.HandleClientAsync(userId.Value, socket);
        return new EmptyResult();
    }
}
