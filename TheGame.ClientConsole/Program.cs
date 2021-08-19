using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using TheGame.BootstrapService;
using TheGame.DataService;

namespace TheGame.ClientConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Bootstrap.ConsoleApplicationBoostrap();           
        }  
    }
}