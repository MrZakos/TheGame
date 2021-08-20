using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TheGame.Common.Models;

namespace TheGame.WebSocketService
{
    /// <summary>
    /// Manage websocket connections
    ///  - keeping records on all active users
    ///  - allows events subscription (onOpen/onClose/onMessage)
    /// </summary>
    public class WebSocketConnectionManager
    {
        private ConcurrentDictionary<Guid, SocketConnectionSession> _clients { get; set; } = new ConcurrentDictionary<Guid, SocketConnectionSession>();
        private readonly ILogger<WebSocketConnectionManager> _logger;
        private readonly int _clientMessageBufferSize = 1024;

        public int OnlineClients => _clients.Count;
        public bool IsExists(Guid id) => _clients.ContainsKey(id);
        public bool IsDeviceExists(Guid deviceId) => _clients.ToList().Exists(x => x.Value.IsLoggedIn && x.Value.Player.DeviceId == deviceId);
        public SocketConnectionSession GetByDevice(Guid deviceId) => _clients.ToList().FirstOrDefault(x => x.Value.IsLoggedIn && x.Value.Player.DeviceId == deviceId).Value;

        public WebSocketConnectionManager(ILogger<WebSocketConnectionManager> logger)
        {
            _logger = logger;
        }

        public void Subscribe(Action<WebSocketConnectionManager> connection)
        {
            connection(this);
        }

        // Events
        public Action<SocketConnectionSession> OnOpen { get; set; } = (session) => { };
        public Action<SocketConnectionSession> OnClose { get; set; } = (session) => { };
        public Action<SocketConnectionSession, string> OnMessage { get; set; } = (session, message) => { };
        public Action<SocketConnectionSession, byte[]> OnBinary { get; set; } = (session, bytes) => { };

        /// <summary>
        /// Send a message to a connceted client
        /// </summary>
        /// <param name="session"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessageAsync(SocketConnectionSession session, string message)
        {
            if (session.Socket.State == WebSocketState.Open)
            {
                try
                {
                    await session.Socket.SendAsync(
                          new ArraySegment<byte>(Encoding.ASCII.GetBytes(message), 0, message.Length),
                          WebSocketMessageType.Text,
                          true,
                          CancellationToken.None);
                    _logger.LogInformation($"{session} message sent {message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"${session} message sending {message} failed");
                }
            }
            else
            {
                _logger.LogInformation($"{session} could not sent a message because socket state is {session.Socket.State}");
            }
        }

        /// <summary>
        /// Send a message to a client
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessageAsync(Guid sessionId, string message)
        {
            SocketConnectionSession session;
            if (_clients.TryGetValue(sessionId, out session))
            {
                await SendMessageAsync(session, message);
            }
            else
            {
                _logger.LogInformation($"${nameof(SendMessageAsync)} - could not find sessionId={sessionId}");
            }
        }

        /// <summary>
        /// /// Close client connection
        /// </summary>
        /// <param name="session"></param>
        /// <param name="clientId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task AbortConnectionAsync(SocketConnectionSession session, string message)
        {
            if (session.Socket.State == WebSocketState.Open)
            {
                try
                {
                    await session.Socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, message, CancellationToken.None);
                    _logger.LogInformation($"{session} forced closed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{session} forced closed failed");
                }
            }
        }

        /// <summary>
        /// /// Close client connection
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task AbortConnectionAsync(Guid sessionId, string message)
        {
            SocketConnectionSession session;
            if (_clients.TryGetValue(sessionId, out session))
            {
                await AbortConnectionAsync(session, message);
            }
            else
            {
                _logger.LogInformation($"${nameof(AbortConnectionAsync)} - could not find sessionId={sessionId}");
            }
        }

        /// <summary>
        /// Accept an incoming websocket connection request
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public async Task HandleWebSocketRequestAsync(HttpContext httpContext)
        {
            using WebSocket socket = await httpContext.WebSockets.AcceptWebSocketAsync();
            var webSocketConnectionSession = new SocketConnectionSession
            {
                Socket = socket,
                HttpContext = httpContext
            };
            _clients.TryAdd(webSocketConnectionSession.Id, webSocketConnectionSession);
            OnOpen(webSocketConnectionSession);
            await RecieveMessageAsync(webSocketConnectionSession, async (result, buffer) =>
            {
                switch (result.MessageType)
                {
                    case WebSocketMessageType.Text:
                        var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        OnMessage(webSocketConnectionSession, msg);
                        break;
                    case WebSocketMessageType.Binary:
                        OnBinary(webSocketConnectionSession, buffer);
                        break;
                    case WebSocketMessageType.Close:
                        webSocketConnectionSession.DisconnectedDateUTC = DateTime.UtcNow;
                        SocketConnectionSession sesion;
                        _clients.TryRemove(webSocketConnectionSession.Id, out sesion);
                        OnClose(webSocketConnectionSession);
                        break;
                }
            });
        }

        /// <summary>
        /// Recieve client's incoming - message /connection clsoed
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        private async Task RecieveMessageAsync(SocketConnectionSession session, Action<WebSocketReceiveResult, byte[]> handler)
        {
            var buffer = new byte[_clientMessageBufferSize];
            while (session.Socket.State == WebSocketState.Open)
            {
                var result = await session.Socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer), cancellationToken: CancellationToken.None);
                handler(result, buffer);
            }
        }

    }
}