using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace simple.dbapp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(assemblyPath)
                .AddJsonFile($"appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var config = configBuilder.Build();

            Console.WriteLine($"Building Services");

            var collection = new ServiceCollection()
                    .AddLogging()
                    .AddSingleton(typeof(IConfigurationFacade), new ConfigurationFacade(config))
                    .AddTransient<Startup>()
                    .AddDbContext<SimpleDbContext>();

            //collection.AddHttpClient<IAuthService, AuthService>();

            var serviceProvider = collection.BuildServiceProvider();

            // create database if not present and apply migrations
            using (var serviceScope = serviceProvider.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<SimpleDbContext>();
                context.Database.Migrate();
            }

            //configure console logging
            using (var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole()))
            {
                // use loggerFactory
                var logger = loggerFactory.CreateLogger<Program>();
                logger.LogDebug("Starting application");

                //do the actual work here
                var app = serviceProvider.GetService<Startup>();
                await app.Run();

                logger.LogDebug("All done!");
            }
        }
    }
}