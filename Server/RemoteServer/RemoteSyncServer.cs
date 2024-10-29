using System.Diagnostics;
using System.Net.WebSockets;
using Common;

namespace RemoteServer;

public class RemoteSyncServer
{
#pragma warning disable CA2211 // Non-constant fields should not be visible
    public static string TempRootFile = "C:/TempPack";
    public static string SqlPackageAbPath = "SqlPackageAbPath";
#pragma warning restore CA2211 // Non-constant fields should not be visible
    private StateHelpBase StateHelper;

    public void SetStateHelpBase(StateHelpBase stateHelper)
    {
        try
        {
            StateHelper = stateHelper;
            if (SyncConfig != null)
            {
                var LastExec = NotNullSyncConfig.ExecProcesses.Find(x =>
                {
                    return x.StepBeforeOrAfter == "A"
                        && x.Step == (stateHelper.Step - 1)
                        && x.ExecInLocalOrServer == "S";
                });
                ExecProcess(LastExec);
                var CurrentExec = NotNullSyncConfig.ExecProcesses.Find(x =>
                {
                    return x.StepBeforeOrAfter == "B"
                        && x.Step == stateHelper.Step
                        && x.ExecInLocalOrServer == "S";
                });
                ExecProcess(CurrentExec);
            }
        }
        catch (Exception ex)
        {
            Close(ex.Message);
        }
    }

    public void ExecProcess(ExecProcess? ep)
    {
        if (ep != null)
        {
            ProcessStartInfo startInfo =
                new()
                {
                    StandardOutputEncoding = System.Text.Encoding.UTF8,
                    Arguments = ep.Argumnets,
                    FileName = ep.FileName, // The command to execute (can be any command line tool)
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
                Pipe.SendMsg(
                        StateHelper.CreateMsg(
                            $"{ep.Step}-{ep.StepBeforeOrAfter}-{ep.ExecInLocalOrServer}-{ep.FileName}  {ep.Argumnets} 执行成功！"
                        )
                    )
                    .Wait();
            }
            else
            {
                Pipe.SendMsg(
                        StateHelper.CreateMsg(
                            $"{ep.Step}-{ep.StepBeforeOrAfter}-{ep.ExecInLocalOrServer}-{ep.FileName}  {ep.Argumnets} 失败 {output}！"
                        )
                    )
                    .Wait();
                throw new Exception("错误,信息参考上一条消息！");
            }
        }
    }

    public StateHelpBase GetStateHelpBase()
    {
        return StateHelper;
    }

    public Config? SyncConfig;

    public Config NotNullSyncConfig
    {
        get
        {
            if (SyncConfig == null)
            {
                throw new ArgumentNullException("SyncConfig");
            }
            return SyncConfig;
        }
    }

    /// <summary>
    /// 发布源连接
    /// </summary>
    public readonly AbsPipeLine Pipe;

    /// <summary>
    /// 父工程，用于释放资源
    /// </summary>
    public readonly RemoteSyncServerFactory Factory;

    public string Name;

    public string Pwd;

    public RemoteSyncServer(
        AbsPipeLine pipe,
        RemoteSyncServerFactory factory,
        string name,
        string pwd
    )
    {
        Pipe = pipe;
        Factory = factory;
        Name = name;
        Pwd = pwd;
        StateHelper = new ConnectAuthorityHelper(this);
    }

    public async Task Connect()
    {
        try
        {
            var rs = Pipe.Work(
                (byte[] b) =>
                {
                    return StateHelper.ReceiveMsg(b);
                }
            );
            await foreach (var r in rs) { }
        }
        catch (Exception e)
        {
            Close(e.Message);
        }
    }

    public void Close(string? CloseReason)
    {
        try
        {
            Pipe.Close(CloseReason);
        }
        catch (Exception e)
        {
            //TODO 日志
            Console.WriteLine(e.Message);
        }
        finally
        {
            Factory.RemoveSyncServer(this);
        }
    }
}
