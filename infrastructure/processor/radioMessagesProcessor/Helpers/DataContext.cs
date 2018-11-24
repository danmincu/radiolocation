using Microsoft.EntityFrameworkCore;
using RadioMessagesProcessor.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RadioMessagesProcessor.Helpers;
using RadioMessagesProcessor.Services;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Pomelo.EntityFrameworkCore.MySql;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;

namespace RadioMessagesProcessor.Helpers
{

    // command for options creation:
    // dotnet ef dbcontext scaffold "Server=192.168.1.8;Database=locations;User=locuser;Password=password1!;" "Pomelo.EntityFrameworkCore.MySql"

    // command for migration creation:
    // dotnet ef migrations add InitialCreate
    
    // command for applying the migration to the db:
    // dotnet ef database update

    public class DbContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", true)
           .Build();

            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();

            optionsBuilder.UseMySql(configuration.GetConnectionString("DefaultConnection"),
                            mysqlOptions =>
                            {
                                mysqlOptions.MaxBatchSize(1);
                                mysqlOptions.ServerVersion(new Version(8, 0, 13), ServerType.MySql);
                                mysqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(1), null);
                            });

            return new DataContext(optionsBuilder.Options);
        }
    }

    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<RadioLocationMessage> RadioLocationMessages { get; set; }
   
        // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        // {
        //     if (!optionsBuilder.IsConfigured)
        //     {
        //         optionsBuilder.UseMySql("Server=192.168.1.8; Database=locations; User=locuser; Password=password1!;",
        //                 mysqlOptions =>
        //                 {
        //                     mysqlOptions.MaxBatchSize(1);
        //                     mysqlOptions.ServerVersion(new Version(8,0,13), ServerType.MySql);
        //                     mysqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(1), null);
        //                 });
        //     }
        // }

        // protected override void OnModelCreating(ModelBuilder modelBuilder)
        // {
        //     base.OnModelCreating(modelBuilder);
        // }
    }
}