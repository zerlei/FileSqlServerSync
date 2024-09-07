using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Common;
using LocalServer.Models;

namespace LocalServer;

public class LocalSyncServer
{
    public StateHelpBase StateHelper;

    public Config? SyncConfig;

    /// <summary>
    /// 发布源连接
    /// </summary>
    public readonly WebSocket LocalSocket;

    /// <summary>
    /// 发布源-缓冲区，存储数据 最大1MB
    /// </summary>
    public byte[] Buffer = new byte[1024 * 1024];

    /// <summary>
    /// 发布目标-连接
    /// </summary>
    public readonly ClientWebSocket RemoteSocket = new();

    /// <summary>
    /// 发布开始时间
    /// </summary>
    private readonly DateTime StartTime = DateTime.Now;

    /// <summary>
    /// 发布名称
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// 父工程，用于释放资源
    /// </summary>
    public readonly LocalSyncServerFactory Factory;

    public LocalSyncServer(WebSocket socket, string name, LocalSyncServerFactory factory)
    {
        LocalSocket = socket;
        Name = name;
        Factory = factory;
        StateHelper = new LocalAuthorityState(this);
    }

    public async Task Start()
    {
        //最大1MB
        var buffer = new byte[1024 * 1024];
        var receiveResult = await LocalSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer),
            CancellationToken.None
        );

        while (!receiveResult.CloseStatus.HasValue)
        {
            await LocalSocket.SendAsync(
                new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                receiveResult.MessageType,
                receiveResult.EndOfMessage,
                CancellationToken.None
            );

            receiveResult = await LocalSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                CancellationToken.None
            );
        }
        Factory.RemoveLocalSyncServer(this);
        await LocalSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None
        );
    }

    public async void LocalSocketSendMsg(object msgOb)
    {
        string msg = JsonSerializer.Serialize(msgOb);
        await LocalSocket.SendAsync(
            new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg)),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );
    }

    public async void RemoteSocketSendMsg(object msgOb)
    {
        string msg = JsonSerializer.Serialize(msgOb);
        var buffer = AESHelper.EncryptStringToBytes_Aes(msg);
        await RemoteSocket.SendAsync(
            buffer,
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );
    }

    public void Close() { }
}
