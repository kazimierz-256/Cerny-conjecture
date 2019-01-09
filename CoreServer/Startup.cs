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

namespace CoreServer
{
    public class Startup
    {
        #region Important parameter
        private int AutomatonProblemSize = 8;
        private int maximumCount = 100;
        #endregion

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSignalR(opts => opts.EnableDetailedErrors = true)
                    .AddMessagePackProtocol();

            var cmdArgs = Environment.GetCommandLineArgs();
            if (cmdArgs.Length >= 3)
            {
                if (!int.TryParse(cmdArgs[2], out AutomatonProblemSize))
                    throw new Exception("Incorrect automaton problem size");
                if (cmdArgs.Length >= 4)
                {
                    if (!int.TryParse(cmdArgs[3], out maximumCount))
                        throw new Exception("Incorrect maximal found automata.");
                }
            }

            var database = new UnaryAutomataDB(AutomatonProblemSize, maximumCount);
            ProgressIO.ProgressIO.ImportStateIfPossible(database);
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

            Console.WriteLine("To participate in distributed computing project just hook up to '/ua'");
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
