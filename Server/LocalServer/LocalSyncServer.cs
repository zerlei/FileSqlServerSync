using Common;

namespace LocalServer;

public class LocalSyncServer
{
#pragma warning disable CA2211 // Non-constant fields should not be visible
    public static string TempRootFile = "C:/TempPack";
#pragma warning restore CA2211 // Non-constant fields should not be visible
    public StateHelpBase StateHelper;

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

    public LocalSyncServer(AbsPipeLine pipe, LocalSyncServerFactory factory,string name,AbsPipeLine remotePipe )
    {
        LocalPipe = pipe;
        Factory = factory;
        StateHelper = new ConnectAuthorityHelper(this);
        Name = name;
        RemotePipe = remotePipe;

        Task.Run(async () =>
        {
            var rs = LocalPipe.Work(
                (byte[] b) =>
                {
                    return StateHelper.ReceiveLocalMsg(b);
                }
            );
            try
            {
                await foreach (var r in rs) { }
            }
            catch (Exception e)
            {
                Close(e.Message);
            }
        });
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
