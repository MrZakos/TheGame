using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using System.Net;
using TheGame.BootstrapService;

namespace TheGame.Server
{
    /// <summary>
    /// Server boostrap , load services & configurations
    /// </summary>
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // Configuration object - holds appSettings configurations data
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();                                                              // 
            services.Configure<KestrelServerOptions>(Configuration.GetSection("Kestrel"));          // configure KestrelServerOptions from appSettings
            Bootstrap.RegisterServices(Configuration, services);                                    // register our application services
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Global Error Handling - just log the error
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        var exceptionObject = context.Features.Get<IExceptionHandlerFeature>();
                        if (null != exceptionObject)
                        {
                            logger.LogError(exceptionObject.Error, string.Empty);
                            var errorMessage = $"<b>Exception Error: {exceptionObject.Error.Message} </b> {exceptionObject.Error.StackTrace}";
                            await context.Response.WriteAsync(errorMessage).ConfigureAwait(false);
                        }
                    });
                });
                app.UseHsts();
            }

            // enable websockets
            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120)
            };
            app.UseWebSockets(webSocketOptions);

            // configure static files folder, this allow us to show startup.html on startup
            app.UseFileServer(new FileServerOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles")),
                RequestPath = "/StaticFiles",
                EnableDefaultFiles = true
            });

            // Serilog takes over all default server logs
            app.UseSerilogRequestLogging();

            // regular .net core middlewares
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}