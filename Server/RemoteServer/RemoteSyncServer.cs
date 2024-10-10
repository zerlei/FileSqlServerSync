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
        StateHelper = stateHelper;
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
