using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TheGame.WebSocketService
{
    /// <summary>
    /// Manage websocket connections
    ///  - keeping records on all active users
    ///  - allows to subscribe to onOpen/onClose/onMessage events
    /// </summary>
    public class WebSocketConnectionManager
    {
        private ConcurrentDictionary<Guid, WebSocket> _clients { get; set; } = new ConcurrentDictionary<Guid, WebSocket>();
        private readonly ILogger<WebSocketConnectionManager> _logger;

        public int OnlineClients => _clients.Count;
        public bool IsExists(Guid id) => _clients.ContainsKey(id);

        public WebSocketConnectionManager(ILogger<WebSocketConnectionManager> logger)
        {
            _logger = logger;
        }

        public async Task SubscribeAsync(Action<WebSocketConnectionManager> connection)
        {
            connection(this);
        }

        // Events
        public Action<WebSocket, Guid> OnOpen { get; set; } = (webSocket, clientId) => { };
        public Action<WebSocket, Guid> OnClose { get; set; } = (webSocket, clientId) => { };
        public Action<WebSocket, Guid, string> OnMessage { get; set; } = (webSocket, clientId, message) => { };
        public Action<WebSocket, Guid, byte[]> OnBinary { get; set; } = (webSocket, clientId, bytes) => { };

        /// <summary>
        /// Send a message to a connceted client
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendAsync(WebSocket socket, string message)
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.SendAsync(
                  new ArraySegment<byte>(Encoding.ASCII.GetBytes(message), 0, message.Length),
                  WebSocketMessageType.Text,
                  true,
                  CancellationToken.None);
            }
        }

        public async Task SendAsync(Guid clientId, string message)
        {
            WebSocket clientSocket;
            if (_clients.TryGetValue(clientId, out clientSocket))
            {
                await SendAsync(clientSocket, message);
            }
        }

        /// <summary>
        /// Force close connection with a connected client
        /// </summary>
        /// <param name="socket"></param>
        public void AbortConnection(WebSocket socket, string message)
        {
            if (socket.State == WebSocketState.Open)
            {
                socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, message, CancellationToken.None);
            } 
        }

        /// <summary>
        /// Force close connection with a connected client
        /// </summary>
        /// <param name="clientId"></param>
        public void AbortConnection(Guid clientId, string message)
        {
            WebSocket clientSocket;
            if (_clients.TryGetValue(clientId, out clientSocket))
            {
                AbortConnection(clientSocket, message);
                _clients.TryRemove(clientId, out clientSocket);
            }

        }

        /// <summary>
        /// Upgrading a HTTP connection to a WebSocket connection and connect to it
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public async Task ConnectAsync(HttpContext httpContext, Guid clientId)
        {
            using WebSocket webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
            _clients.TryAdd(clientId, webSocket);
            OnOpen(webSocket, clientId);
            await ListenToClientEventAsync(webSocket, async (result, buffer) =>
            {
                await Task.Delay(1);
                switch (result.MessageType)
                {
                    case WebSocketMessageType.Text:
                        var s = Encoding.UTF8.GetString(buffer);
                        var msg = s.Substring(0, Math.Max(0, s.IndexOf('\0')));
                        OnMessage(webSocket, clientId, msg);
                        break;
                    case WebSocketMessageType.Binary:
                        OnBinary(webSocket, clientId, buffer);
                        break;
                    case WebSocketMessageType.Close:
                        WebSocket clientSocket;
                        _clients.TryRemove(clientId,out clientSocket);
                        OnClose(webSocket, clientId);
                        break;
                }
            });
        }

        /// <summary>
        /// Listen to client event - a message received or connection clsoed
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        private async Task ListenToClientEventAsync(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handler)
        {
            var buffer = new byte[1024];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer), cancellationToken: CancellationToken.None);
                handler(result, buffer);
            }
        }

    }
}
