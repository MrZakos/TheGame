using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TheGame.WebSocketService
{
    public class WebSocketConnectionManager
    {
        private Dictionary<string, WebSocket> _clients { get; set; } = new Dictionary<string, WebSocket>();

        private readonly ILogger<WebSocketConnectionManager> _logger;

        public WebSocketConnectionManager(ILogger<WebSocketConnectionManager> logger)
        {
            _logger = logger;
        }

        public Action<WebSocket> OnOpen { get; set; } = webSocket => { };
        public Action<WebSocket> OnClose { get; set; } = webSocket => { };
        public Action<WebSocket, string> OnMessage { get; set; } = (webSocket, message) => { };
        public Action<WebSocket, byte[]> OnBinary { get; set; } = (webSocket, bytes) => { };

        public async Task SendAsync(WebSocket socket, string message)
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.SendAsync(
                  new ArraySegment<byte>(Encoding.ASCII.GetBytes(message),
                    0,
                    message.Length),
                  WebSocketMessageType.Text,
                  true,
                  CancellationToken.None);
            }
        }

        public void AbortConnection(WebSocket socket)
        {
            if (socket.State == WebSocketState.Open)
            {
                socket.Abort();
            }
        }

        public async Task Connect(HttpContext httpContext)
        {
            using WebSocket webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
            await Receive(webSocket, async (result, buffer) =>
            {
                switch (result.MessageType)
                {
                    case WebSocketMessageType.Text:
                        var s = Encoding.UTF8.GetString(buffer);
                        var msg = s.Substring(0, Math.Max(0, s.IndexOf('\0')));
                        await SendAsync(webSocket, $"You sent >> {msg}");
                        OnMessage(webSocket, msg);
                        break;
                    case WebSocketMessageType.Binary:
                        OnBinary(webSocket, buffer);
                        break;
                    case WebSocketMessageType.Close:
                        OnClose(webSocket);
                        break;
                }
            });
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handler)
        {
            var buffer = new byte[1024];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer), cancellationToken: CancellationToken.None);
                handler(result, buffer);
            }
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
        //    OnClose(webSocket);
        //    _logger.Log(LogLevel.Information, "WebSocket connection closed");
        //}
    }
}
