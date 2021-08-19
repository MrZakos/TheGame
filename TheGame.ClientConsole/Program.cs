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
     //   public static IConfiguration Configuration { get; set; }

        static async Task Main(string[] args)
        {
            Bootstrap.ConsoleApplicationBoostrap();

            var unitOfWork =  ActivatorUtilities.GetServiceOrCreateInstance<IUnitOfWork>(Bootstrap.IHost.Services);
            await unitOfWork.EnsureCreated();
            var allPLayers = await unitOfWork.Players.All();
            //await unitOfWork.Players.Add(new Common.Models.Player
            //{
            //    Id = 2,
            //    DeviceId = Guid.NewGuid(),
            //    IsOnline = true
            //});
            //await unitOfWork.CompleteAsync();guid 
            //svc.Run();
        }

  
    }
}