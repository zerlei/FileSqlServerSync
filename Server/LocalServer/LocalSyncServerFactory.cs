using System.Net.WebSockets;
using Common;
namespace LocalServer;

public class LocalSyncServerFactory
{
    private readonly object Lock = new();

    public  void CreateLocalSyncServer(AbsPipeLine pipeLine, string name)
    {
        if (Servers.Select(x => x.Name == name).Any())
        {
            throw new Exception("LocalServer:存在同名发布源！");
        }
        var server = new LocalSyncServer(pipeLine, name, this);
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
}
