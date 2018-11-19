﻿using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Extensions.DependencyInjection;
using StructureMap;
using Microsoft.Extensions.Logging;
using RadioMessagesProcessor.Services;
using radioMessagesProcessor.Services;
using RadioMessagesProcessor.Helpers;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace RadioMessagesProcessor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", true)
              .Build();

            //add the framework services
            var services = new ServiceCollection()
                .AddLogging();

          

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContextPool<DataContext>(
                    options => options.UseMySql(connectionString,
                        mysqlOptions =>
                        {
                            mysqlOptions.MaxBatchSize(1);
                            mysqlOptions.ServerVersion(new Version(8, 0, 13), ServerType.MySql);
                            mysqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(1), null);
                        }
                ));



            var appSettingsSection = configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            var appSettings = appSettingsSection.Get<AppSettings>();


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