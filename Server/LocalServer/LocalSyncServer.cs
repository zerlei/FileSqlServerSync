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
        StateHelper = helper;
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
