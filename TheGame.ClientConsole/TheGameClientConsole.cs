using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.WebSockets;
using System.Runtime.Loader;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TheGame.Common.Interfaces;
using TheGame.Common.Models;
using Websocket.Client;
using Websocket.Client.Models;

namespace TheGame.ClientConsole
{
    /// <summary>
    /// The Client - connects to the TheGame web soceket server and play by sending text commands
    /// </summary>
    public class TheGameClientConsole : IClient
    {
        private readonly ILogger<TheGameClientConsole> _logger;
        private readonly ClientOptions _options;
        private static readonly ManualResetEvent _exitEvent = new ManualResetEvent(false);
        private static bool _keepConsoleCommandRead = true;
        private WebsocketClient _client;

        public TheGameClientConsole(ILogger<TheGameClientConsole> logger, IOptions<ClientOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public async Task Start()
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;
            AssemblyLoadContext.Default.Unloading += DefaultOnUnloading;
            Console.CancelKeyPress += ConsoleOnCancelKeyPress;
            ShowTheGameDrawing();
            ShowWelcome();
            Console.WriteLine(string.Empty);
            var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
            {
                Options =
                    {
                      KeepAliveInterval = TimeSpan.FromSeconds(_options.KeepAliveIntervalSeconds),
                    }
            });
            using (var client = new WebsocketClient(new Uri(this._options.WebSocketURL), factory))
            {
                client.DisconnectionHappened.Subscribe(info => OnDisconnection(info));
                client.ReconnectionHappened.Subscribe(info => OnReconnection(info));
                client.MessageReceived.Subscribe(msg => OnMessageReceived(msg));
                client.IsReconnectionEnabled = false;
                WriteToConsole($"connecting ({this._options.WebSocketURL}) ...", ConsoleColor.Yellow);
                _logger.LogInformation($"connecting ({this._options.WebSocketURL}) ...");
                try
                {
                    await client.StartOrFail();
                    _client = client;
                }
                catch (Exception ex)
                {
                    WriteToConsole($"connecting failed ({ex.Message})", ConsoleColor.Red);
                    _logger.LogError(ex, $"connecting failed");
                    _exitEvent.Set();
                    return;
                }
                WriteToConsole("connected", ConsoleColor.Yellow);
                _logger.LogInformation($"connected");
                ReadCommandsFromConsole(client);
                _exitEvent.WaitOne();
            }
        }

        public void ReadCommandsFromConsole(WebsocketClient client)
        {
            ShowAvailableCommands();
            do
            {
                WriteToConsole("type a command", ConsoleColor.Green);
                var inputCommand = Console.ReadLine();
                var commandParser = new CommandParser(inputCommand);
                if (commandParser.IsValid && client.IsRunning)
                {
                    WriteToConsole($" >> ({commandParser.Model.RequestId}) ({inputCommand}) ...", ConsoleColor.Yellow);
                    Task.Run(() => client.Send(commandParser.ConvertToServerJSONRequest));
                }
                else
                {
                    WriteToConsole(client.IsRunning ? "invalid command" : "not connected", ConsoleColor.Red);
                }

            } while (_keepConsoleCommandRead);
        }

        public void OnMessageReceived(ResponseMessage message)
        {
            _logger.LogInformation($"server message : type=${message.MessageType} text${message.Text}");
            var model = default(WebSocketServerClientDTO);
            try
            {
                if (message.MessageType != WebSocketMessageType.Text)
                {
                    _logger.LogInformation("message is not WebSocketMessageType.Text, ignore it");
                    return;
                }
                try
                {
                    model = JsonSerializer.Deserialize<WebSocketServerClientDTO>(message.Text);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "could not parse model");
                }
                var msg = default(string);
                WebSocketServerClientEventCode eventCode;
                Enum.TryParse(model.Event, out eventCode);
                if (model.Success.Value)
                {
                    switch (eventCode)
                    {
                        case WebSocketServerClientEventCode.Login:
                            msg = $"PlayerId={model.LoginResponse.PlayerId}";
                            break;
                        case WebSocketServerClientEventCode.UpdateResources:
                            msg = $"Balance={model.UpdateResourcesResponse.Balance}";
                            break;
                        case WebSocketServerClientEventCode.SendGift:
                            msg = model.SendGiftResponse.Message;
                            break;
                        case WebSocketServerClientEventCode.GiftEvent:
                            msg = $"you got a gift of {model.GiftEvent.ResourceValue} {model.GiftEvent.ResourceType.ToLower()}s from playerId={model.GiftEvent.FromPlayerId}, previous balance = {model.GiftEvent.PreviousResourceBalance},current balance = {model.GiftEvent.CurrentResourceBalance}";
                            ShowGiftDrawing();
                            break;
                        case WebSocketServerClientEventCode.Message:
                            msg = model.Message;
                            break;
                    }
                }
                else
                {
                    msg = $"request failed : ({model.Message})";
                }

                WriteToConsole($" << ({model.RequestId}) {msg}", model.Success.Value ? ConsoleColor.Yellow : ConsoleColor.Red);
            }
            catch (Exception ex)
            {
                WriteToConsole($" error in handling server message ${ex.Message}", ConsoleColor.Red);
                _logger.LogError(ex, "error in handling server message");
            }
        }

        public void OnReconnection(ReconnectionInfo info)
        {
            _logger.LogWarning($"reconnection happened, type: {info.Type}");
        }

        public void OnDisconnection(DisconnectionInfo info)
        {
            _logger.LogWarning($"disconnected ({info.Type}) ({info.CloseStatus}) {info.CloseStatusDescription}");
            if (info.Type == DisconnectionType.ByServer)
            {
                WriteToConsole($"server closed connection ({info.CloseStatus}) {info.CloseStatusDescription}", ConsoleColor.Yellow);
            }
            _exitEvent.Set();
        }

        private void CurrentDomainOnProcessExit(object sender, EventArgs eventArgs)
        {
            _logger.LogWarning("exiting process");
            _exitEvent.Set();
        }

        private void DefaultOnUnloading(AssemblyLoadContext assemblyLoadContext)
        {
            _logger.LogWarning("unloading process");
            _exitEvent.Set();
        }

        private void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            _logger.LogWarning("canceling process");
            e.Cancel = true;
            _exitEvent.Set();
            CloseSocketConnection();
            Environment.Exit(-1);
        }

        public void CloseSocketConnection()
        {
            try
            {
                if (_client.IsRunning)
                {
                    _client.Stop(WebSocketCloseStatus.NormalClosure, string.Empty).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(CloseSocketConnection));
            }
        }

        public void ShowWelcome()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(" |=====================================|");
            Console.WriteLine(" |         Welcome to The Game  !      |");
            Console.WriteLine(" |=====================================|");
            Console.ResetColor();
        }

        public void ShowAvailableCommands()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(" |=============================================================|");
            Console.WriteLine(" |                     Available Commands                      |");
            Console.WriteLine(" |=============================================================|");
            Console.WriteLine(" | i.    Login {UUID}                                          |");
            Console.WriteLine(" | ii.   Update {Coin/Roll} {Ammount}                          |");
            Console.WriteLine(" | iii.  Gift {FriendId} {Coin/Roll} {Ammount}                 |");
            Console.WriteLine(" | iv.   Ctrl+C to exit                                        |");
            Console.WriteLine(" |=============================================================|");
            Console.ResetColor();
        }

        public void ShowGiftDrawing()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(@"              .__.      .==========.   ");
            Console.WriteLine(@"            .(\\//).  .-[ for you! ]   ");
            Console.WriteLine(@"           .(\\()//)./  '=========='   ");
            Console.WriteLine(@"       .----(\)\/(/)----.              ");
            Console.WriteLine(@"       |     ///\\\     |              ");
            Console.WriteLine(@"       |    ///||\\\    |              ");
            Console.WriteLine(@"       |   //`||||`\\   |              ");
            Console.WriteLine(@"       |      ||||      |              ");
            Console.WriteLine(@"       |      ||||      |              ");
            Console.WriteLine(@"       |      ||||      |              ");
            Console.WriteLine(@"       |      ||||      |              ");
            Console.WriteLine(@"       |      ||||      |              ");
            Console.WriteLine(@"       |      ||||      |              ");
            Console.WriteLine(@"       '------====------'              ");
            Console.ResetColor();
        }

        public void ShowTheGameDrawing()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(@"           _______ _             _____                                  ");
            Console.WriteLine(@"          |__   __| |           / ____|                                 ");
            Console.WriteLine(@"             | |  | |__   ___   | |  __  __ _ _ __ ___   ___            ");
            Console.WriteLine(@"             | |  | '_ \ / _ \  | | |_ |/ _` | '_ ` _ \ / _ \           ");
            Console.WriteLine(@"             | |  | | | |  __/  | |__| | (_| | | | | | |  __/           ");
            Console.WriteLine(@"             |_|  |_| |_|\___|   \_____|\__,_|_| |_| |_|\___|           ");
            Console.ResetColor();
        }

        public void WriteToConsole(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}