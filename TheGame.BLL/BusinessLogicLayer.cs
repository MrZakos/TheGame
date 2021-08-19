using System;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TheGame.Common.Models;
using TheGame.DAL;
using TheGame.WebSocketService;

namespace TheGame.BLL
{

    public enum OperationType
    {
        ClientLogin,
        ClientUpdateResources,
        ClientSendGift,
        ClientSendingMessage
    }


    public class OperationResult
    {
        public OperationType OperationType { get; set; }
        public Exception Exception { get; set; }
        public bool IsSuccess => Exception == null;
        public Stopwatch Stopwatch { get; set; } = new Stopwatch();
        public string Content { get; set; } = string.Empty;
        public object Result { get; set; }
        public object[] Input { get; set; }
        public HttpStatusCode HttpStatusCodeResult { get; set; } = HttpStatusCode.OK;

        public void StartTimer()
        {
            Stopwatch.Start();
        }
        public void StopTimer()
        {
            Stopwatch.Stop();
        }
    }

    /// <summary>
    /// Business Logic Layer
    /// </summary>
    public class BusinessLogicLayer
    {
        private readonly ILogger<BusinessLogicLayer> _logger;
        private readonly DataAccessLayer _dal;
        private readonly WebSocketConnectionManager _webSocketConnectionManager;

        private static readonly SemaphoreSlim _registerWebSocketEventsSemaphoreLock = new SemaphoreSlim(1);
        private bool _registerWebSocketEvents = false;

        public BusinessLogicLayer(
            ILogger<BusinessLogicLayer> log,
            DataAccessLayer dataAccessLayer,
            WebSocketConnectionManager webSocketConnectionManager)
        {
            _logger = log;
            _dal = dataAccessLayer;
            _webSocketConnectionManager = webSocketConnectionManager;
            _dal.EnsureCreated().GetAwaiter().GetResult();
        }

        public async Task<OperationResult> RunOperationAsync(OperationType operationType, params object[] args)
        {
            var result = new OperationResult
            {
                OperationType = operationType,
                Input = args
            };
            try
            {
                result.StartTimer();
                switch (operationType)
                {
                    case OperationType.ClientLogin:
                        await ProcessOnClientLoginAsync(args[0] as HttpContext, args[1] as GameLoginDTO);
                        break;
                }
            }

            catch (Exception ex)
            {
                result.Exception = ex;
            }
            result.StopTimer();
            return result;
        }

        public async Task ProcessOnClientLoginAsync(HttpContext httpContext, GameLoginDTO model)
        {
            // model validation
            if (model.DeviceId == Guid.Empty) throw new Exception(Common.Constants.STRING_MissingDeviceId);

            // workflow
            var isDeviceAlreadyConnected = _webSocketConnectionManager.IsExists(model.DeviceId);
            if (isDeviceAlreadyConnected)
            {
                _webSocketConnectionManager.AbortConnection(model.DeviceId, Common.Constants.STRING_ForcedSignedOutMessage);
            }
            await _webSocketConnectionManager.ConnectAsync(httpContext, model.DeviceId);
        }

        public async Task SubscribeToWebSocketsEventsAsync()
        {
            await _webSocketConnectionManager.SubscribeAsync(connection =>
            {
                connection.OnOpen = async (socket, clientId) => await OnWebSocketOpenedAsync(socket, clientId);
                connection.OnClose = async (socket, clientId) => await OnWebSocketClosedAsync(socket, clientId);
                connection.OnMessage = async (socket, clientId, message) => await OnWebSocketMessageAsync(socket, clientId, message);
                connection.OnBinary = async (socket, clientId, bytes) => await OnWebSocketBinaryMessageAsync(socket, clientId, bytes);
            });
        }

        public async Task OnWebSocketOpenedAsync(WebSocket webSocket, Guid clientId)
        {
            var player = await _dal.RegisterPlayerAsync(clientId, true);
            await _webSocketConnectionManager.SendAsync(clientId, $"player ID = {player.Id}");
        }

        public async Task OnWebSocketClosedAsync(WebSocket webSocket, Guid clientId)
        {
            var player = await _dal.FindPlayerByDeviceIdAsync(clientId);
            await _dal.SetOnlineStatusAsync(player.Id, false);
        }

        public async Task OnWebSocketMessageAsync(WebSocket webSocket, Guid clientId, string message)
        {

        }

        public async Task OnWebSocketBinaryMessageAsync(WebSocket webSocket, Guid clientId, byte[] bytes)
        {

        }

        public async Task ConnectWebSocketConnectionAsync(HttpContext httpContext, Guid clientId)
        {
            await _webSocketConnectionManager.ConnectAsync(httpContext, clientId);
        }

        public async Task SubscribeToWebSocketsEventsIfNotRegistredAsync()
        {
            if (_registerWebSocketEvents) return;
            await _registerWebSocketEventsSemaphoreLock.WaitAsync().ConfigureAwait(false);
            try
            {
                await SubscribeToWebSocketsEventsAsync();
                _registerWebSocketEvents = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Empty);
            }
            finally
            {
                _registerWebSocketEventsSemaphoreLock.Release();
            }

        }
    }
}
