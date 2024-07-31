using System.Net.NetworkInformation;
using System.Net.WebSockets;
using System.Runtime.Intrinsics.Arm;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json;
using Common;
using LocalServer.Models;

namespace LocalServer;

// enum StateWhenMsg
// {
//     Authority = 0,
//     ConfigInfo = 1,
//     LocalPackAndUpload = 2,
//     RemoteUnPackAndRelease = 3,
// }

public abstract class StateHelpBase(LocalSyncServer context)
{
    protected readonly LocalSyncServer Context = context;

    public abstract void HandleRemoteMsg(SyncMsg? msg);

    public abstract void HandleLocalMsg(SyncMsg? msg);
}

/// <summary>
/// 0. 发布源验证密码
/// </summary>
/// <param name="context"></param>
public class LocalAuthorityState(LocalSyncServer context) : StateHelpBase(context)
{
    public override void HandleRemoteMsg(SyncMsg? msg)
    {
        throw new NotImplementedException("error usage!");
    }

    public override void HandleLocalMsg(SyncMsg? msg)
    {
        if (msg == null)
        {
            return;
        }
        else
        {
            string Pwd = msg.Body;
            if (Pwd == "Xfs1%$@_fdYU.>>")
            {
                Context.LocalSocketSendMsg(new SyncMsg(true, "源服务密码校验成功！"));
                Context.StateHelper = new WaitingConfigInfoState(Context);
            }
            else
            {
                throw new UnauthorizedAccessException("pwd error!");
            }
        }
    }
}

/// <summary>
/// 1. 获取配置信息，它包含目标的服务器的配置信息
/// </summary>
/// <param name="context"></param>
public class WaitingConfigInfoState(LocalSyncServer context) : StateHelpBase(context)
{
    public override void HandleRemoteMsg(SyncMsg? msg) { }

    public override void HandleLocalMsg(SyncMsg? msg)
    {
        if (msg == null)
        {
            return;
        }
        else
        {
            string ConfigInfo = msg.Body;
            Context.SyncConfig =
                JsonSerializer.Deserialize<Config>(ConfigInfo)
                ?? throw new NullReferenceException("ConfigInfo is null");
            var task = Context.RemoteSocket.ConnectAsync(
                new Uri(Context.SyncConfig.RemoteUrl),
                CancellationToken.None
            );
            if (task.Wait(10000))
            {
                if (Context.RemoteSocket.State == WebSocketState.Open)
                {
                    var state = new RemoteAuthorityState(Context);
                    state.SendPwdToRemoteServer();
                    Context.StateHelper = state;
                }
                else
                {
                    throw new Exception("connect remote server failed!");
                }
            }
            else
            {
                throw new TimeoutException("connect remote server timeout");
            }
        }
    }
}

/// <summary>
/// 2. 目标服务器权限校验
/// </summary>
/// <param name="context"></param>
public class RemoteAuthorityState(LocalSyncServer context) : StateHelpBase(context)
{
    public override void HandleRemoteMsg(SyncMsg? msg)
    {
        if (msg == null)
        {
            return;
        }
        else { }
    }

    public override void HandleLocalMsg(SyncMsg? msg) { }

    public void SendPwdToRemoteServer()
    {
        var authorityInfo = new
        {
            Pwd = "xfs@#123hd??1>>|12#4",
            MacAdr = new LocalServer.Controllers.LocalServerController(
                Context.Factory
            ).GetMacAddress()
        };
        Context.RemoteSocketSendMsg(authorityInfo);
    }
}

/// <summary>
/// 3. 文件比较
/// </summary>
/// <param name="context"></param>
public class DirFilesDiffState(LocalSyncServer context) : StateHelpBase(context)
{
    public override void HandleRemoteMsg(SyncMsg? msg)
    {
        if (msg == null)
        {
            return;
        }
        else { }
    }

    public override void HandleLocalMsg(SyncMsg? msg) { }
}

/// <summary>
/// 4. 本地打包并上传
/// </summary>
/// <param name="context"></param>
public class LocalPackAndUploadState(LocalSyncServer context) : StateHelpBase(context)
{
    public override void HandleRemoteMsg(SyncMsg? msg)
    {
        if (msg == null)
        {
            return;
        }
        else { }
    }

    public override void HandleLocalMsg(SyncMsg? msg) { }
}

/// <summary>
/// 5. 目标服务器解包并发布
/// </summary>
/// <param name="context"></param>
public class RemoteUnPackAndReleaseState(LocalSyncServer context) : StateHelpBase(context)
{
    public override void HandleRemoteMsg(SyncMsg? msg)
    {
        if (msg == null)
        {
            return;
        }
        else { }
    }

    public override void HandleLocalMsg(SyncMsg? msg) { }
}
