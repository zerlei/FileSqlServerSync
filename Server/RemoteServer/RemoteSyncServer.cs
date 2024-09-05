using System.Net.WebSockets;
using System.Text.Json;
using Common;

namespace RemoteServer;

public class RemoteSyncServer
{
#pragma warning disable CA2211 // Non-constant fields should not be visible
    public static string TempRootFile = "C:/TempPack";
#pragma warning restore CA2211 // Non-constant fields should not be visible
    // public StateHelpBase StateHelper;

    public Config? SyncConfig;

    public Config NotNullSyncConfig {get {
        if (SyncConfig == null)
        {
            throw new ArgumentNullException("SyncConfig");
        }
        return SyncConfig;
    }}

    /// <summary>
    /// remote server
    /// </summary>
    public readonly WebSocket RemoteSocket;

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
    public readonly RemoteSyncServerFactory Factory;

    public RemoteSyncServer(WebSocket socket, string name, RemoteSyncServerFactory factory)
    {
        RemoteSocket = socket;
        Name = name;
        Factory = factory;
        // StateHelper = new ConnectAuthorityHelper(this);
    }


    public async Task RemoteSocketListen()
    {
        string CloseMsg = "任务结束关闭";
        try
        {
            //最大1MB!=
            var buffer = new byte[1024 * 1024];

            while (RemoteSocket.State == WebSocketState.Open)
            {
                var receiveResult = await RemoteSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    CancellationToken.None
                );

                if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    Close(receiveResult.CloseStatusDescription);
                }
                else
                {
                    // StateHelper.ReceiveLocalMsg(
                    //     Encoding.UTF8.GetString(buffer, 0, receiveResult.Count)
                    // );
                }
            }
        }
        catch (Exception e)
        {
            CloseMsg = e.Message;
        }
        finally
        {
            Close(CloseMsg);
        }
    }

    public async Task RemoteSocketSendMsg(object msgOb)
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

    public void Close(string? CloseReason)
    {
        try
        {
            if (RemoteSocket.State == WebSocketState.Open)
            {
                RemoteSocket
                    .CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        CloseReason,
                        CancellationToken.None
                    )
                    .Wait(60 * 1000);
            }

            if (RemoteSocket.State == WebSocketState.Open)
            {
                RemoteSocket
                    .CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        CloseReason,
                        CancellationToken.None
                    )
                    .Wait(60 * 1000);
            }
        }
        catch (Exception e)
        {
            //TODO 日志
            Console.WriteLine(e.Message);
        }
        finally
        {
            Factory.RemoveLocalSyncServer(this);
        }
    }
}
