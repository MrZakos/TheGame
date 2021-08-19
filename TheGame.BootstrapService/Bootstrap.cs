using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using TheGame.DataService;

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
                    RegisterServices(services);
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


        public static void RegisterServices(IServiceCollection services)
        {
            var connectionString = Configuration["ConnectionStrings:Sqlite"];
            services.AddDbContext<TheGameDatabaseContext>(options => options.UseSqlite(connectionString));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}