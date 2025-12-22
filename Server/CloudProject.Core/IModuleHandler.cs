namespace CloudProject.Core;

public interface IModuleHandler
{
    Task HandleModuleAsync(string fingerprint, WebSocket? webSocket);
    Task CloseAllAsync(CancellationToken token);
}