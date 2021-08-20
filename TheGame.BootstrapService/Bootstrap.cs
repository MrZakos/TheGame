using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using TheGame.BLL;
using TheGame.DAL;
using TheGame.DataService;
using TheGame.WebSocketService;

namespace TheGame.BootstrapService
{
    public static class Bootstrap
    {
        public static IConfiguration Configuration { get; set; }
        public static IHost IHost { get; set; }
        public static void ConsoleApplicationBoostrap()
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);
            Configuration = builder.Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Build())
                .CreateLogger();

            Log.Logger.Information("application starting");

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    RegisterServices(Configuration,services);
                })
                .UseSerilog()
                .Build();

            IHost = host;
        }

        static void BuildConfig(IConfigurationBuilder builder) => builder
                                                                    .SetBasePath(Directory.GetCurrentDirectory())
                                                                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                                                    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                                                                    .AddEnvironmentVariables();


        public static void RegisterServices(IConfiguration configuration,IServiceCollection services)
        {
            var connectionString = configuration["ConnectionStrings:Sqlite"];
            services.AddDbContext<TheGameDatabaseContext>(options => options.UseSqlite(connectionString),ServiceLifetime.Transient);
            services.AddTransient<IResourceRepository, ResourceRepository>();
            services.AddTransient<IPlayerRepository, PlayerRepository>();
            services.AddTransient<IUnitOfWork, UnitOfWork>();         
            services.AddTransient<DataAccessLayer>();
            services.AddTransient<BusinessLogicLayer>();
            services.AddSingleton<WebSocketConnectionManager>();

        }
    }
}