namespace CloudAPI.Services;

public interface IModuleHandler
{
    Task HandleModuleAsync(string fingerprint, WebSocket? webSocket);
}