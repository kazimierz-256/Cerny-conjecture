using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoreServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            var builder = WebHost.CreateDefaultBuilder()
                   .UseStartup<Startup>();

            var cmdArgs = Environment.GetCommandLineArgs();
            if (cmdArgs.Length >= 2)
                builder.UseUrls(cmdArgs[1]);

            return builder.Build();
        }
    }
}
