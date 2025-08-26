using Docker.DotNet.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.MsSql;
using WeatherForecastApplication.Data;

namespace WeatherForecastApplication.IntegrationTesting
{
    public class WeatherForecastFactory : WebApplicationFactory<Program>,IAsyncLifetime
    {
        private readonly MsSqlContainer _dbContainer;

        public WeatherForecastFactory()
        {
            _dbContainer = new MsSqlBuilder().WithImage("mcr.microsoft.com/mssql/server")
            .WithCleanUp(true)
            .Build();
        }
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var connectionString = $"{_dbContainer.GetConnectionString()};Database=WeatherTestDb";
            base.ConfigureWebHost(builder);
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<WeatherAppDbContext>));
                services.AddDbContext<WeatherAppDbContext>(options => options.UseSqlServer(connectionString));
            });
        }
       
        public async Task InitializeAsync()
        {

            await _dbContainer.StartAsync();
            using (var scope = Services.CreateScope()) {
                var provider = scope.ServiceProvider;
                var context = provider.GetRequiredService<WeatherAppDbContext>();
                await context.Database.EnsureCreatedAsync();

            }
        }

        public async Task DisposeAsync()
        {
            await _dbContainer.StopAsync();
        }
    }
}
