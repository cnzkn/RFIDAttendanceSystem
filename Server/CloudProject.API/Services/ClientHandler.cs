namespace CloudProject.API.Services;

public class ClientHandler : IClientHandler
{
    private readonly ConcurrentDictionary<Guid, WebSocket> _sockets = new();
    private readonly ConcurrentDictionary<Guid, Guid> _clientSessions = new(); // ClientId -> SessionId
    private readonly ILogger<ClientHandler> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public ClientHandler(ILogger<ClientHandler> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
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
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
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
            _logger.LogWarning(ex, "Failed to parse client message.");
        }
    }

    private async Task SendInitialListAsync(Guid connectionId, Guid timetableId)
    {
        using var scope = _scopeFactory.CreateScope();
        var attendanceManager = scope.ServiceProvider.GetRequiredService<AttendanceManager>();
        var courseManager = scope.ServiceProvider.GetRequiredService<CourseManager>();
        var timetableManager = scope.ServiceProvider.GetRequiredService<TimetableManager>();
        
        TimetableDto timetable;
        try
        {
            timetable = await timetableManager.GetTimetableByIdAsync(timetableId);
        }
        catch (ObjectNotFoundException)
        {
            return;
        }

        // Use Semester WeekNumber for filtering to ensure consistency with History and Updates
        var currentWeek = await timetableManager.GetCurrentWeekAsync();
        
        var logs = await attendanceManager.GetAttendanceLogsByTimetableAndWeekAsync(timetableId, currentWeek);

        var attendeeList = await courseManager.GetSectionAttendeesAsync(timetable.Section.Id!.Value);
        _logger.LogCritical("Count: {attendeeCount}", attendeeList.Length);
        
        var studentList = new List<object>();

        foreach (var attendee in attendeeList)
        {
            var log = logs.OrderByDescending(x => x.Date)
                .FirstOrDefault(l => l.Attendee.Id == attendee.Id);

            studentList.Add(new
            {
                studentId = attendee.Id.ToString(),
                name = attendee.FullName,
                status = log != null ? (log.IsPresent ? "present" : "absent") : "nothing",
                timestamp = log?.Date.ToString("o"), // ISO 8601
                isManual = log?.Registrar is UserModel // Simplified check
            });
        }

        var payload = new
        {
            type = "initial_list",
            attendanceSessionId = timetableId.ToString(),
            students = studentList
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        if (_sockets.TryGetValue(connectionId, out var socket) && socket.State == WebSocketState.Open)
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            _logger.LogInformation($"Sent initial_list to {connectionId}. Payload size: {bytes.Length} bytes");
        }
        else 
        {
            _logger.LogWarning($"Could not send initial_list. Socket not found or closed. ID: {connectionId}");
        }
    }
    
    public async Task BroadcastUpdateAsync(AttendanceLogDto log)
    {
        dynamic message = new
        {
            type = "student_updated",
            studentId = log.Attendee.Id.ToString(),
            status = log.IsPresent ? "present" : "absent",
            timestamp = log.Date.ToString("o"), // ISO 8601
            isManual = log.Registrar is UserModel
        };
        
        var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        var segment = new ArraySegment<byte>(bytes);

        var tasks = new List<Task>();

        foreach (var client in _clientSessions)
        {
            if (client.Value == log.Timetable.Id)
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
    
    public async Task CloseAllAsync(CancellationToken token)
    {
        foreach (var client in _sockets.Values)
        {
            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down", token);
        }
    }
}
