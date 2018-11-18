using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace WriteMessagesApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true)
                .Build();
            
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()                
                .UseUrls(config.GetSection("AppSettings")["Url"])
                .Build()
                .Run();

        }
    }
}
