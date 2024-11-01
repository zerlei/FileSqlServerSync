using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Common;

public abstract class AbsPipeLine(bool isAES)
{
    /// <summary>
    /// pipeLine工作函数，生效期间永久阻塞
    /// </summary>
    /// <param name="receiveCb">回调函数，从pipeline接收到的消息</param>
    /// <param name="addr">连接的远程地址</param>
    /// <returns>第一次返回连接信息，第二次当pipeline 被断开时返回，此时可能是正常完成断开，或异常发生断开</returns>
    public abstract IAsyncEnumerable<int> Work(Func<byte[], bool> receiveCb, string addr = "");
    protected Func<byte[], bool> ReceiveMsg = (byte[] a) =>
    {
        return true;
    };

    /// <summary>
    /// 监听pipeline 消息，由Work 函数调用
    /// </summary>
    /// <param name="receiveCb">消息回调</param>
    /// <returns></returns>
    protected abstract Task Listen(Func<byte[], bool> receiveCb);

    /// <summary>
    /// 关闭连接
    /// </summary>
    /// <param name="CloseReason">关闭原因</param>
    /// <returns></returns>
    public abstract Task Close(string? CloseReason);

    /// <summary>
    /// 向管道中发送消息
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    public abstract Task SendMsg(SyncMsg msg);

    /// <summary>
    /// 打包文件上传
    /// </summary>
    /// <param name="url">上传地址</param>
    /// <param name="filePath">上传的文件路径</param>
    /// <param name="progressCb">上传进度回调(现在没有回调)</param>
    /// <returns>上传完成时返回</returns>/
    public abstract Task UploadFile(string url, string filePath, Func<double, bool> progressCb);

    /// <summary>
    ///  管道消息是否使用AES加密
    /// </summary>
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
        using var client = new HttpClient();
        using var content = new MultipartFormDataContent();
        using var fileStream = new FileStream(filePath, FileMode.Open);
        // TODO 上传进度回调
        var progress = new Progress<double>(
            (current) =>
            {
                progressCb(current);
            }
        );
        var fileContent = new ProgressStreamContent(fileStream, progress);
        content.Add(fileContent, "file", Path.GetFileName(filePath));
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
        //warning 最大支持10MB，这由需要同步的文件数量大小决定 UTF-8 每个字符，汉字视为4个字节，数字1个 ，英文字母2个。1MB=256KB*4，25万个字符能描述就行
        var buffer = new byte[10 * 1024 * 1024];

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
            //CloseReason 最大不能超过123bytes.https://developer.mozilla.org/en-US/docs/Web/API/WebSocket/close
            //若传递超过这个限制，此处表现WebSocket将会卡住，无法关闭。
            if (Encoding.UTF8.GetBytes(CloseReason ?? "").Length > 120)
            {
                await SendMsg(
                    new SyncMsg
                    {
                        Type = SyncMsgType.Error,
                        Step = SyncProcessStep.Close,
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
        if (IsAES)
        {
            // 加密后的字符，使用Binary 发送，加密通常不会发送到最前端，通常是 js 写的websocket
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
