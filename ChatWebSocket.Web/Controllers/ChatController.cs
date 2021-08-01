using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace ChatWebSocket.Web.Controllers
{
    [Route("api/[controller]")]
    public class ChatController : Controller
    {
        public IChatSocketConnection _socketConnection { get; }

        public ChatController(IChatSocketConnection socketConnection)
        {
            _socketConnection = socketConnection;
        }

        [HttpGet]
        public async Task Get(string nickName)
        {
            var context = ControllerContext.HttpContext;
            var isSocketRequest = context.WebSockets.IsWebSocketRequest;

            if (isSocketRequest)
            {
                WebSocket websocket = await context.WebSockets.AcceptWebSocketAsync();
                await _socketConnection.EntrarNaSala(nickName, websocket);
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        }
    }
}
