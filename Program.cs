using Anna.Core.Services;
using Serilog;

namespace SchichtberichtInterface
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args)
               .UseSerilog()
               .Build();

            host.Run();
        }



        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UsePluginServiceProvider()
                //.UseUnityServiceProvider()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // Without this it throws in Kestrel init that ApplicationServices is null 
                    webBuilder.UseKestrel();
                    webBuilder.UseStartup<Startup>();
                });
    }
}
