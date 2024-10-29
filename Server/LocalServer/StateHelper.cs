using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Common;

namespace LocalServer;

public abstract class StateHelpBase(
    LocalSyncServer context,
    SyncProcessStep step = SyncProcessStep.Connect
)
{
    protected readonly LocalSyncServer Context = context;

    public readonly SyncProcessStep Step = step;

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
        if (syncMsg.Step != Step && syncMsg.Step != SyncProcessStep.Close)
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
        if (syncMsg.Step != Step && syncMsg.Step != SyncProcessStep.Close)
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
        Context.SetStateHelper(deployHelper);
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
                        var x = Context.GetStateHelper().Step;
                        return Context.GetStateHelper().ReceiveRemoteMsg(b);
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
                Context.Close(e.Message);
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
            Context.SetStateHelper(h);
            h.DiffProcess();
        }
        else
        { // msbuild 只在windows 才有
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                //构建

                ProcessStartInfo startbuildInfo =
                    new()
                    {
                        FileName = LocalSyncServer.MSBuildAbPath, // The command to execute (can be any command line tool)
                        Arguments =
                            $" {Context.NotNullSyncConfig.LocalProjectAbsolutePath} /t:ResolveReferences"
                            + $" /t:Compile /p:Configuration=Release /t:_CopyWebApplication  /p:OutputPath={Context.NotNullSyncConfig.LocalRootPath}/bin"
                            + $" /p:WebProjectOutputDir={Context.NotNullSyncConfig.LocalRootPath}",
                        // The arguments to pass to the command (e.g., list directory contents)
                        RedirectStandardOutput = true, // Redirect the standard output to a string
                        RedirectStandardError = true,
                        StandardOutputEncoding = System.Text.Encoding.UTF8,
                        UseShellExecute = false, // Do not use the shell to execute the command
                        CreateNoWindow = true // Do not create a new window for the command
                    };
                using Process bprocess = new() { StartInfo = startbuildInfo };
                // Start the process
                bprocess.Start();

                // Read the output from the process
                string boutput = bprocess.StandardOutput.ReadToEnd();

                // Wait for the process to exit
                bprocess.WaitForExit();

                if (bprocess.ExitCode == 0)
                {
                    Context.LocalPipe.SendMsg(CreateMsg("本地编译成功！")).Wait();
                    var h = new DiffFileAndPackHelper(Context);
                    Context.SetStateHelper(h);
                    h.DiffProcess();
                }
                else
                {
                    var aTexts = boutput.Split('\n');
                    if (aTexts.Length > 10)
                    {
                        boutput = string.Join('\n', aTexts.Skip(aTexts.Length - 10));
                    }
                    Context.LocalPipe.SendMsg(CreateErrMsg(boutput)).Wait();
                    throw new Exception("执行编译错误，错误信息参考上一条消息！");
                }
                //发布
                //ProcessStartInfo startInfo =
                //    new()
                //    {
                //        FileName = LocalSyncServer.MsdeployAbPath, // The command to execute (can be any command line tool)
                //        Arguments =
                //            $" -verb:sync -source:contentPath={Context.NotNullSyncConfig.LocalProjectAbsolutePath} -dest:contentPath={Context.NotNullSyncConfig.LocalRootPath} -disablerule:BackupRule",
                //        // The arguments to pass to the command (e.g., list directory contents)
                //        RedirectStandardOutput = true, // Redirect the standard output to a string
                //        UseShellExecute = false, // Do not use the shell to execute the command
                //        CreateNoWindow = true // Do not create a new window for the command
                //    };
                //using Process process = new() { StartInfo = startInfo };
                //// Start the process
                //process.Start();

                //// Read the output from the process
                //string output = process.StandardOutput.ReadToEnd();

                //// Wait for the process to exit
                //process.WaitForExit();

                //if (process.ExitCode == 0)
                //{
                //    Context.LocalPipe.SendMsg(CreateMsg("本地发布成功！")).Wait();
                //    var h = new DiffFileAndPackHelper(Context);
                //    Context.SetStateHelper(h);
                //    h.DiffProcess();
                //}
                //else
                //{
                //    Context.LocalPipe.SendMsg(CreateErrMsg(output)).Wait();
                //    throw new Exception("执行发布错误，错误信息参考上一条消息！");
                //}
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
            e.LocalDirInfo = new Dir
            {
                Path = Context.NotNullSyncConfig.LocalRootPath + e.DirPath,
                Children = []
            };
            e.LocalDirInfo.ExtractInfo(e.CherryPicks, e.Excludes);
        });

        //将配置信息发送到remoteServer
        var options = new JsonSerializerOptions { WriteIndented = true };
        Context
            .RemotePipe.SendMsg(
                CreateMsg(JsonSerializer.Serialize(Context.NotNullSyncConfig, options))
            )
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
        Directory.CreateDirectory(
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
        Context.LocalPipe.SendMsg(CreateMsg("文件差异比较成功！")).Wait();
        var n = new DeployMSSqlHelper(Context);
        Context.SetStateHelper(n);
        n.PackSqlServerProcess();
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
        Context.SetStateHelper(h);
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
            var arguments =
                $" /Action:Extract /TargetFile:{LocalSyncServer.TempRootFile}/{Context.NotNullSyncConfig.Id.ToString()}/{Context.NotNullSyncConfig.Id.ToString()}.dacpac"
                // 不要log file 了
                //+ $" /DiagnosticsFile:{LocalSyncServer.TempRootFile}/{Context.NotNullSyncConfig.Id.ToString()}/{Context.NotNullSyncConfig.Id.ToString()}.log"
                + $" /p:ExtractAllTableData=false /p:VerifyExtraction=true /SourceServerName:{Context.NotNullSyncConfig.SrcDb.ServerName}"
                + $" /SourceDatabaseName:{Context.NotNullSyncConfig.SrcDb.DatabaseName} /SourceUser:{Context.NotNullSyncConfig.SrcDb.User}"
                + $" /SourcePassword:{Context.NotNullSyncConfig.SrcDb.Password} /SourceTrustServerCertificate:{Context.NotNullSyncConfig.SrcDb.TrustServerCertificate}"
                + $" /p:ExtractReferencedServerScopedElements=False /p:IgnoreUserLoginMappings=True /p:IgnorePermissions=True";
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
                    FileName = LocalSyncServer.SqlPackageAbPath, // The command to execute (can be any command line tool)
                    Arguments = arguments,
                    StandardOutputEncoding = System.Text.Encoding.UTF8,
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
                $"{LocalSyncServer.TempRootFile}/{Context.NotNullSyncConfig.Id}.zip",
                (double current) =>
                {
                    //这里可能需要降低获取上传进度的频率
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
        Context.SetStateHelper(h);
    }
}

public class FinallyPublishHelper(LocalSyncServer context)
    : StateHelpBase(context, SyncProcessStep.Publish)
{
    protected override void HandleLocalMsg(SyncMsg msg)
    {
        //throw new NotImplementedException();
    }

    /// <summary>
    /// 由最初始的客户端断开连接，表示发布完成。
    /// </summary>
    /// <param name="msg"></param>
    protected override void HandleRemoteMsg(SyncMsg msg)
    {
        if (msg.Body == "发布完成！")
        {
            Context.SetStateHelper(new NormalCloseHelper(Context));
        }
        Context.LocalPipe.SendMsg(msg).Wait();
    }
}

public class NormalCloseHelper(LocalSyncServer context)
    : StateHelpBase(context, SyncProcessStep.Close)
{
    protected override void HandleRemoteMsg(SyncMsg msg) { }

    protected override void HandleLocalMsg(SyncMsg msg) { }
}
