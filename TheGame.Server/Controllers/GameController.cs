using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TheGame.Common.Models;
using TheGame.WebSocketService;

namespace TheGame.Server.Controllers
{
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly WebSocketConnectionManager _webSocketConnectionManager;
        private readonly ILogger<GameController> _logger;

        public GameController(ILogger<GameController> logger, WebSocketConnectionManager webSocketConnectionManager)
        {
            _logger = logger;
            _webSocketConnectionManager = webSocketConnectionManager;
        }


        [Route("/Login/{deviceId}")]
        public async Task Login([FromQuery] GameLoginDTO model)
        {
 
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                await _webSocketConnectionManager.Connect(HttpContext);
            }
            else
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }

        [Route("/UpdateResources/{deviceId}")]
        [HttpPost]
        public async Task UpdateResourcesAsync(Guid deviceId)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await Echo(HttpContext, webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }


        [Route("/UpdateResources/{deviceId}")]
        [HttpPost]
        public async Task SendGiftAsync(Guid deviceId)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await Echo(HttpContext, webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }

        private async Task Echo(HttpContext httpContext, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            _logger.Log(LogLevel.Information, "Message received from Client");

            while (!result.CloseStatus.HasValue)
            {
                var serverMsg = Encoding.UTF8.GetBytes($"Server: Hello. You said: {Encoding.UTF8.GetString(buffer)}");
                await webSocket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
                _logger.Log(LogLevel.Information, "Message sent to Client");

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                _logger.Log(LogLevel.Information, "Message received from Client");

            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            _logger.Log(LogLevel.Information, "WebSocket connection closed");
        }
    }
}

