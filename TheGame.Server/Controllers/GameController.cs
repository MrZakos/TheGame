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
using TheGame.BLL;
using TheGame.Common.Models;
using TheGame.DataService;
using TheGame.WebSocketService;

namespace TheGame.Server.Controllers
{
    /// <summary>
    /// Game controller API - handle websocket initiation and other operations on users
    /// </summary>
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly BusinessLogicLayer _bll;
        private readonly ILogger<GameController> _logger;

        public GameController(ILogger<GameController> logger, BusinessLogicLayer businessLogicLayer)
        {
            _logger = logger;
            _bll = businessLogicLayer;
            _bll.SubscribeToWebSocketsEventsIfNotRegistredAsync().GetAwaiter().GetResult();
            
         }

        [Route("/Login")]
        public async Task<dynamic> Login([FromQuery] GameLoginDTO model)
        {
            var result = await _bll.RunOperationAsync(OperationType.ClientLogin, HttpContext, model);
            return result.IsSuccess ? result.Result : result.Exception.Message;
        }

        [Route("/UpdateResources/{deviceId}")]
        [HttpPost]
        public async Task UpdateResourcesAsync(Guid deviceId)
        {
        }


        [Route("/UpdateResources/{deviceId}")]
        [HttpPost]
        public async Task SendGiftAsync(Guid deviceId)
        {
        }

        //private async Task Echo(HttpContext httpContext, WebSocket webSocket)
        //{
        //    var buffer = new byte[1024 * 4];
        //    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        //    _logger.Log(LogLevel.Information, "Message received from Client");

        //    while (!result.CloseStatus.HasValue)
        //    {
        //        var serverMsg = Encoding.UTF8.GetBytes($"Server: Hello. You said: {Encoding.UTF8.GetString(buffer)}");
        //        await webSocket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
        //        _logger.Log(LogLevel.Information, "Message sent to Client");

        //        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        //        _logger.Log(LogLevel.Information, "Message received from Client");

        //    }
        //    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        //    _logger.Log(LogLevel.Information, "WebSocket connection closed");
        //}
    }
}

