using System.Net.WebSockets;

namespace LocalServer;

public class LocalSyncServer
{
    public readonly WebSocket Socket;
    public readonly string Name;

    public readonly LocalSyncServerFactory Factory;

    public LocalSyncServer(WebSocket socket, string name, LocalSyncServerFactory factory)
    {
        Socket = socket;
        Name = name;
        Factory = factory;
        
    }
}
