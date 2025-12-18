using CloudProject.Business;

namespace CloudAPI.Services;

public class ClientHandler : IClientHandler
{
    private readonly ConcurrentDictionary<Guid, WebSocket> _sockets = new();
    private readonly ConcurrentDictionary<Guid, Guid> _clientSessions = new(); // ClientId -> SessionId
    private readonly ILogger<ClientHandler> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimetableManager _timetableManager;

    public ClientHandler(ILogger<ClientHandler> logger, IServiceScopeFactory scopeFactory, TimetableManager timetableManager)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _timetableManager = timetableManager;
    }

    public async Task HandleClientAsync(Guid userId, WebSocket socket)
    {
        var connectionId = Guid.NewGuid();
        _sockets.TryAdd(connectionId, socket);
        _logger.LogInformation($"Client connected: {connectionId} (User: {userId})");

        var buffer = new byte[1024 * 4];
        try
        {
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await HandleMessageAsync(connectionId, message);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WebSocket error for client {ConnectionId}", connectionId);
        }
        finally
        {
            _sockets.TryRemove(connectionId, out _);
            _clientSessions.TryRemove(connectionId, out _);
            _logger.LogInformation($"Client disconnected: {connectionId}");
        }
    }

    private async Task HandleMessageAsync(Guid connectionId, string jsonMessage)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonMessage);
            var root = doc.RootElement;

            if (root.TryGetProperty("action", out var actionProp))
            {
                var action = actionProp.GetString();
                if (action == "join_session")
                {
                    if (root.TryGetProperty("attendanceSessionId", out var sessionIdProp))
                    {
                        if (Guid.TryParse(sessionIdProp.GetString(), out var sessionId))
                        {
                            _clientSessions[connectionId] = sessionId;
                            _logger.LogInformation($"Client {connectionId} joined session {sessionId}");

                            // Send initial list
                            await SendInitialListAsync(connectionId, sessionId);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to parse client message: " + ex.Message);
        }
    }

    private async Task SendInitialListAsync(Guid connectionId, Guid sessionId)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        var timetable = await db.Timetables
            .Include(t => t.CourseSection)
            .ThenInclude(s => s.Attendees)
            .FirstOrDefaultAsync(t => t.Id == sessionId);

        if (timetable == null) return;

        // Use Semester WeekNumber for filtering to ensure consistency with History and Updates
        var currentWeek = await _timetableManager.GetCurrentWeekAsync();

        var logs = await db.AttendanceLogs
            .Where(l => l.TimetableId == sessionId && l.WeekNumber == currentWeek)
            .ToListAsync();

        _logger.LogInformation($"Found {timetable.CourseSection.Attendees.Count} attendees for session {sessionId}. Logs found: {logs.Count} (Week {currentWeek})");

        var studentList = new List<object>();

        foreach (var attendee in timetable.CourseSection.Attendees)
        {
            var log = logs.FirstOrDefault(l => l.AttendeeId == attendee.Id);

            studentList.Add(new
            {
                studentId = attendee.Id.ToString(),
                name = attendee.FullName,
                status = log != null ? (log.IsPresent ? "present" : "absent") : "nothing",
                timestamp = log?.Date.ToString("o"), // ISO 8601
                isManual = log?.MarkedByType == "User" // Simplified check
            });
        }

        var payload = new
        {
            type = "initial_list",
            attendanceSessionId = sessionId.ToString(),
            students = studentList
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        if (_sockets.TryGetValue(connectionId, out var socket) && socket.State == WebSocketState.Open)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            _logger.LogInformation($"Sent initial_list to {connectionId}. Payload size: {bytes.Length} bytes");
        }
        else 
        {
            _logger.LogWarning($"Could not send initial_list. Socket not found or closed. ID: {connectionId}");
        }
    }

    public async Task BroadcastUpdateAsync(Guid sessionId, object message)
    {
        var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        var segment = new ArraySegment<byte>(bytes);

        var tasks = new List<Task>();

        foreach (var client in _clientSessions)
        {
            if (client.Value == sessionId)
            {
                if (_sockets.TryGetValue(client.Key, out var socket) && socket.State == WebSocketState.Open)
                {
                    tasks.Add(socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None));
                }
            }
        }

        if (tasks.Any())
        {
            await Task.WhenAll(tasks);
        }
    }
}
