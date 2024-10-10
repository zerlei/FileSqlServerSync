using System.Net.WebSockets;
using Common;

namespace LocalServer;

public class LocalSyncServerFactory
{
    private readonly object Lock = new();

    public async Task CreateLocalSyncServer(
        AbsPipeLine pipeLine,
        string name,
        AbsPipeLine absPipeLine
    )
    {
        var server = new LocalSyncServer(pipeLine, this, name, absPipeLine);
        lock (Lock)
        {
            Servers.Add(server);
        }
        await server.Connect();
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
        var it = Servers.Where(x => x.Name == name).FirstOrDefault();
        return it;
    }
}
