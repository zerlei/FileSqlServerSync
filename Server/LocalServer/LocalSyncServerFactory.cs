using Common;
namespace LocalServer;

public class LocalSyncServerFactory
{
    private readonly object Lock = new();

    public  void CreateLocalSyncServer(AbsPipeLine pipeLine,string name)
    {
        var server = new LocalSyncServer(pipeLine, this,name);
        lock (Lock)
        {
            Servers.Add(server);
        }
    }

    private readonly List<LocalSyncServer> Servers = [];

    public void RemoveLocalSyncServer(LocalSyncServer server)
    {
        lock (Lock)
        {
            Servers.Remove(server);
        }
    }

    public LocalSyncServer? GetServerByName(string name)
    {
        var it = Servers.Where(x=>x.Name== name).FirstOrDefault();
        return it;
    }
}
