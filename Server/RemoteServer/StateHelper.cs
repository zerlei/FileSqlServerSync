using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
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
        return new SyncMsg
        {
            Body = Body,
            Type = SyncMsgType.Error,
            Step = Step
        };
    }

    public SyncMsg CreateMsg(string body, SyncMsgType type = SyncMsgType.General)
    {
        return new SyncMsg
        {
            Body = body,
            Type = type,
            Step = Step
        };
    }

    public bool ReceiveMsg(byte[] bytes)
    {
        var msg = Encoding.UTF8.GetString(bytes);

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
            var h = new DiffFileHelper(Context);
            Context.StateHelper = h;
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

        var diffConfigs = new List<DirFileConfig>();
        //文件对比
        Context.NotNullSyncConfig.DirFileConfigs.ForEach(e =>
        {
            if (e.LocalDirInfo == null)
            {
                throw new NullReferenceException("RemoteServer: 发布的文件为空！--这个异常应该永远不会发生~");
            }
            else
            {
                var nd = e.LocalDirInfo.Clone();
                nd.ResetRootPath(
                    Context.NotNullSyncConfig.LocalRootPath,
                    Context.NotNullSyncConfig.RemoteRootPath
                );
                nd.ExtractInfo(e.CherryPicks, e.Excludes);
                e.DiffDirInfo = nd.Diff(nd);
                e.RemoteDirInfo = nd;
                diffConfigs.Add(
                    new DirFileConfig { DiffDirInfo = e.DiffDirInfo, DirPath = e.DirPath }
                );
            }
        });
        var h = new UnPackAndReleaseHelper(Context);
        Context.StateHelper = h;
        //将对比结果发送到Local
        Context.Pipe.SendMsg(CreateMsg(JsonSerializer.Serialize(diffConfigs)));
    }
}

public class UnPackAndReleaseHelper(RemoteSyncServer context)
    : StateHelpBase(context, SyncProcessStep.UploadAndUnpack)
{
    public void UnPack()
    {
        FileDirOpForUnpack.FirstUnComparess(
            Path.Combine(RemoteSyncServer.TempRootFile, Context.NotNullSyncConfig.Id.ToString()),
            Context.NotNullSyncConfig.Id.ToString()
        );
        Context.Pipe.SendMsg(CreateMsg("解压完成！")).Wait();
        var h = new FinallyPublishHelper(Context);
        Context.StateHelper = h;
        h.FinallyPublish();
    }

    protected override void HandleMsg(SyncMsg msg) { }
}

public class FinallyPublishHelper(RemoteSyncServer context)
    : StateHelpBase(context, SyncProcessStep.Publish)
{
    public void FinallyPublish()
    {
        // 发布数据库
        if (Context.NotNullSyncConfig.IsDeployDb)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var arguments =
                    $"/Action:Publish  /SourceFile: {RemoteSyncServer.TempRootFile}/{Context.NotNullSyncConfig.Id}/{Context.NotNullSyncConfig.Id}.dacpac "
                    + $"/TargetServerName:{Context.NotNullSyncConfig.DstDb.ServerName} /TargetDatabaseName:{Context.NotNullSyncConfig.DstDb.DatebaseName}"
                    + $" /TargetUser:{Context.NotNullSyncConfig.DstDb.User} /TargetPassword:{Context.NotNullSyncConfig.DstDb.Password} /TargetTrustServerCertificate:True ";

                ProcessStartInfo startInfo =
                    new()
                    {
                        FileName = "SqlPackage", // The command to execute (can be any command line tool)
                        Arguments = arguments,
                        // The arguments to pass to the command (e.g., list directory contents)
                        RedirectStandardOutput = true, // Redirect the standard output to a string
                        UseShellExecute = false, // Do not use the shell to execute the command
                        CreateNoWindow = true // Do not create a new window for the command
                    };
                using Process process = new() { StartInfo = startInfo };
                // Start the process
                process.Start();

                // Read the output from the process
                string output = process.StandardOutput.ReadToEnd();

                // Wait for the process to exit
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Context.Pipe.SendMsg(CreateMsg("数据库发布成功！")).Wait();
                }
                else
                {
                    Context.Pipe.SendMsg(CreateErrMsg(output)).Wait();
                    throw new Exception("执行发布错误，错误信息参考上一条消息！");
                }
            }
            else
            {
                throw new NotSupportedException("只支持windows!");
            }
        }
        else
        {
            Context.Pipe.SendMsg(CreateMsg("跳过数据库发布！")).Wait();
        }
        var DirFileOp = new FileDirOpForUnpack(
            Path.Combine(
                RemoteSyncServer.TempRootFile,
                Context.NotNullSyncConfig.Id.ToString(),
                Context.NotNullSyncConfig.Id.ToString()
            ),
            Context.NotNullSyncConfig.RemoteRootPath
        );
        Context.NotNullSyncConfig.DirFileConfigs.ForEach(e =>
        {
            if (e.RemoteDirInfo != null && e.DiffDirInfo != null)
            {
                e.RemoteDirInfo.Combine(DirFileOp, e.DiffDirInfo,false,true);
            }
        });

        Context.Pipe.SendMsg(CreateMsg("发布完成！")).Wait();
    }

    protected override void HandleMsg(SyncMsg msg) { }
}
