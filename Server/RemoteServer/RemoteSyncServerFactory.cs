using Common;

namespace RemoteServer;

public class RemoteSyncServerFactory
{
    private readonly object Lock = new();

#pragma warning disable CA2211 // Non-constant fields should not be visible
    public static List<Tuple<string, string>> NamePwd = [];
#pragma warning restore CA2211 // Non-constant fields should not be visible

    public async Task CreateRemoteSyncServer(AbsPipeLine pipeLine, string Name)
    {
        var pwd =
            NamePwd.Where(x => x.Item1 == Name).FirstOrDefault()
            ?? throw new Exception("RemoteServer: 不被允许的发布名称！");
        var server = new RemoteSyncServer(pipeLine, this, Name, pwd.Item2);
        lock (Lock)
        {
            Servers.Add(server);
        }
        await server.Connect();
    }

    private readonly List<RemoteSyncServer> Servers = [];

    public void RemoveSyncServer(RemoteSyncServer server)
    {
        lock (Lock)
        {
            Servers.Remove(server);
        }
    }

    public RemoteSyncServer? GetServerByName(string name)
    {
        var it = Servers.Where(x => x.Name == name).FirstOrDefault();
        return it;
    }

    public RemoteSyncServer? GetServerById(string Id)
    {
        return Servers.Where(x => x.NotNullSyncConfig.Id.ToString() == Id).FirstOrDefault();
    }
}
