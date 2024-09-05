using System.Net.WebSockets;

namespace LocalServer;

public class LocalSyncServerFactory
{
    private readonly object Lock = new();

    public  void CreateLocalSyncServer(WebSocket socket, string name)
    {
        if (Servers.Select(x => x.Name == name).Any())
        {
            throw new Exception("LocalServer:存在同名发布源！");
        }
        var server = new LocalSyncServer(socket, name, this);
        lock (Lock)
        {
            Servers.Add(server);
        }
        //脱离当前函数栈
        Task.Run(async ()=>{
            await server.LocalSocketListen();
        });
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
