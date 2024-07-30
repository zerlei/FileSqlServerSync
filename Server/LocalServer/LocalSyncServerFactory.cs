using System.Net.WebSockets;

namespace LocalServer;

public class LocalSyncServerFactory
{
    public void CreateLocalSyncServer(WebSocket socket, string name)
    {
        if (Servers.Select(x => x.Name == name).Any())
        {
            throw new Exception("there already is a server with that name is Runing!");
        }
        Servers.Add(new LocalSyncServer(socket, name, this));
    }

    private readonly List<LocalSyncServer> Servers = [];

    public void RemoveLocalSyncServer(LocalSyncServer server)
    {
        Servers.Remove(server);
    }
}
