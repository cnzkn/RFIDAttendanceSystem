namespace CloudProject.API.Controllers;

[ApiController]
[Route("ws")]
public class WebSocketController : ControllerEx
{
    private readonly ILogger<WebSocketController> _logger;
    private readonly ICertificateValidator _validator;
    private readonly IModuleHandler _moduleHandler;

    public WebSocketController(ILogger<WebSocketController> logger, ICertificateValidator validator, IModuleHandler moduleHandler)
    {
        _logger = logger;
        _validator = validator;
        _moduleHandler = moduleHandler;
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
}
