using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using CloudAPI.Database;
using CloudAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudAPI.Services;

public record WebSocketConnection(string Id, WebSocket Socket, DateTime ConnectedAt);

public enum AttendanceStatus
{
    Success, // Attendance registered.
    AlreadyScanned, // Already registered attendance.
    NoLecture, // There's no upcoming/current lecture in the classroom.
    NotRegistered, // Student not registered in current lecture.
    UnrecognizedID, // Unrecognized ID card.
    Error // Exception occurred.
}

public class ModuleHandler : IModuleHandler
{
    private readonly ILogger<ModuleHandler> _logger;
    private readonly IClientHandler _clientHandler;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConcurrentDictionary<string, WebSocketConnection> _connections;

    public ModuleHandler(ILogger<ModuleHandler> logger, IClientHandler clientHandler, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _clientHandler = clientHandler;
        _scopeFactory = scopeFactory;
        _connections = new();
    }

    public async Task HandleModuleAsync(string fingerprint, WebSocket? webSocket)
    {
        var client = new WebSocketConnection(fingerprint, webSocket, DateTime.UtcNow);
        _connections[client.Id] = client;

        try
        {
            var buffer = new byte[4096];
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(buffer.AsMemory(0, buffer.Length), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Binary)
                {
                    // Use a slice of the buffer corresponding to the received data
                    using (var __ms = new MemoryStream(buffer, 0, result.Count))
                    using (var br = new BinaryReader(__ms))
                    {
                        if (__ms.Length < 4) continue;
                        var pkt = br.ReadInt32();

                        switch (pkt)
                        {
                            case 0x49514552: // REQ I (Info)
                            {
                                // Using OLD logic to maintain device compatibility
                                using (var ms = new MemoryStream())
                                using (var bw = new BinaryWriter(ms))
                                {
                                    bw.Write(0x464E494D); // MINF
                                    
                                    if (fingerprint.Length > 0 && fingerprint[0] == 'B')
                                    {
                                        bw.Write("S-113"u8);
                                        bw.Write((byte)0);
                                        bw.Write((byte)0);
                                    }
                                    else
                                    {
                                        bw.Write("S-114"u8);
                                        bw.Write((byte)0);
                                        bw.Write("CNG491"); // Keeps the string write behavior from old code
                                        bw.Write((byte)0);
                                    }

                                    await webSocket.SendAsync(ms.ToArray(), WebSocketMessageType.Binary, true, CancellationToken.None);
                                }
                                break;
                            }

                            case 0x41514552: // REQ A (Auth/Attendance)
                            {
                                // Using NEW logic to support Frontend features (DB + Broadcast)
                                var uidBytes = br.ReadBytes(4);
                                // string hexUid = BitConverter.ToString(uidBytes).Replace("-", "").ToUpperInvariant();

                                AttendanceStatus status = AttendanceStatus.Error;
                                string studentName = "";
                                Guid? sessionId = null;
                                Guid? studentId = null;

                                using (var scope = _scopeFactory.CreateScope())
                                {
                                    var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                                    
                                    // 1. Find Device & Classroom
                                    // Note: We are using the DB logic here. If the device fingerprint isn't in the DB, this will fail.
                                    // Ideally, ensure your DB has the device registered with the correct fingerprint.
                                    byte[] fingerprintBytes;
                                    try {
                                        fingerprintBytes = Convert.FromHexString(fingerprint);
                                    } catch {
                                        fingerprintBytes = Array.Empty<byte>();
                                    }

                                    var device = await context.Devices
                                        .Include(d => d.AssignedClassroom)
                                        .FirstOrDefaultAsync(d => d.Fingerprint == fingerprintBytes);

                                    if (device?.AssignedClassroom != null)
                                    {
                                        var now = DateTime.Now;
                                        
                                        // 2. Find Active Timetable (Session)
                                        var potentialSessions = await context.Timetables
                                            .Where(t => t.ClassroomId == device.AssignedClassroom.Id)
                                            .ToListAsync();
                                            
                                        var activeSession = potentialSessions.FirstOrDefault(t => t.Timeslot.DayOfWeek == now.DayOfWeek);
                                        
                                        if (activeSession != null)
                                        {
                                            sessionId = activeSession.Id;

                                            // 3. Find Attendee
                                            var attendee = await context.Attendee
                                                .FirstOrDefaultAsync(a => a.CardUID == uidBytes);

                                            if (attendee != null)
                                            {
                                                studentName = attendee.FullName;
                                                studentId = attendee.Id;

                                                // 4. Register Attendance
                                                var existingLog = await context.AttendanceLogs
                                                    .FirstOrDefaultAsync(l => l.TimetableId == activeSession.Id && 
                                                                              l.AttendeeId == attendee.Id && 
                                                                              l.Date.Date == now.Date);

                                                if (existingLog != null)
                                                {
                                                    status = AttendanceStatus.AlreadyScanned;
                                                }
                                                else
                                                {
                                                    var log = new AttendanceLogModel
                                                    {
                                                        Date = now,
                                                        TimetableId = activeSession.Id,
                                                        AttendeeId = attendee.Id,
                                                        MarkedById = device.Id,
                                                        MarkedByType = "Device",
                                                        IsPresent = true,
                                                        WeekNumber = System.Globalization.ISOWeek.GetWeekOfYear(now)
                                                    };
                                                    context.AttendanceLogs.Add(log);
                                                    await context.SaveChangesAsync();
                                                    status = AttendanceStatus.Success;
                                                }
                                            }
                                            else
                                            {
                                                status = AttendanceStatus.UnrecognizedID;
                                            }
                                        }
                                        else
                                        {
                                            status = AttendanceStatus.NoLecture;
                                        }
                                    }
                                    else
                                    {
                                        // Fallback/Error if device not found in DB
                                        status = AttendanceStatus.Error; 
                                    }
                                }

                                // Send Response to Device
                                using (var ms = new MemoryStream())
                                using (var bw = new BinaryWriter(ms))
                                {
                                    bw.Write(0x53455253); // SRES
                                    bw.Write((byte)status);
                                    bw.Write(Encoding.UTF8.GetBytes(studentName));
                                    bw.Write((byte)0);
                                    await webSocket.SendAsync(ms.ToArray(), WebSocketMessageType.Binary, true, CancellationToken.None);
                                }

                                // Broadcast to Frontend Clients
                                if (status == AttendanceStatus.Success && sessionId.HasValue && studentId.HasValue)
                                {
                                    await _clientHandler.BroadcastUpdateAsync(sessionId.Value, new
                                    {
                                        type = "student_updated",
                                        studentId = studentId.Value.ToString(),
                                        status = "present",
                                        timestamp = DateTime.Now.ToString("o"),
                                        isManual = false
                                    });
                                }

                                break;
                            }

                            default:
                            {
                                byte[] msg = [0];
                                await webSocket.SendAsync(msg, WebSocketMessageType.Binary, true, CancellationToken.None);
                                break;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred handling client {fingerprint}.", client.Id);
        }
        finally
        {
            _connections.TryRemove(client.Id, out _);
            _logger.LogInformation("Client {fingerprint} disconnected.", client.Id);
        }
    }
}