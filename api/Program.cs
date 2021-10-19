using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace dotnet_webapi_db_testcontainers
{
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        public static void Main(string[] args)
        {
            IWebHost host;
            try
            {
                host = CreateWebHostBuilder(args).Build();
            }
            catch (Exception e)
            {
                Console.Out.WriteLine($"Fatal error on application startup: {e.Message}");
                return;
            }
            host.Run();
        }

        // EF Core uses this method at design time to access the DbContext
        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
            => WebHost.CreateDefaultBuilder(args)
                .CaptureStartupErrors(true)
                .UseKestrel()
                .UseUrls("http://::5000")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>();
    }
}
