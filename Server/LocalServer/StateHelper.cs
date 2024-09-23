using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Common;

namespace LocalServer;

// enum StateWhenMsg
// {
//     Authority = 0,
//     ConfigInfo = 1,
//     LocalPackAndUpload = 2,
//     RemoteUnPackAndRelease = 3,
// }

public abstract class StateHelpBase(
    LocalSyncServer context,
    SyncProcessStep step = SyncProcessStep.Connect
)
{
    protected readonly LocalSyncServer Context = context;

    protected readonly SyncProcessStep Step = step;

    public SyncMsg CreateErrMsg(string Body)
    {
        return new SyncMsg
        {
            Body = Body,
            Step = Step,
            Type = SyncMsgType.Error
        };
    }

    public SyncMsg CreateMsg(string body, SyncMsgType type = SyncMsgType.General)
    {
        return new SyncMsg
        {
            Body = body,
            Step = Step,
            Type = type
        };
    }

    public bool ReceiveLocalMsg(byte[] msg)
    {
        var syncMsg =
            JsonSerializer.Deserialize<SyncMsg>(msg)
            ?? throw new NullReferenceException("msg is null");
        if (syncMsg.Step != Step)
        {
            throw new Exception("Sync step error!");
        }
        HandleLocalMsg(syncMsg);
        return true;
    }

    public bool ReceiveRemoteMsg(byte[] bytes)
    {
        var msg = Encoding.UTF8.GetString(bytes);

        var syncMsg =
            JsonSerializer.Deserialize<SyncMsg>(msg)
            ?? throw new NullReferenceException("msg is null");
        if (syncMsg.Step != Step)
        {
            throw new Exception("Sync step error!");
        }
        HandleRemoteMsg(syncMsg);
        return true;
    }

    protected abstract void HandleRemoteMsg(SyncMsg msg);

    protected abstract void HandleLocalMsg(SyncMsg msg);
}

/// <summary>
/// 0. 链接验证
/// </summary>
/// <param name="context"></param>
public class ConnectAuthorityHelper(LocalSyncServer context)
    : StateHelpBase(context, SyncProcessStep.Connect)
{
    // 如果密码错误，那么就直接关闭连接，不会进入这个方法
    protected override void HandleRemoteMsg(SyncMsg msg)
    {
        //将remote的消息传递到前端界面
        Context.LocalPipe.SendMsg(msg);
        //下一步
        var deployHelper = new DeployHelper(Context);
        Context.StateHelper = deployHelper;
        deployHelper.DeployProcess();
    }

    protected override void HandleLocalMsg(SyncMsg msg)
    {
        //收到配置文件
        var config = JsonSerializer.Deserialize<Config>(msg.Body);
        Context.SyncConfig = config;
        Task.Run(async () =>
        {
            try
            {
                var rs = Context.RemotePipe.Work(
                    (byte[] b) =>
                    {
                        return Context.StateHelper.ReceiveRemoteMsg(b);
                    },
                    Context.NotNullSyncConfig.RemoteUrl + "/websoc?Name=" + Context.Name
                );
                await foreach (var r in rs)
                {
                    if (r == 0)
                    {
                        await Context.RemotePipe.SendMsg(
                            CreateMsg(Context.NotNullSyncConfig.RemotePwd)
                        );
                    }
                }
            }
            catch (Exception e)
            {
                await Context.LocalPipe.Close(e.Message);
            }
        });
    }
}

/// <summary>
/// 1. 执行发布步骤
/// </summary>
/// <param name="context"></param>
public class DeployHelper(LocalSyncServer context)
    : StateHelpBase(context, SyncProcessStep.DeployProject)
{
    public void DeployProcess()
    {
        if (Context.NotNullSyncConfig.IsDeployProject == false)
        {
            Context.LocalPipe.SendMsg(CreateMsg("配置为不发布跳过此步骤")).Wait();
            var h = new DiffFileAndPackHelper(Context);
            Context.StateHelper = h;
            h.DiffProcess();
        }
        else
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ProcessStartInfo startInfo =
                    new()
                    {
                        FileName = "msdeploy", // The command to execute (can be any command line tool)
                        Arguments =
                            $" -verb:sync -source:contentPath={Context.NotNullSyncConfig.LocalProjectAbsolutePath} -dest:contentPath={Context.NotNullSyncConfig.LocalRootPath} -disablerule:BackupRule",
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
                    Context.LocalPipe.SendMsg(CreateMsg("发布成功！")).Wait();
                    var h = new DiffFileAndPackHelper(Context);
                    Context.StateHelper = h;
                    h.DiffProcess();
                }
                else
                {
                    Context.LocalPipe.SendMsg(CreateErrMsg(output)).Wait();
                    throw new Exception("执行发布错误，错误信息参考上一条消息！");
                }
            }
            else
            {
                throw new NotSupportedException("只支持windows!");
            }
        }
    }

    protected override void HandleRemoteMsg(SyncMsg msg) { }

    protected override void HandleLocalMsg(SyncMsg msg) { }
}

public class DiffFileAndPackHelper(LocalSyncServer context)
    : StateHelpBase(context, SyncProcessStep.DiffFileAndPack)
{
    public void DiffProcess()
    {
        //提取本地文件的信息
        Context.NotNullSyncConfig.DirFileConfigs.ForEach(e =>
        {
            e.LocalDirInfo = new Dir(Context.NotNullSyncConfig.LocalRootPath + e.DirPath);
            e.LocalDirInfo.ExtractInfo(e.CherryPicks, e.Excludes);
        });
        //将配置信息发送到remoteServer
        Context
            .RemotePipe.SendMsg(CreateMsg(JsonSerializer.Serialize(Context.NotNullSyncConfig)))
            .Wait();
    }

    protected override void HandleLocalMsg(SyncMsg msg) { }

    protected override void HandleRemoteMsg(SyncMsg msg)
    {
        var diffConfig =
            JsonSerializer.Deserialize<List<DirFileConfig>>(msg.Body)
            ?? throw new Exception("LocalServer: DirFile为空！");

        for (var i = 0; i < diffConfig.Count; ++i)
        {
            Context.NotNullSyncConfig.DirFileConfigs[i].DiffDirInfo = diffConfig[i].DiffDirInfo;
        }

        var PackOp = new FileDirOpForPack(
            Context.NotNullSyncConfig.LocalRootPath,
            LocalSyncServer.TempRootFile + "/" + Context.NotNullSyncConfig.Id.ToString()
        );
        Context.NotNullSyncConfig.DirFileConfigs.ForEach(e =>
        {
            if (e.DiffDirInfo != null)
            {
                e.DiffDirInfo.ResetRootPath(
                    Context.NotNullSyncConfig.RemoteRootPath,
                    Context.NotNullSyncConfig.LocalRootPath
                );
                e.DiffDirInfo.WriteByThisInfo(PackOp);
            }
        });
        var n = new DeployMSSqlHelper(Context);
        n.PackSqlServerProcess();
        Context.StateHelper = n;
    }
}

public class DeployMSSqlHelper(LocalSyncServer context)
    : StateHelpBase(context, SyncProcessStep.PackSqlServer)
{
    private void PackAndSwitchNext()
    {
        FileDirOpForPack.FinallyCompress(
            LocalSyncServer.TempRootFile + "/" + Context.NotNullSyncConfig.Id.ToString(),
            Context.NotNullSyncConfig.Id.ToString()
        );
        var h = new UploadPackedHelper(Context);
        Context.StateHelper = h;
        h.UpLoadPackedFile();
    }

    public void PackSqlServerProcess()
    {
        if (Context.NotNullSyncConfig.IsDeployDb == false)
        {
            Context.LocalPipe.SendMsg(CreateMsg("配置为不发布数据库跳过此步骤")).Wait();
            PackAndSwitchNext();
        }
        else
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var arguments =
                    $" /Action:Extract /TargetFile:{LocalSyncServer.TempRootFile}/{Context.NotNullSyncConfig.Id.ToString()}/{Context.NotNullSyncConfig.Id.ToString()}.dacpac "
                    + $"/DiagnosticsFile:{LocalSyncServer.TempRootFile}/{Context.NotNullSyncConfig.Id.ToString()}/{Context.NotNullSyncConfig.Id.ToString()}.log "
                    + $"/p:ExtractAllTableData=false /p:VerifyExtraction=true /SourceServerName:{Context.NotNullSyncConfig.SrcDb.ServerName}"
                    + $"/SourceDatabaseName:{Context.NotNullSyncConfig.SrcDb.DatebaseName} /SourceUser:{Context.NotNullSyncConfig.SrcDb.User} "
                    + $"/SourcePassword:{Context.NotNullSyncConfig.SrcDb.Password} /SourceTrustServerCertificate:{Context.NotNullSyncConfig.SrcDb.TrustServerCertificate} "
                    + $"/p:ExtractReferencedServerScopedElements=False /p:IgnoreUserLoginMappings=True /p:IgnorePermissions=True ";
                if (Context.NotNullSyncConfig.SrcDb.SyncTablesData != null)
                {
                    foreach (var t in Context.NotNullSyncConfig.SrcDb.SyncTablesData)
                    {
                        arguments += $" /p:TableData={t}";
                    }
                }

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
                    Context.LocalPipe.SendMsg(CreateMsg("数据库打包成功！")).Wait();
                    PackAndSwitchNext();
                }
                else
                {
                    Context.LocalPipe.SendMsg(CreateErrMsg(output)).Wait();
                    throw new Exception("执行发布错误，错误信息参考上一条消息！");
                }
            }
            else
            {
                throw new NotSupportedException("只支持windows!");
            }
        }
    }

    protected override void HandleLocalMsg(SyncMsg msg) { }

    protected override void HandleRemoteMsg(SyncMsg msg)
    {
        throw new NotImplementedException();
    }
}

public class UploadPackedHelper(LocalSyncServer context)
    : StateHelpBase(context, SyncProcessStep.UploadAndUnpack)
{
    public void UpLoadPackedFile()
    {
        Context
            .LocalPipe.UploadFile(
                Context.NotNullSyncConfig.RemoteUrl,
                $"{LocalSyncServer.TempRootFile}/{Context.NotNullSyncConfig.Id}/{Context.NotNullSyncConfig.Id}.zip",
                (double current) =>
                {
                    Context
                        .LocalPipe.SendMsg(CreateMsg(current.ToString(), SyncMsgType.Process))
                        .Wait();
                    return true;
                }
            )
            .Wait();
        Context.LocalPipe.SendMsg(CreateMsg("上传完成！")).Wait();
    }

    protected override void HandleLocalMsg(SyncMsg msg)
    {
        throw new NotImplementedException();
    }

    protected override void HandleRemoteMsg(SyncMsg msg)
    {
        Context.LocalPipe.SendMsg(msg).Wait();
        var h = new FinallyPublishHelper(Context);
        Context.StateHelper = h;
    }
}

public class FinallyPublishHelper(LocalSyncServer context)
    : StateHelpBase(context, SyncProcessStep.Publish)
{
    protected override void HandleLocalMsg(SyncMsg msg)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 由最初始的客户端断开连接，表示发布完成。
    /// </summary>
    /// <param name="msg"></param>
    protected override void HandleRemoteMsg(SyncMsg msg)
    {
        Context.LocalPipe.SendMsg(msg).Wait();
    }
}
// /// <summary>
// /// 0. 发布源验证密码
// /// </summary>
// /// <param name="context"></param>
// public class LocalAuthorityState(LocalSyncServer context) : StateHelpBase(context)
// {
//     public override void HandleRemoteMsg(SyncMsg? msg)
//     {
//         throw new NotImplementedException("error usage!");
//     }

//     public override void HandleLocalMsg(SyncMsg? msg)
//     {
//         if (msg == null)
//         {
//             return;
//         }
//         else
//         {
//             string Pwd = msg.Body;
//             if (Pwd == "Xfs1%$@_fdYU.>>")
//             {
//                 Context.LocalSocketSendMsg(new SyncMsg(SyncMsgType.Success, "源服务密码校验成功！"));
//                 Context.StateHelper = new WaitingConfigInfoState(Context);
//             }
//             else
//             {
//                 throw new UnauthorizedAccessException("pwd error!");
//             }
//         }
//     }
// }

// /// <summary>
// /// 1. 获取配置信息，它包含目标的服务器的配置信息
// /// </summary>
// /// <param name="context"></param>
// public class WaitingConfigInfoState(LocalSyncServer context) : StateHelpBase(context)
// {
//     public override void HandleRemoteMsg(SyncMsg? msg) { }

//     public override void HandleLocalMsg(SyncMsg? msg)
//     {
//         if (msg == null)
//         {
//             return;
//         }
//         else
//         {
//             string ConfigInfo = msg.Body;
//             Context.SyncConfig =
//                 JsonSerializer.Deserialize<Config>(ConfigInfo)
//                 ?? throw new NullReferenceException("ConfigInfo is null");
//             var task = Context.RemoteSocket.ConnectAsync(
//                 new Uri(Context.SyncConfig.RemoteUrl),
//                 CancellationToken.None
//             );
//             if (task.Wait(10000))
//             {
//                 if (Context.RemoteSocket.State == WebSocketState.Open)
//                 {
//                     var state = new RemoteAuthorityState(Context);
//                     state.SendPwdToRemoteServer();
//                     Context.StateHelper = state;
//                 }
//                 else
//                 {
//                     throw new Exception("connect remote server failed!");
//                 }
//             }
//             else
//             {
//                 throw new TimeoutException("connect remote server timeout");
//             }
//         }
//     }
// }

// /// <summary>
// /// 2. 目标服务器权限校验
// /// </summary>
// /// <param name="context"></param>
// public class RemoteAuthorityState(LocalSyncServer context) : StateHelpBase(context)
// {
//     public override void HandleRemoteMsg(SyncMsg? msg)
//     {
//         if (msg == null)
//         {
//             return;
//         }
//         else
//         {
//             if (msg.Type == SyncMsgType.Success)
//             {
//                 Context.LocalSocketSendMsg(new SyncMsg(SyncMsgType.Success, "远程服务器校验成功！"));
//                 var diffState = new DirFilesDiffState(Context);
//                 diffState.SendSyncConfigToRemote();
//                 Context.StateHelper = diffState;
//             }
//             else
//             {
//                 throw new Exception("远程服务器权限校验失败，请检查Local Server 的Mac地址是否在 Remote Server 的允许列表内！");
//             }
//         }
//     }

//     public override void HandleLocalMsg(SyncMsg? msg) { }

//     public void SendPwdToRemoteServer()
//     {
//         var authorityInfo = new
//         {
//             Pwd = "xfs@#123hd??1>>|12#4",
//             MacAdr = new LocalServer.Controllers.LocalServerController(
//                 Context.Factory
//             ).GetMacAddress()
//         };
//         Context.RemoteSocketSendMsg(authorityInfo);
//     }
// }

// /// <summary>
// /// 3. 文件比较
// /// </summary>
// /// <param name="context"></param>
// public class DirFilesDiffState(LocalSyncServer context) : StateHelpBase(context)
// {
//     public override void HandleRemoteMsg(SyncMsg? msg)
//     {
//         if (msg == null)
//         {
//             return;
//         }
//         else
//         {
//             if (msg.IsSuccess)
//             {
//                 var state = new LocalPackAndUploadState(Context);
//                 state.PackDiffDir(msg);
//                 Context.StateHelper = state;
//             }
//             else
//             {
//                 throw new Exception(msg.Body);
//             }
//         }
//     }

//     public override void HandleLocalMsg(SyncMsg? msg) { }

//     public void SendSyncConfigToRemote()
//     {
//         Context.RemoteSocketSendMsg(
//             Context.SyncConfig
//                 ?? throw new NullReferenceException("SyncConfig should't be null here!")
//         );
//     }
// }

// /// <summary>
// /// 4. 本地打包并上传
// /// </summary>
// /// <param name="context"></param>
// public class LocalPackAndUploadState(LocalSyncServer context) : StateHelpBase(context)
// {
//     public override void HandleRemoteMsg(SyncMsg? msg)
//     {
//         if (msg == null)
//         {
//             return;
//         }
//         else { }
//     }

//     public override void HandleLocalMsg(SyncMsg? msg) { }

//     /// <summary>
//     /// 打包文件
//     /// </summary>
//     /// <param name="msg"></param>
//     /// <exception cref="Exception"></exception>
//     public void PackDiffDir(SyncMsg msg)
//     {
//         if (msg.IsSuccess)
//         {
//             var diff = JsonSerializer.Deserialize<Dir>(msg.Body);

//             Context.LocalSocketSendMsg(new SyncMsg(SyncMsgType.Success, "文件打包完成！"));
//             Context.LocalSocketSendMsg(new SyncMsg(SyncMsgType.Success, "文件上传完成！"));
//         }
//         else
//         {
//             throw new Exception(msg.Body);
//         }
//     }

//     private void UploadPackedFiles(string absolutePath)
//     {
//         //TODO 传递上传进度到前端。
//     }
// }

// /// <summary>
// /// 5. 目标服务器解包并发布
// /// </summary>
// /// <param name="context"></param>
// public class RemoteUnPackAndReleaseState(LocalSyncServer context) : StateHelpBase(context)
// {
//     public override void HandleRemoteMsg(SyncMsg? msg)
//     {
//         if (msg == null)
//         {
//             return;
//         }
//         else { }
//     }

//     public override void HandleLocalMsg(SyncMsg? msg) { }
// }
