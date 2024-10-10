using System.Xml.Linq;
using Common;

namespace LocalServer;

public class LocalSyncServer
{
#pragma warning disable CA2211 // Non-constant fields should not be visible
    public static string TempRootFile = "C:/TempPack";
    public static string SqlPackageAbPath = "sqlpackage";

    //public static string MsdeployAbPath = "msdeploy";

    //与visual studio 匹配的Msbuild 路径。在vs 中打开power shell 命令行，使用 `(get-Command -Name msbuild).Source `
    public static string MSBuildAbPath = "MSBuild";
#pragma warning restore CA2211 // Non-constant fields should not be visible
    private StateHelpBase StateHelper;

    public void SetStateHelper(StateHelpBase helper)
    {
        StateHelper = helper;
    }

    public static string GetProjectOutPath(string project)
    {
        try
        {
            XDocument xdoc = XDocument.Load(project);
            // 获取根元素
            XElement rootElement = xdoc.Root ?? throw new NullReferenceException("Root");
            Console.WriteLine("根元素: " + rootElement.Name);

            // 遍历子节点
            foreach (XElement element in rootElement.Elements())
            {
                if (element.Name.LocalName.Contains("PropertyGroup"))
                {
                    var Conditon = element.Attribute("Condition");

                    if (Conditon != null)
                    {
                        if (Conditon.Value.Contains("Release"))
                        {
                            foreach (XElement element2 in element.Elements())
                            {
                                if (element2.Name.LocalName == "OutputPath")
                                {
                                    return element2.Value;
                                }
                            }
                        }
                    }
                }
            }
            return "bin/";
        }
        catch (Exception)
        {
            return "bin/";
        }
    }

    public StateHelpBase GetStateHelper()
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

    public string Name;

    /// <summary>
    /// 发布源连接
    /// </summary>
    public readonly AbsPipeLine LocalPipe;

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
