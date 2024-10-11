using System.Net.NetworkInformation;
using System.Net.WebSockets;
using System.Text;
using Common;
using Microsoft.AspNetCore.Mvc;

namespace LocalServer.Controllers
{
    public class LocalServerController(LocalSyncServerFactory factory) : ControllerBase
    {
        private readonly LocalSyncServerFactory Factory = factory;

        private static async Task Echo(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                CancellationToken.None
            );

            while (!receiveResult.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(
                    new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                    receiveResult.MessageType,
                    receiveResult.EndOfMessage,
                    CancellationToken.None
                );

                receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    CancellationToken.None
                );
            }

            await webSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None
            );
        }

        [Route("/websoc")]
        public async Task WebsocketConnection(string Name)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                try
                {
                    if (Factory.GetServerByName(Name) == null)
                    {
                        var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                        //await Echo(webSocket);
                        var pipeLine = new WebSocPipeLine<WebSocket>(webSocket, false);
                        await Factory.CreateLocalSyncServer(
                            pipeLine,
                            Name,
                            new WebSocPipeLine<ClientWebSocket>(new ClientWebSocket(), true)
                        );
                    }
                    else
                    {
                        throw new Exception("LocalServer: 已经存在同名的发布正在进行!");
                    }
                }
                catch (Exception e)
                {
                    HttpContext.Response.Body = new MemoryStream(Encoding.UTF8.GetBytes(e.Message));
                    HttpContext.Response.StatusCode = StatusCodes.Status406NotAcceptable;
                }
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        [Route("/macaddr")]
        public string GetMacAddress()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            string macaddrs = "";
            foreach (NetworkInterface nic in nics)
            {
                PhysicalAddress physicalAddress = nic.GetPhysicalAddress();
                macaddrs += physicalAddress.ToString() + ";";
            }
            return macaddrs;
        }
        //TODO 是否在本地记载同步日志？
    }
}
