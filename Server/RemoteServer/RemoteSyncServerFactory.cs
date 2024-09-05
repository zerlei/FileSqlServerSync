using System.Net.WebSockets;

namespace RemoteServer;

public class RemoteSyncServerFactory
{
    private readonly object Lock = new();

    public  void CreateLocalSyncServer(WebSocket socket, string name)
    {
        if (Servers.Select(x => x.Name == name).Any())
        {
            throw new Exception("RemoteServer:存在同名发布源！");
        }
        var server = new RemoteSyncServer(socket, name, this);
        lock (Lock)
        {
            Servers.Add(server);
        }
        //脱离当前函数栈
        Task.Run(async ()=>{
            await server.RemoteSocketListen();
        });
    }

    private readonly List<RemoteSyncServer> Servers = [];

    public void RemoveLocalSyncServer(RemoteSyncServer server)
    {
        lock (Lock)
        {
            Servers.Remove(server);
        }
    }
}
