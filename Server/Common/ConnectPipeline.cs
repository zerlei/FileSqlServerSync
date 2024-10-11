using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Common;

public abstract class AbsPipeLine(bool isAES)
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

    public abstract Task UploadFile(string url, string filePath, Func<double, bool> progressCb);
    protected readonly bool IsAES = isAES;
}

public class WebSocPipeLine<TSocket>(TSocket socket, bool isAES) : AbsPipeLine(isAES)
    where TSocket : WebSocket
{
    public readonly TSocket Socket = socket;

    public override async Task UploadFile(
        string url,
        string filePath,
        Func<double, bool> progressCb
    )
    {
        //throw new Exception("sdfsdf");
        using var client = new HttpClient();
        using var content = new MultipartFormDataContent();
        using var fileStream = new FileStream(filePath, FileMode.Open);
        var progress = new Progress<double>(
            (current) =>
            {
                progressCb(current);
            }
        );
        //var fileContent = new ProgressStreamContent(fileStream, progress);
        content.Add(new StreamContent(fileStream), "file", Path.GetFileName(filePath));
        var it = await client.PostAsync("http://" + url + "/UploadFile", content);
        if (it.StatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new Exception(it.Content.ReadAsStringAsync().Result);
        }
    }

    public override async IAsyncEnumerable<int> Work(Func<byte[], bool> receiveCb, string addr = "")
    {
        if (Socket is ClientWebSocket CSocket)
        {
            //连接失败会抛出异常
            await CSocket.ConnectAsync(new Uri("ws://" + addr), CancellationToken.None);
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
                if (IsAES)
                {
                    //var msg1 = Encoding.UTF8.GetString(nbuffer);
                    //var n1Buffler = Encoding.UTF8.GetBytes(msg1);
                    var nnbuffer = AESHelper.DecryptStringFromBytes_Aes(nbuffer);
                    receiveCb(Encoding.UTF8.GetBytes(nnbuffer));
                }
                else
                {
                    receiveCb(nbuffer);
                }
            }
        }
    }

    public override async Task Close(string? CloseReason)
    {
        if (Socket.State == WebSocketState.Open)
        {
            //await SendMsg(
            //    new SyncMsg
            //    {
            //        Type = SyncMsgType.Error,
            //        Step = SyncProcessStep.Finally,
            //        Body = CloseReason ?? ""
            //    }
            //);
            if (Encoding.UTF8.GetBytes(CloseReason ?? "").Length > 120)
            {
                await SendMsg(
                    new SyncMsg
                    {
                        Type = SyncMsgType.Error,
                        Step = SyncProcessStep.CloseError,
                        Body = CloseReason ?? ""
                    }
                );
                await Socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "查看上一条错误信息！",
                    CancellationToken.None
                );
            }
            else
            {
                await Socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    CloseReason,
                    CancellationToken.None
                );
            }
        }
    }

    public override async Task SendMsg(SyncMsg msg)
    {
        string msgStr = JsonSerializer.Serialize(msg);
        var it = AESHelper.EncryptStringToBytes_Aes(msgStr);
        //var xx = new ArraySegment<byte>(it);
        if (IsAES)
        {
            await Socket.SendAsync(
                new ArraySegment<byte>(it),
                WebSocketMessageType.Binary,
                true,
                CancellationToken.None
            );
        }
        else
        {
            await Socket.SendAsync(
                new ArraySegment<byte>(Encoding.UTF8.GetBytes(msgStr)),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
        }
    }
}
