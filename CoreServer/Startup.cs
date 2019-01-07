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
#if DEBUG
            services.AddSignalR(opts => opts.EnableDetailedErrors = true)
                    .AddMessagePackProtocol();
#else
            services.AddSignalR();
#endif

            #region Unary automata database singleton

            var AutomatonProblemSize = 8;

#if !DEBUG
            Console.WriteLine("Please enter the number of states:");
            var success = false;
            while (!success || AutomatonProblemSize <= 1)
            {
                success = int.TryParse(Console.ReadLine(), out AutomatonProblemSize);
            }
#endif
            var database = new UnaryAutomataDB(AutomatonProblemSize);
            ProgressIO.ProgressIO.ImportStateIfPossible(database);
            services.AddSingleton(database);
            #endregion
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
                    options.ApplicationMaxBufferSize = UnaryAutomataDB.MaximumLongestAutomataCount * 10 * 1024;
                    options.TransportMaxBufferSize = options.ApplicationMaxBufferSize;
                });
            });
            app.UseMvc();
        }
    }
}
