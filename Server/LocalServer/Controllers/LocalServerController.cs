using System.Net.NetworkInformation;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Common;
using System.Net.WebSockets;
namespace LocalServer.Controllers
{
    public class LocalServerController(LocalSyncServerFactory factory) : ControllerBase
    {
        private readonly LocalSyncServerFactory Factory = factory;

        [Route("/websoc")]
        public async Task WebsocketConnection(string Name)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                try
                {
                    var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                    var pipeLine = new WebSocPipeLine<WebSocket>(webSocket);
                    Factory.CreateLocalSyncServer(pipeLine, Name);
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
