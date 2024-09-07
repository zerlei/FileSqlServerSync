using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Common;

namespace RemoteServer;

// enum StateWhenMsg
// {
//     Authority = 0,
//     ConfigInfo = 1,
//     LocalPackAndUpload = 2,
//     RemoteUnPackAndRelease = 3,
// }

public abstract class StateHelpBase(
    RemoteSyncServer context,
    SyncProcessStep step = SyncProcessStep.Connect
)
{
    protected readonly RemoteSyncServer Context = context;

    protected readonly SyncProcessStep Step = step;

    public SyncMsg CreateErrMsg(string Body)
    {
        return new SyncMsg(SyncMsgType.Error, Step, Body);
    }

    public SyncMsg CreateMsg(string body, SyncMsgType type = SyncMsgType.General)
    {
        return new SyncMsg(type, Step, body);
    }

    public bool ReceiveMsg(byte[] bytes)
    {
        var msg = AESHelper.DecryptStringFromBytes_Aes(bytes);

        var syncMsg =
            JsonSerializer.Deserialize<SyncMsg>(msg)
            ?? throw new NullReferenceException("msg is null");
        if (syncMsg.Step != Step)
        {
            throw new Exception("Sync step error!");
        }
        HandleMsg(syncMsg);
        return true;
    }

    protected abstract void HandleMsg(SyncMsg msg);
}

/// <summary>
/// 0. 链接验证
/// </summary>
/// <param name="context"></param>
public class ConnectAuthorityHelper(RemoteSyncServer context)
    : StateHelpBase(context, SyncProcessStep.Connect)
{
    protected override void HandleMsg(SyncMsg msg)
    {
        if (msg.Body == Context.Pwd)
        {
            Context.Pipe.SendMsg(CreateMsg("RemoteServer: 密码验证成功！"));
        }
        else
        {
            throw new Exception("密码错误！");
        }
    }
}

public class DiffFileHelper(RemoteSyncServer context)
    : StateHelpBase(context, SyncProcessStep.DiffFileAndPack)
{
    protected override void HandleMsg(SyncMsg msg)
    {
        Context.SyncConfig = JsonSerializer.Deserialize<Config>(msg.Body);
        //文件对比
        Context.NotNullSyncConfig.DirFileConfigs.ForEach(e =>
        {
            if (e.DirInfo == null)
            {
                throw new NullReferenceException("RemoteServer: 发布的文件为空！--这个异常应该永远不会发生~");
            }
            else
            {
                var nd = e.DirInfo.Clone();
                nd.ResetRootPath(
                    Context.NotNullSyncConfig.LocalRootPath,
                    Context.NotNullSyncConfig.RemoteRootPath
                );
                nd.ExtractInfo(e.CherryPicks, e.Excludes);
                var diff = e.DirInfo.Diff(nd);
                e.DirInfo = diff;
            }
        });
        //将对比结果发送到Local
        Context.Pipe.SendMsg(
            CreateMsg(JsonSerializer.Serialize(Context.NotNullSyncConfig.DirFileConfigs))
        );
    }
}
public class UnPackFilesHelper(RemoteSyncServer context):StateHelpBase(context,SyncProcessStep.UploadAndUnpack)
{
    protected override void HandleMsg(SyncMsg msg)
    {
        throw new NotImplementedException();
    }
}