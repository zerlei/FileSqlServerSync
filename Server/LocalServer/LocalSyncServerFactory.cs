using System.Net.WebSockets;

namespace LocalServer;

public class LocalSyncServerFactory
{
    private readonly object Lock = new();

    public async void CreateLocalSyncServer(WebSocket socket, string name)
    {
        if (Servers.Select(x => x.Name == name).Any())
        {
            throw new Exception("there already is a server with that name is Runing!");
        }
        var server = new LocalSyncServer(socket, name, this);
        lock (Lock)
        {
            Servers.Add(server);
        }
        await server.Start();
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
