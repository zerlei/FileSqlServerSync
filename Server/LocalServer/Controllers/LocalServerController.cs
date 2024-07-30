using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace LocalServer.Controllers
{
    public class LocalServerController(LocalSyncServerFactory factory) : ControllerBase
    {
        private readonly LocalSyncServerFactory Factory = factory;

        [Route("/")]
        public async Task WebsocketConnection(string Name)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                try
                {
                    var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                    Factory.CreateLocalSyncServer(webSocket, Name);
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
        //TODO 是否在本地记载同步日志？
    }
}
