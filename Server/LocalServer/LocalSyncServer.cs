using System.Diagnostics;
using System.IO.Pipelines;
using System.Xml.Linq;
using Common;

namespace LocalServer;

public class LocalSyncServer
{
#pragma warning disable CA2211 // Non-constant fields should not be visible
    public static string TempRootFile = "C:/TempPack";
    public static string SqlPackageAbPath = "sqlpackage";

    // 使用msdeploy 将会打包当前可运行的内容，它很有可能不包含最新的构建
    //public static string MsdeployAbPath = "msdeploy";

    //与visual studio 匹配的Msbuild 路径。在vs 中打开power shell 命令行，使用 `(get-Command -Name msbuild).Source `
    //使用msbuild 会缺少.net frame的运行环境 bin\roslyn 里面的内容，第一次需要人为复制一下，后面就就好了。
    public static string MSBuildAbPath = "MSBuild";
#pragma warning restore CA2211 // Non-constant fields should not be visible

    /// <summary>
    /// 连接状态，流程管理，LocalPipe 和 remotePipe 也在此处交换信息
    /// </summary>
    private StateHelpBase StateHelper;

    public void SetStateHelper(StateHelpBase helper)
    {
        try
        {
            StateHelper = helper;
            if (SyncConfig != null)
            {
                var LastExec = NotNullSyncConfig.ExecProcesses.Find(x =>
                {
                    return x.StepBeforeOrAfter == "A"
                        && x.Step == (helper.Step - 1)
                        && x.ExecInLocalOrServer == "L";
                });
                ExecProcess(LastExec);
                var CurrentExec = NotNullSyncConfig.ExecProcesses.Find(x =>
                {
                    return x.StepBeforeOrAfter == "B"
                        && x.Step == helper.Step
                        && x.ExecInLocalOrServer == "L";
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
                LocalPipe
                    .SendMsg(
                        StateHelper.CreateMsg(
                            $"{ep.Step}-{ep.StepBeforeOrAfter}-{ep.ExecInLocalOrServer}-{ep.FileName} {ep.Argumnets} 执行成功！"
                        )
                    )
                    .Wait();
            }
            else
            {
                LocalPipe
                    .SendMsg(
                        StateHelper.CreateMsg(
                            $"{ep.Step}-{ep.StepBeforeOrAfter}-{ep.ExecInLocalOrServer}-{ep.FileName}  {ep.Argumnets} 失败 {output}！"
                        )
                    )
                    .Wait();
                throw new Exception("错误,信息参考上一条消息！");
            }
        }
    }

    /// <summary>
    /// 查找构建xml文件，获取构建信息，那这个不用了
    /// </summary>
    /// <returns></returns>
    // public static string GetProjectOutPath(string project)
    // {
    //     try
    //     {
    //         XDocument xdoc = XDocument.Load(project);
    //         // 获取根元素
    //         XElement rootElement = xdoc.Root ?? throw new NullReferenceException("Root");
    //         Console.WriteLine("根元素: " + rootElement.Name);

    //         // 遍历子节点
    //         foreach (XElement element in rootElement.Elements())
    //         {
    //             if (element.Name.LocalName.Contains("PropertyGroup"))
    //             {
    //                 var Conditon = element.Attribute("Condition");

    //                 if (Conditon != null)
    //                 {
    //                     if (Conditon.Value.Contains("Release"))
    //                     {
    //                         foreach (XElement element2 in element.Elements())
    //                         {
    //                             if (element2.Name.LocalName == "OutputPath")
    //                             {
    //                                 return element2.Value;
    //                             }
    //                         }
    //                     }
    //                 }
    //             }
    //         }
    //         return "bin/";
    //     }
    //     catch (Exception)
    //     {
    //         return "bin/";
    //     }
    // }

    public StateHelpBase GetStateHelper()
    {
        return StateHelper;
    }

    /// <summary>
    /// 此次发布的配置
    /// </summary>
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
    /// 发布的名称
    /// </summary>
    public string Name;

    /// <summary>
    ///  jswebsocket 和 local server 的连接，它没有加密
    /// </summary>
    public readonly AbsPipeLine LocalPipe;

    /// <summary>
    ///  local server 和 remote server 的连接，它有加密
    /// </summary>
    public readonly AbsPipeLine RemotePipe;

    /// <summary>
    /// 父工程，用于释放资源
    /// </summary>
    public readonly LocalSyncServerFactory Factory;

    public LocalSyncServer(
        AbsPipeLine pipe,
        LocalSyncServerFactory factory,
        string name,
        AbsPipeLine remotePipe
    )
    {
        LocalPipe = pipe;
        Factory = factory;
        StateHelper = new ConnectAuthorityHelper(this);
        Name = name;
        RemotePipe = remotePipe;
    }

    /// <summary>
    /// 这个阻塞在，接口中有http上下文处
    /// </summary>
    /// <returns></returns>
    public async Task Connect()
    {
        try
        {
            var rs = LocalPipe.Work(
                (byte[] b) =>
                {
                    return StateHelper.ReceiveLocalMsg(b);
                }
            );
            await foreach (var r in rs) { }
        }
        catch (Exception e)
        {
            Close(e.Message);
        }
    }

    /// <summary>
    /// 关闭连接
    /// </summary>
    /// <param name="CloseReason"></param>
    public void Close(string? CloseReason)
    {
        try
        {
            LocalPipe.Close(CloseReason);
            RemotePipe.Close(CloseReason);
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
