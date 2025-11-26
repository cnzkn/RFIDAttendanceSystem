namespace CloudAPI.Controllers;

[ApiController]
[Route("ws")]
public class WebSocketController : ControllerBase
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

        // Validate it against your database
        if (!_validator.IsValid(cert))
            return Forbid("Invalid client certificate.");

        if (!HttpContext.WebSockets.IsWebSocketRequest)
            return BadRequest("Expected WebSocket request.");

        using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        Console.WriteLine($"[WS] Device connected: {cert.Thumbprint}");

        _ = _moduleHandler.HandleModuleAsync(cert.Thumbprint, socket);
        return new EmptyResult();
    }

    [HttpGet("public")]
    public async Task<IActionResult> PublicWebSocket()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
            return BadRequest("Expected WebSocket request.");

        using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        Console.WriteLine("[WS] Public connection established.");

        await HandleSocket(socket);
        return new EmptyResult();
    }

    private static async Task HandleSocket(WebSocket socket)
    {
        var buffer = new byte[1024];
        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            else
                await socket.SendAsync(buffer.AsMemory(0, result.Count), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
