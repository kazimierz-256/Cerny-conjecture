using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SignalR;
using CoreServer.Hubs;
using CoreServer.UnaryAutomataDatabase;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace CoreServer
{
    public class Startup
    {
        #region Important parameters
        private int AutomatonProblemSize = 7;
        private int maximumCount = 20;
        private bool useMessagePack = true;
        private string readAddress = $"./";
        #endregion

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var cmdArgs = Environment.GetCommandLineArgs();
            if (cmdArgs.Length >= 2)
            {
                if (!int.TryParse(cmdArgs[1], out AutomatonProblemSize))
                    throw new Exception("Incorrect automaton problem size");
                if (cmdArgs.Length >= 3)
                {
                    if (!int.TryParse(cmdArgs[2], out maximumCount))
                        throw new Exception("Incorrect maximal found automata.");

                    if (cmdArgs.Length >= 5)
                    {
                        if (!int.TryParse(cmdArgs[4], out var useMessagePackNumber))
                            throw new Exception("Incorrect message pack setting.");
                        useMessagePack = useMessagePackNumber != 0;

                        if (cmdArgs.Length >= 6)
                        {
                            readAddress = cmdArgs[5];
                        }
                    }
                }
            }

            services.AddMvc();
            var signalrbuilder = services.AddSignalR(opts => opts.EnableDetailedErrors = true);
            if (useMessagePack)
                signalrbuilder.AddMessagePackProtocol();
            Console.WriteLine($"Automata size: {AutomatonProblemSize}");
            Console.WriteLine($"Maximum count of interesting automata: {maximumCount}.");
            Console.WriteLine("Please note that some automata including those violating Cerny Conjecture are collected without limits.");
            var database = new UnaryAutomataDB(AutomatonProblemSize, maximumCount);
            ProgressIO.ProgressIO.ImportStateIfPossible(database, readAddress);
            services.AddSingleton(database);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            Console.WriteLine("To participate in distributed computing project just hook up to 'http://address:port/ua' with two subsituted variables accordingly");
            app.UseSignalR(routes =>
            {
                routes.MapHub<UnaryAutomataHub>("/ua", options =>
                {
                    options.ApplicationMaxBufferSize = maximumCount * 10 * 1024;
                    options.TransportMaxBufferSize = options.ApplicationMaxBufferSize;
                });
            });
            app.UseMvc();
        }
    }
}
