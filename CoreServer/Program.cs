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
            var builder = WebHost.CreateDefaultBuilder(args)
                   .UseStartup<Startup>();
            Console.WriteLine("Please enter the desired address: (e.g. http://localhost:5001/ or http://localhost:62752/)");
#if !DEBUG
            
            builder.UseUrls(Console.ReadLine());
#endif
            return builder.Build();
        }
    }
}
