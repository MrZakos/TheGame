using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TheGame.Common.Models;
using TheGame.DAL;
using TheGame.WebSocketService;
using System.Text.Json;
using System.Linq;

namespace TheGame.BLL
{
    /// <summary>
    /// Business Logic Layer - handle websocket connection and events
    /// </summary>
    public class BusinessLogicLayer
    {
        private readonly ILogger<BusinessLogicLayer> _logger;
        private readonly DataAccessLayer _dal;
        private readonly WebSocketConnectionManager _webSocketConnectionManager;

        public BusinessLogicLayer(
            ILogger<BusinessLogicLayer> log,
            DataAccessLayer dataAccessLayer,
            WebSocketConnectionManager webSocketConnectionManager)
        {
            _logger = log;
            _dal = dataAccessLayer;
            _webSocketConnectionManager = webSocketConnectionManager;
            SubscribeToWebSocketsEvents();
            _dal.EnsureCreated().GetAwaiter().GetResult();
        }

        public void SubscribeToWebSocketsEvents()
        {
            _webSocketConnectionManager.Subscribe(connection =>
            {
                connection.OnOpen = async (session) => await OnWebSocketOpenedAsync(session);
                connection.OnClose = async (session) => await OnWebSocketClosedAsync(session);
                connection.OnMessage = async (session, message) => await OnWebSocketMessageAsync(session, message);
                connection.OnBinary = async (session, bytes) => await OnWebSocketBinaryMessageAsync(session, bytes);
            });
        }

        public async Task ProcessAcceptWebSocketRequestAsync(HttpContext httpContext)
        {
            var context = httpContext;
            var isSocketRequest = context.WebSockets.IsWebSocketRequest;

            if (!isSocketRequest)
            {
                _logger.LogInformation($"a non web socket request incoming from {httpContext.Connection.RemoteIpAddress}, response with status code 400");
                context.Response.StatusCode = 400;
                return;
            }
            await _webSocketConnectionManager.AcceptAndConnectIncomingWebSocketRequestAsync(httpContext);
        }

        public async Task ProcessOnClientWebSocketMessageReceivedAsync(SocketConnectionSession session, string message)
        {
            var model = default(WebSocketServerClientDTO);
            try
            {
                _logger.LogInformation($"{session} message received {message}");

                // model validation
                model = JsonSerializer.Deserialize<WebSocketServerClientDTO>(message);
                WebSocketServerClientEventCode eventCode;
                if (!Enum.TryParse(model.Event, out eventCode))
                {
                    await SendUnSuccessResponseAsync(Common.Constants.STRING_UnknownEvent);
                    return;
                }

                if (model == null)
                {
                    await SendUnSuccessResponseAsync(Common.Constants.STRING_InvalidRequest);
                    return;
                }

                _logger.LogInformation($"{session} processing message");
                switch (eventCode)
                {
                    case WebSocketServerClientEventCode.Login:
                        await ProcessLoginAsync();
                        break;
                    case WebSocketServerClientEventCode.UpdateResources:
                        await ProcessSendGiftAsync();
                        break;
                    case WebSocketServerClientEventCode.SendGift:
                        await ProcessUpdateResourcesAsync();
                        break;
                    default:
                        _logger.LogError($"{session} {nameof(ProcessOnClientWebSocketMessageReceivedAsync)} received unknown event code ${model.EventCode}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{session} processing message failed");
            }

            async Task ProcessLoginAsync()
            {
                // model validation
                var isValidModel =
                    model.LoginRequest != null &&
                    model.LoginRequest.DeviceId.HasValue &&
                    model.LoginRequest.DeviceId != Guid.Empty;
                if (!isValidModel)
                {
                    await SendUnSuccessResponseAsync(Common.Constants.STRING_InvalidRequest);
                    return;
                }

                // if an already connected device is trying to connect(or reconnect) , disconnect previous connection/device
                var isDeviceAlreadyConnected = _webSocketConnectionManager.IsDeviceExists(model.LoginRequest.DeviceId.Value);
                if (isDeviceAlreadyConnected)
                {
                    var connectedDeviceSession = _webSocketConnectionManager.GetByDevice(model.LoginRequest.DeviceId.Value);
                    _logger.LogInformation($"{session} connecting with an already connected device ${model.LoginRequest.DeviceId.Value}");
                    await _webSocketConnectionManager.AbortConnectionAsync(connectedDeviceSession, Common.Constants.STRING_ForcedSignedOutMessage);
                    await Task.Delay(3000);
                }

                // register user if not already exists and response with playerId 
                var player = await _dal.RegisterPlayerAsync(model.LoginRequest.DeviceId.Value, true);
                session.Player = player;
                var response = new WebSocketServerClientDTO
                {
                    RequestId = model.RequestId,
                    Event = model.EventCode.ToString(),
                    Success = true,
                    LoginResponse = new LoginResponse
                    {
                        PlayerId = player.Id
                    }
                };
                await SendResponseAsync(response);
            }

            async Task ProcessUpdateResourcesAsync()
            {
                // model validation

                ResourceType resourceType = default(ResourceType);
                var isValidModel =
                    model.UpdateResourcesRequest != null &&
                    model.UpdateResourcesRequest.ResourceType != null &&
                    Enum.TryParse(model.UpdateResourcesRequest.ResourceType, out resourceType) &&
                    model.UpdateResourcesRequest.ResourceValue.HasValue &&
                    model.UpdateResourcesRequest.ResourceValue.Value >= 0;

                if (!isValidModel)
                {
                    await SendUnSuccessResponseAsync(Common.Constants.STRING_InvalidRequest);
                    return;
                }

                // login validation
                if (!session.HasConnectedPlayer)
                {
                    await SendUnSuccessResponseAsync(Common.Constants.STRING_ThisOperationRequireToBeLoggedIn);
                    return;
                }

                await _dal.AddOrUpdateResourceForPlayerAsync(session.Player.Id, resourceType, model.UpdateResourcesRequest.ResourceValue.Value);
                var response = new WebSocketServerClientDTO
                {
                    RequestId = model.RequestId,
                    Event = model.EventCode.ToString(),
                    Success = true,
                    UpdateResourcesResponse = new UpdateResourcesResponse
                    {
                        Balance = model.UpdateResourcesRequest.ResourceValue.Value
                    }
                };
                await SendResponseAsync(response);
            }

            async Task ProcessSendGiftAsync()
            {
                // model validation
                ResourceType resourceType = default(ResourceType);
                var isValidModel =
                    model.SendGiftRequest != null &&
                    model.SendGiftRequest.FriendPlayerId.HasValue &&
                    model.SendGiftRequest.ResourceType != null &&
                    Enum.TryParse(model.UpdateResourcesRequest.ResourceType, out resourceType) &&
                    model.SendGiftRequest.ResourceValue.HasValue &&
                    model.SendGiftRequest.ResourceValue.Value > 0;

                if (!isValidModel)
                {
                    await SendUnSuccessResponseAsync(Common.Constants.STRING_InvalidRequest);
                    return;
                }

                // login validation
                if (!session.HasConnectedPlayer)
                {
                    await SendUnSuccessResponseAsync(Common.Constants.STRING_ThisOperationRequireToBeLoggedIn);
                    return;
                }

                // find friend player
                var player = await _dal.GetPlayerAsync(model.SendGiftRequest.FriendPlayerId.Value);
                var isFriendPlayerExists = player != null;
                if (!isFriendPlayerExists)
                {
                    await SendUnSuccessResponseAsync(Common.Constants.STRING_PlayerDoesNotExists);
                    return;
                }
                // update friend player resource
                var friendPlayerResource = player.Resources.FirstOrDefault(x => x.ResourceType == resourceType);
                var friendPlayerResourceExists = friendPlayerResource != null;
                var newBalance = friendPlayerResourceExists ? (friendPlayerResource.ResourceValue + model.SendGiftRequest.ResourceValue.Value) : model.SendGiftRequest.ResourceValue.Value;
                await _dal.AddOrUpdateResourceForPlayerAsync(
                    session.Player.Id,
                    resourceType,
                    newBalance);

                // send GiftEvent to friend if online
                var isDeviceAlreadyConnected = _webSocketConnectionManager.IsExists(player.DeviceId);
                var response = new WebSocketServerClientDTO
                {
                    Event = WebSocketServerClientEventCode.SendGift.ToString(),
                    SendGiftResponse = new SendGiftResponse
                    {
                        CurrentResourceBalance = newBalance
                    }
                };
                await SendResponseAsync(response);
            }

            async Task SendUnSuccessResponseAsync(string message)
            {
                var response = new WebSocketServerClientDTO
                {
                    RequestId = model.RequestId,
                    EventCode = model.EventCode,
                    Success = false,
                    Message = message
                };
                await SendResponseAsync(response);
            }

            async Task SendResponseAsync(WebSocketServerClientDTO response)
            {
                var message = JsonSerializer.Serialize(response, new JsonSerializerOptions { IgnoreNullValues = true, WriteIndented = true });
                await _webSocketConnectionManager.SendMessageAsync(session, message);
            }
        }

        public async Task ProcessOnClientWebSocketConnectionOpenedAsync(SocketConnectionSession session)
        {
            try
            {
                _logger.LogInformation($"{session} connected");
            }
            catch (Exception ex)
            {

            }
        }

        public async Task ProcessOnClientWebSocketConnectionClosedAsync(SocketConnectionSession session)
        {
            try
            {
                _logger.LogInformation($"{session} disconnected");
                if (session.HasConnectedPlayer)
                {
                    await _dal.SetOnlineStatusAsync(session.Player.Id, false);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public async Task OnWebSocketOpenedAsync(SocketConnectionSession session)
        {
            await ProcessOnClientWebSocketConnectionOpenedAsync(session);
        }

        public async Task OnWebSocketClosedAsync(SocketConnectionSession session)
        {
            await ProcessOnClientWebSocketConnectionClosedAsync(session);
        }

        public async Task OnWebSocketMessageAsync(SocketConnectionSession session, string message)
        {
            await ProcessOnClientWebSocketMessageReceivedAsync(session, message);
        }

        public async Task OnWebSocketBinaryMessageAsync(SocketConnectionSession session, byte[] bytes)
        {
            throw new NotImplementedException();
        }
    }
}
