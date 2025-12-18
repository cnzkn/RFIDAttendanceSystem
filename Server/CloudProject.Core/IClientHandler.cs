namespace CloudProject.Core;

public interface IClientHandler
{
    Task HandleClientAsync(Guid userId, WebSocket socket);
    Task BroadcastUpdateAsync(Guid sessionId, object message);
}
