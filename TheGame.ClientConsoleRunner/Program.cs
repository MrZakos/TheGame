using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using TheGame.BootstrapService;
using TheGame.Common.Interfaces;

namespace TheGame.ClientConsoleRunner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Bootstrap.ConsoleApplicationBoostrap();
                var client = ActivatorUtilities.GetServiceOrCreateInstance<IClient>(Bootstrap.IHost.Services);
                await client.Start();
            }
            catch (System.Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
            }
        }
    }          
}