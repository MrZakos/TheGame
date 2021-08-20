using System.Threading.Tasks;
using TheGame.ClientConsole;

namespace TheGame.ClientConsoleRunner
{
    class Program
    {

        static async Task Main(string[] args)
        {
            var client = new TheGameClientConsole();
            await client.Start("wss://localhost:44329/connect");
        }
    }

      

    
}
