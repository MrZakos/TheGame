using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TheGame.Common.Interfaces;
using TheGame.Common.Models;
using Websocket.Client;
using Websocket.Client.Models;

namespace TheGame.ClientConsole
{
    public class TheGameClientConsole : IClient
    {
        private readonly ILogger<TheGameClientConsole> _logger;
        private readonly ClientOptions _options;
        private static readonly ManualResetEvent _exitEvent = new ManualResetEvent(false);
        private static bool _keepConsoleCommandRead = true;

        public TheGameClientConsole(ILogger<TheGameClientConsole> logger, IOptions<ClientOptions> options)
        {
            _logger = logger;
            _options = options.Value;
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
            Console.WriteLine(" | iv.   ESC to exit                                           |");
            Console.WriteLine(" |=============================================================|");
            Console.ResetColor();
        }

        public void WriteToConsole(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public async Task Start()
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;
            AssemblyLoadContext.Default.Unloading += DefaultOnUnloading;
            Console.CancelKeyPress += ConsoleOnCancelKeyPress;

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
                //client.ReconnectTimeout = TimeSpan.FromSeconds(30);
                client.ReconnectionHappened.Subscribe(info => OnReconnection(info));
                client.MessageReceived.Subscribe(msg => OnMessageReceived(msg));
                client.IsReconnectionEnabled = false;
                WriteToConsole($" connecting ({this._options.WebSocketURL}) ...", ConsoleColor.Yellow);
                _logger.LogInformation($"connecting ({this._options.WebSocketURL}) ...");
                try
                {
                    await client.StartOrFail();
                }
                catch (Exception ex)
                {
                    WriteToConsole($" connecting failed ({ex.Message})", ConsoleColor.Red);
                    _logger.LogError(ex, $"connecting failed");
                    _exitEvent.Set();
                    return;
                }
                WriteToConsole(" connected", ConsoleColor.Yellow);
                _logger.LogInformation($"connected");
                ReadCommandsFromConsole(client);
                _exitEvent.WaitOne();
            }
        }

        public void ReadCommandsFromConsole(WebsocketClient client)
        {
            Console.WriteLine(string.Empty);
            ShowAvailableCommands();
            Console.WriteLine(string.Empty);
            do
            {
                WriteToConsole(" type a command", ConsoleColor.Green);
                var inputCommand = Console.ReadLine();
                var commandParser = new CommandParser(inputCommand);
                if (commandParser.IsValid && client.IsRunning)
                {
                    WriteToConsole($" >> ({commandParser.Model.RequestId}) ({inputCommand}) ...", ConsoleColor.Yellow);
                    Task.Run(() => client.Send(commandParser.ConvertToServerJSONRequest));
                }
                else
                {
                    WriteToConsole(client.IsRunning ? "invalid command" :"not connected", ConsoleColor.Red);
                }

            } while (_keepConsoleCommandRead);
        }

        public void OnMessageReceived(ResponseMessage message)
        {
            var response = JsonSerializer.Deserialize<WebSocketServerClientDTO>(message.Text);
            var msg = default(string);
            WebSocketServerClientEventCode eventCode;
            if (!Enum.TryParse(response.Event, out eventCode))
            {

            }
            switch (eventCode)
            {
                case WebSocketServerClientEventCode.Login:
                    msg = $"PlayerId={response.LoginResponse.PlayerId}";
                    break;
                case WebSocketServerClientEventCode.UpdateResources:
                    msg = $"Balance={response.UpdateResourcesResponse.Balance}";
                    break;
                case WebSocketServerClientEventCode.SendGift:
                    msg = $"you got a gift of {response.SendGiftResponse.ResourceValue} {response.SendGiftRequest.ResourceType.ToLower()}s from playerId={response.SendGiftResponse.FromPlayerId}";
                    break;
                case WebSocketServerClientEventCode.Message:
                    break;
            }

            WriteToConsole($" << ({response.RequestId}) {msg}", response.Success.Value ? ConsoleColor.Yellow : ConsoleColor.Red);
        }

        public void OnReconnection(ReconnectionInfo info)
        {
            _logger.LogWarning($"reconnection happened, type: {info.Type}");
        }

        public void OnDisconnection(DisconnectionInfo info)
        {
            _logger.LogWarning($"disconnected ({info.Type}) ({info.CloseStatus}) {info.CloseStatusDescription}");
            if(info.Type == DisconnectionType.ByServer)
            {
                WriteToConsole($"server closed connection ({info.CloseStatus}) {info.CloseStatusDescription}", ConsoleColor.Yellow);
            }            
            _keepConsoleCommandRead = false;
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
        }
    }
}
