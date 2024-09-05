using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Common;

public abstract class AbsPipeLine
{
    public abstract IAsyncEnumerable<int> Work(Func<byte[], bool> receiveCb, string addr = "");
    protected Func<byte[], bool> ReceiveMsg = (byte[] a) =>
    {
        return true;
    };
    protected abstract Task Listen(Func<byte[], bool> receiveCb);

    /// <summary>
    /// 关闭连接
    /// </summary>
    /// <param name="CloseReason"></param>
    /// <returns></returns>
    public abstract Task Close(string? CloseReason);

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    public abstract Task SendMsg(SyncMsg msg);
}

public class WebSocPipeLine<TSocket>(TSocket socket) : AbsPipeLine
    where TSocket : WebSocket
{
    public readonly TSocket Socket = socket;

    public override async IAsyncEnumerable<int> Work(
        Func<byte[], bool> receiveCb,
        string addr = ""
    )
    {
        if (Socket is ClientWebSocket CSocket)
        {
            //连接失败会抛出异常
            await CSocket.ConnectAsync(new Uri(addr), CancellationToken.None);
            yield return 0;
        }
        // 从controller 来，这个已经连接上了
        else if (Socket is WebSocket)
        {
            yield return 0;
        }
        await Listen(receiveCb);
        yield return 1;
    }

    protected override async Task Listen(Func<byte[], bool> receiveCb)
    {
        //最大1MB!=
        var buffer = new byte[1024 * 1024];

        while (Socket.State == WebSocketState.Open)
        {
            var receiveResult = await Socket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                CancellationToken.None
            );

            if (receiveResult.MessageType == WebSocketMessageType.Close)
            {
                throw new Exception(receiveResult.CloseStatusDescription);
            }
            else
            {
                var nbuffer = new byte[receiveResult.Count];
                System.Buffer.BlockCopy(buffer, 0, nbuffer, 0, receiveResult.Count);
                receiveCb(nbuffer);
            }
        }
    }

    public override async Task Close(string? CloseReason)
    {
        if (Socket.State == WebSocketState.Open)
        {
            await Socket.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                CloseReason,
                CancellationToken.None
            );
        }
    }

    public override async Task SendMsg(SyncMsg msg)
    {
        string msgStr = JsonSerializer.Serialize(msg);
        await Socket.SendAsync(
            new ArraySegment<byte>(Encoding.UTF8.GetBytes(msgStr)),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );
    }
}
