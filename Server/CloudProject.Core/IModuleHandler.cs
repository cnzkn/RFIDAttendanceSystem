namespace CloudProject.Core;

public interface IModuleHandler
{
    Task HandleModuleAsync(string fingerprint, WebSocket? webSocket);
    bool IsConnected(string fingerprint);
    Task CloseAllAsync(CancellationToken token);
}