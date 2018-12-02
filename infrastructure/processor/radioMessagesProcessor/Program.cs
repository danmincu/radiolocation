using System.IO;
using Microsoft.Extensions.Configuration;
using System;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using StructureMap;
using Microsoft.Extensions.Logging;
using radioMessagesProcessor.Services;
using RadioMessagesProcessor.Helpers;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.EntityFrameworkCore.Design;
using RadioMessagesProcessor.Services;
using System.Threading.Tasks;

namespace RadioMessagesProcessor
{
    public class Program
    {


        public class Startup
        {
            private IConfigurationRoot configuration;

            public Startup()
            {
                configuration = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json", true)
                 .Build();
            }

            public Startup ConfigureServices(IServiceCollection services)
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                services.AddDbContextPool<DataContext>(options => options.UseMySql(connectionString,
                       mysqlOptions =>
                       {
                           mysqlOptions.MaxBatchSize(1);
                           mysqlOptions.ServerVersion(new Version(8, 0, 13), ServerType.MySql);
                           mysqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(1), null);
                       }
               ), 10);
               return this;
            }

            public IConfigurationRoot Configuration { get { return this.configuration; } }
        }

        //private static void Main()
        //{
        //    var serviceCollection = new ServiceCollection();
        //    var startup = new Startup().ConfigureServices(serviceCollection);
        //    var serviceProvider = serviceCollection.BuildServiceProvider();

        //    SetupDatabase(serviceProvider);


        //    var appSettingsSection = startup.Configuration.GetSection("AppSettings");
        //    serviceCollection.Configure<AppSettings>(appSettingsSection);

        //    var appSettings = appSettingsSection.Get<AppSettings>();


        //    serviceCollection.AddTransient<IRadioLocationMessagesService, RadioLocationMessagesService>();
        //    serviceCollection.AddTransient<IMessageProcessor, MessageProcessor>();

        //    //add structuremp
        //    var container = new Container();
        //    container.Configure(config =>
        //    {
        //                // Register stuff in container, using the StructureMap APIs...
        //                config.Scan(_ =>
        //        {
        //            _.AssemblyContainingType(typeof(Program));
        //            _.WithDefaultConventions();
        //        });
        //                //Populate the container using the service collection
        //                config.Populate(serviceCollection);
        //    });

        //    //var serviceProvider = container.GetInstance<IServiceProvider>();

        //    //configure console logging
        //    serviceProvider
        //        .GetService<ILoggerFactory>()
        //        .AddConsole(appSettings.logLevel);

        //    var logger = serviceProvider.GetService<ILoggerFactory>()
        //        .CreateLogger<Program>();

        //    logger.LogDebug("Starting application");

        //    //do the hard work here
        //    var processor = serviceProvider.GetService<IMessageProcessor>();

        //    processor.Run();

        //    logger.LogDebug("All done!");

        //    //var stopwatch = new Stopwatch();

        //    //MonitorResults(TimeSpan.FromSeconds(Seconds), stopwatch);

        //    //await Task.WhenAll(
        //    //    Enumerable
        //    //        .Range(0, Threads)
        //    //        .Select(_ => SimulateRequestsAsync(serviceProvider, stopwatch)));
        //}

        private static void SetupDatabase(IServiceProvider serviceProvider)
        {
            using (var serviceScope = serviceProvider.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DataContext>();

                if (context.Database.EnsureCreated())
                {
                    //context.Blogs.Add(new Blog { Name = "The Dog Blog", Url = "http://sample.com/dogs" });
                    //context.Blogs.Add(new Blog { Name = "The Cat Blog", Url = "http://sample.com/cats" });
                    //context.SaveChanges();
                }
            }
        }


        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", true)
              .Build();

            //add the framework services
            var services = new ServiceCollection()
                .AddLogging()
                .AddAutoMapper();



            var connectionString = configuration.GetConnectionString("DefaultConnection");

            //todo - here we need a connection pool otherwise hundreeds of possible connections are jamming the database
            //services.AddDbContext<DataContext>(
            //        options => options.UseMySql(connectionString,
            //            mysqlOptions =>
            //            {
            //                mysqlOptions.MaxBatchSize(1);
            //                mysqlOptions.ServerVersion(new Version(8, 0, 13), ServerType.MySql);
            //                mysqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(1), null);
            //            }
            //    ), ServiceLifetime.Transient);

            services.AddDbContextPool<DataContext>(
                    options => options.UseMySql(connectionString,
                        mysqlOptions =>
                        {
                            mysqlOptions.MaxBatchSize(1);
                            mysqlOptions.ServerVersion(new Version(8, 0, 13), ServerType.MySql);
                            mysqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(1), null);
                        }
                ), 10);


            var appSettingsSection = configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            var appSettings = appSettingsSection.Get<AppSettings>();


            services.AddTransient<IRadioLocationMessagesService, RadioLocationMessagesService>();

            //add structuremp
            var container = new Container();
            container.Configure(config =>
            {
                    // Register stuff in container, using the StructureMap APIs...
                    config.Scan(_ =>
                {
                    _.AssemblyContainingType(typeof(Program));
                    _.WithDefaultConventions();
                });
                    //Populate the container using the service collection
                    config.Populate(services);
            });

            var serviceProvider = container.GetInstance<IServiceProvider>();

            //configure console logging
            serviceProvider
                .GetService<ILoggerFactory>()
                .AddConsole(appSettings.logLevel);

            var logger = serviceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();

            logger.LogDebug("Starting application");

            //do the hard work here
            var processor = serviceProvider.GetService<IMessageProcessor>();

            processor.Run();

            logger.LogDebug("All done!");
        }

    }
    }
