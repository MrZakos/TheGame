using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;

namespace TheGame.ClientConsole
{
    public class TheGameClientConsole
    {
        private readonly ILogger<TheGameClientConsole> _logger;

        public TheGameClientConsole()
        {

        }

        public void ShowWelcome()
        {
            Console.ForegroundColor = ConsoleColor.Green;  
            Console.WriteLine("|=====================================|");
            Console.WriteLine("|         Welcome to The Game  !      |");
            Console.WriteLine("|=====================================|");
            Console.ResetColor();          
          }

        public void ShowAvailableCommands()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("|=============================================================|");
            Console.WriteLine("|                     Available Commands                      |");
            Console.WriteLine("|=============================================================|");
            Console.WriteLine("| i.    Login {UUID}                                          |");
            Console.WriteLine("| ii.   Update {Coin/Roll} {Ammount}                          |");
            Console.WriteLine("| iii.  Gift {FriendId} {Coin/Roll} {Ammount}                 |");
            Console.WriteLine("| iv.   exit to terminate                                     |");
            Console.WriteLine("|=============================================================|");
            Console.ResetColor();
        }

        public void WriteToConsole(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public void OnMessageReceived(ResponseMessage message)
        {

        }

        public async Task Start(string webSocketServerURL)
        {
            ShowWelcome();
            Console.WriteLine(string.Empty);

            var exitEvent = new ManualResetEvent(false);
            var url = new Uri(webSocketServerURL);

            using (var client = new WebsocketClient(url))
            {
                client.MessageReceived.Subscribe(msg => OnMessageReceived(msg));
                WriteToConsole($"connecting ({webSocketServerURL}) ...", ConsoleColor.Yellow);
                await client.Start();
                WriteToConsole("connected", ConsoleColor.Yellow);
                ReadCommandsFromConsole(client);
                exitEvent.WaitOne();
            }
        }

        public void ReadCommandsFromConsole(WebsocketClient client)
        {
            Console.WriteLine(string.Empty);
            ShowAvailableCommands();
            Console.WriteLine(string.Empty);
            var keepLoop = true;
            do
            {
                WriteToConsole("type a command (type quit/exit to terminate)", ConsoleColor.Green);
                var inputCommand = Console.ReadLine();
                Task.Run(() => client.Send(inputCommand));
            } while (keepLoop);
        }
    }
}
