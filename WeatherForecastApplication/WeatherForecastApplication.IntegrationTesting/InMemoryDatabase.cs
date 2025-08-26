using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using WeatherForecastApplication.Data;

namespace WeatherForecastApplication.IntegrationTesting
{
    public class InMemoryDatabase:IClassFixture<WeatherForecastFactory>
    {
        private WeatherForecastFactory factory;

        public InMemoryDatabase(WeatherForecastFactory factory)
        {
            this.factory = factory;
        }
        public HttpClient CreateClient()
        {
            return factory.CreateClient();

        }
        public IServiceScope CreateScope()
        {
            return factory.Services.CreateScope();
        }
        public async Task DisposeAsync()
        {
            await factory.DisposeAsync();
        }
        public async Task emptyDB(WeatherAppDbContext weatherAppDbContext) {
            await using (var transaction = await weatherAppDbContext.Database.BeginTransactionAsync())
            {
                await weatherAppDbContext.Sites.ExecuteDeleteAsync();
                await weatherAppDbContext.Cities.ExecuteDeleteAsync();
                await weatherAppDbContext.Users.ExecuteDeleteAsync();
                await weatherAppDbContext.Timeslots.ExecuteDeleteAsync();
                await weatherAppDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
        }
    }
}
