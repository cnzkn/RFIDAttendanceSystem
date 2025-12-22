namespace CloudProject.Business;

public interface IClientHandler
{
    Task HandleClientAsync(Guid userId, WebSocket socket);
    Task BroadcastUpdateAsync(AttendanceLogDto model);
    Task CloseAllAsync(CancellationToken token);
}
