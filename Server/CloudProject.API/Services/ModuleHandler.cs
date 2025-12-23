namespace CloudProject.API.Services;

public record WebSocketConnection(string Id, WebSocket Socket, DateTime ConnectedAt)
{
    public DateTime LastPongTime { get; set; } = DateTime.UtcNow;
    public bool PingPending { get; set; }
}

public class ModuleHandler : IModuleHandler
{
    private readonly ILogger<ModuleHandler> _logger;
    private readonly ConcurrentDictionary<string, WebSocketConnection> _connections;
    private readonly DeviceManager _deviceManager;
    private readonly AttendanceManager _attendanceManager;
    private readonly TimetableManager _timetableManager;

    public ModuleHandler(ILogger<ModuleHandler> logger, DeviceManager deviceManager, AttendanceManager attendanceManager, TimetableManager timetableManager)
    {
        _logger = logger;
        _connections = new(StringComparer.OrdinalIgnoreCase);
        _deviceManager = deviceManager;
        _attendanceManager = attendanceManager;
        _timetableManager = timetableManager;
    }

    public async Task HandleModuleAsync(string fingerprint, WebSocket? webSocket)
    {
        ValueTask<ValueWebSocketReceiveResult> receiveTask;
        
        var client = new WebSocketConnection(fingerprint, webSocket, DateTime.UtcNow);
        _connections[client.Id] = client;
        var lastPingTime = DateTime.UtcNow;

        try
        {
            var buffer = new byte[4096];
            receiveTask = webSocket.ReceiveAsync(new Memory<byte>(buffer), CancellationToken.None);
            
            while (webSocket.State == WebSocketState.Open)
            {
                var now = DateTime.UtcNow;
                
                if (client.PingPending && (now - client.LastPongTime) > TimeSpan.FromSeconds(30))
                {
                    _logger.LogWarning("PONG timeout for connection {fingerprint}. Closing...", client.Id);
                    webSocket.Abort();
                    break;
                }

                if ((now - lastPingTime).TotalSeconds >= 30)
                {
                    using (var ms = new MemoryStream())
                    using (var bw = new BinaryWriter(ms))
                    {
                        bw.Write(0x474E4950);
                        await webSocket.SendAsync(ms.ToArray(), WebSocketMessageType.Binary, true, CancellationToken.None);
                        client.PingPending = true;
                        client.LastPongTime = now;
                    }
                    lastPingTime = now;
                }

                if (receiveTask.IsCompleted)
                {
                    var result = await receiveTask;
                    
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        using (var __ms = new MemoryStream(buffer, 0, result.Count))
                        using (var br = new BinaryReader(__ms))
                        {
                            var pkt = br.ReadInt32();

                            switch (pkt)
                            {
                                case 0x49514552: // PKT_REQUESTINFO
                                {
                                    using (var ms = new MemoryStream())
                                    using (var bw = new BinaryWriter(ms))
                                    {
                                        bw.Write(0x464E494D); // RSP_MODULEINFO
                                        if (await _deviceManager.GetByFingerprintAsync(Convert.FromHexString(fingerprint)) is not { } device)
                                        {
                                            bw.Write("---"u8);
                                            bw.Write((byte)0);
                                            bw.Write("---"u8);
                                            bw.Write((byte)0);
                                        }
                                        else
                                        {
                                            bw.Write(Encoding.UTF8.GetBytes(device.Classroom.Name));
                                            bw.Write((byte)0);

                                            try
                                            {
                                                if (await _timetableManager.GetClassroomCurrentTimetableAsync(device.Classroom.Id!.Value, CancellationToken.None) is { } timetable)
                                                {
                                                    bw.Write(Encoding.UTF8.GetBytes(timetable.Section.Course.Name));
                                                    bw.Write((byte)0);
                                                }
                                                else
                                                {
                                                    bw.Write("---"u8);
                                                    bw.Write((byte)0);
                                                }
                                            }
                                            catch (InvalidOperationException)
                                            {
                                                bw.Write("---"u8);
                                                bw.Write((byte)0);
                                            }

                                        }

                                        await webSocket.SendAsync(ms.ToArray(), WebSocketMessageType.Binary, true,
                                            CancellationToken.None);
                                    }

                                    break;
                                }

                                case 0x41514552: // PKT_SUBMITSCAN
                                {
                                    var length = br.ReadByte();
                                    var uidBytes = br.ReadBytes(length);

                                    var (status, name) = await _attendanceManager.RecordAttendanceAsync(Convert.FromHexString(fingerprint), uidBytes, CancellationToken.None);

                                    using (var ms = new MemoryStream())
                                    using (var bw = new BinaryWriter(ms))
                                    {
                                        bw.Write(0x53455253); // RSP_SCANRESULT
                                        bw.Write((byte)status);

                                        if (name is not null)
                                        {
                                            bw.Write(Encoding.UTF8.GetBytes(name));
                                            bw.Write((byte)0);
                                        }

                                        await webSocket.SendAsync(ms.ToArray(), WebSocketMessageType.Binary, true,
                                            CancellationToken.None);
                                    }

                                    break;
                                }

                                case 0x474E4950: // PKT_PING
                                {
                                    using (var ms = new MemoryStream())
                                    using (var bw = new BinaryWriter(ms))
                                    {
                                        bw.Write(0x474E4F50);
                                        await webSocket.SendAsync(ms.ToArray(), WebSocketMessageType.Binary, true,
                                            CancellationToken.None);
                                    }

                                    break;
                                }

                                case 0x474E4F50: // PKT_PONG
                                {
                                    if (_connections.TryGetValue(client.Id, out var conn))
                                    {
                                        conn.PingPending = false;
                                        conn.LastPongTime = DateTime.UtcNow;
                                    }

                                    break;
                                }
                            }
                        }
                    }

                    receiveTask = webSocket.ReceiveAsync(new Memory<byte>(buffer), CancellationToken.None);
                }

                await Task.Delay(50);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred handling client {fingerprint}.", client.Id);
        }
        finally
        {
            try
            {
                webSocket.Dispose();
            }
            catch (Exception)
            {
                // ignore
            }
            
            _connections.TryRemove(client.Id, out _);
            _logger.LogInformation("Client {fingerprint} disconnected.", client.Id);
        }
    }

    public bool IsConnected(string fingerprint)
    {
        return _connections.ContainsKey(fingerprint);
    }

    public async Task CloseAllAsync(CancellationToken token)
    {
        foreach (var client in _connections.Values)
        {
            await client.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down", token);
        }
    }
}
