using System.Text;

namespace CloudProject.API.Services;

public record WebSocketConnection(string Id, WebSocket Socket, DateTime ConnectedAt);

public class ModuleHandler : IModuleHandler
{
    private readonly ILogger<ModuleHandler> _logger;
    private readonly ConcurrentDictionary<string, WebSocketConnection> _connections;

    public ModuleHandler(ILogger<ModuleHandler> logger)
    {
        _logger = logger;
        _connections = new();
    }

    private List<string> alreadyScanned = new();
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
                    using (var __ms = new MemoryStream(buffer))
                    using (var br = new BinaryReader(__ms))
                    {
                        var pkt = br.ReadInt32();

                        switch (pkt)
                        {
                            case 0x49514552:
                            {
                                using (var ms = new MemoryStream())
                                using (var bw = new BinaryWriter(ms))
                                {
                                    bw.Write(0x464E494D);
                                    if (fingerprint[0] == 'B')
                                    {
                                        bw.Write("S-113"u8);
                                        bw.Write((byte)0);
                                        bw.Write((byte)0);
                                    }
                                    else
                                    {
                                        bw.Write("S-114"u8);
                                        bw.Write((byte)0);
                                        bw.Write("CNG491");
                                        bw.Write((byte)0);
                                    }

                                    await webSocket.SendAsync(ms.ToArray(), WebSocketMessageType.Binary, true, CancellationToken.None);
                                }
                                
                                break;
                            }

                            case 0x41514552:
                            {
                                Dictionary<string, string> uids = new()
                                {
                                    { "B39FB8AD", "Can Özkan" },
                                    { "B27ECC3F", "John Doe" }
                                };

                                string hex = BitConverter.ToString(buffer)
                                    .Replace("-", "")
                                    .ToUpperInvariant();

                                AttendanceStatus status = AttendanceStatus.Error;
                                string name = string.Empty;

                                if (fingerprint[0] == '8')
                                {
                                    if (uids.TryGetValue(hex, out name))
                                    {
                                        if (!alreadyScanned.Contains(hex))
                                        {
                                            status = AttendanceStatus.Success;
                                            alreadyScanned.Add(hex);
                                        }
                                        else
                                        {
                                            status = AttendanceStatus.AlreadyScanned;
                                        }
                                    }
                                    else
                                    {
                                        status = AttendanceStatus.UnrecognizedId;
                                    }
                                }
                                else
                                {
                                    status = AttendanceStatus.NoLecture;
                                }

                                using (var ms = new MemoryStream())
                                using (var bw = new BinaryWriter(ms))
                                {
                                    bw.Write(0x53455253);
                                    bw.Write((byte)status);
                                    bw.Write(Encoding.UTF8.GetBytes(name));
                                    bw.Write((byte)0);
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
