using Microsoft.EntityFrameworkCore;
using LocationData.Entities;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;

namespace LocationData
{

    // command for options creation:
    // dotnet ef dbcontext scaffold "Server=192.168.1.8;Database=locations;User=locuser;Password=password1!;" "Pomelo.EntityFrameworkCore.MySql"

    // command for migration creation:
    // dotnet ef migrations add InitialCreate

    // command for applying the migration to the db:
    // dotnet ef database update

    public class DbContextFactory : IDesignTimeDbContextFactory<LocationDataContext>
    {
        public LocationDataContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", true)
           .Build();

            var optionsBuilder = new DbContextOptionsBuilder<LocationDataContext>();

            optionsBuilder.UseMySql(configuration.GetConnectionString("DefaultConnection"),
                            mysqlOptions =>
                            {
                                mysqlOptions.MaxBatchSize(1);
                                mysqlOptions.ServerVersion(new Version(8, 0, 13), ServerType.MySql);
                                mysqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(1), null);
                            });

            return new LocationDataContext(optionsBuilder.Options);
        }
    }

    public class LocationDataContext : DbContext
    {
        public LocationDataContext(DbContextOptions<LocationDataContext> options) : base(options) { }

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