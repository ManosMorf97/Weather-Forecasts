using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using WeatherForecastApplication.Data;
using WeatherForecastApplication.Web.Data;
using WeatherForecastApplication.Web.Models;

namespace WeatherForecastApplication.IntegrationTesting
{
    [Collection("Sequential")]
    public class Different_Sites:InMemoryDatabase
    {
        private WeatherForecastFactory? _factory;
        public Different_Sites(WeatherForecastFactory _factory) : base(_factory) { }
        [Fact]
        public async Task Cities_Some_Intersection() {
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            //Trace.WriteLine()
            using (var scope = base.CreateScope())
            {
                var provider = scope.ServiceProvider;
                var dbContext = provider.GetRequiredService<WeatherAppDbContext>();
                var client = base.CreateClient();
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
                string[] siteNames = { "ACCU", "METEO", "EMY", "WMS" };
                dbContext.Sites.AddRange(siteNames.Select(sitename => new ForecastSite(sitename)));
                await dbContext.SaveChangesAsync();
                int total_sites = dbContext.Sites.Count();

                var request = new SignUpInput()
                {
                    Email = "mmorf@gmail.com",
                    Hashed_password = "werwq",
                    Username = "mmorf",
                    CityNames = new[] { "ATH", "THESS" },
                    SiteNames = new[] { "ACCU", "METEO" }
                };
                var response = await client.PostAsJsonAsync("https://localhost:7038/Authentication/SignUp", request);
                var responseContent = await response.Content.ReadAsStringAsync();

                request = new SignUpInput()
                {
                    Email = "stelios@gmail.com",
                    Hashed_password = "werwq",
                    Username = "stelios",
                    CityNames = new[] { "HERAK", "THESS" },
                    SiteNames = new[] { "EMY", "WMS" }
                };
                response = await client.PostAsJsonAsync("https://localhost:7038/Authentication/SignUp", request);
                responseContent = await response.Content.ReadAsStringAsync();
                var res_users = dbContext.Users.OrderBy(user => user.Email).ToList();
                int i = 0;
                res_users[i].Email.Should().Be("mmorf@gmail.com");
                res_users[i].Hashed_password.Should().Be("werwq");
                res_users[i].Username.Should().Be("mmorf");
                i++;
                res_users[i].Email.Should().Be("stelios@gmail.com");
                res_users[i].Hashed_password.Should().Be("werwq");
                res_users[i].Username.Should().Be("stelios");

                var res_cities = dbContext.Cities.OrderBy(city => city.City_name).ToList();
                i = 0;
                res_cities[i].City_name.Should().Be("ATH");
                i++;
                res_cities[i].City_name.Should().Be("HERAK");
                i++;
                res_cities[i].City_name.Should().Be("THESS");
                i++;
                dbContext.Cities.Count().Should().Be(i);

                var res_sites = dbContext.Sites.OrderBy(site => site.Site_name).ToList();
                i = 0;
                res_sites[i].Site_name.Should().Be("ACCU");
                i++;
                res_sites[i].Site_name.Should().Be("EMY");
                i++;
                res_sites[i].Site_name.Should().Be("METEO");
                i++;
                res_sites[i].Site_name.Should().Be("WMS");
                i++;
                dbContext.Sites.Count().Should().Be(i);


                var citysite_city = dbContext.CitySites.Join(dbContext.Cities, citysite => citysite.City_Id, city => city.City_Id,
                    (citysite, city) => new { citysite.City_Id, citysite.Site_Id, city.City_name });
                var citysite_city_site = citysite_city.Join(dbContext.Sites, citysite_city => citysite_city.Site_Id, site => site.Site_Id,
                    (citysite_city, site) => new { citysite_city.City_Id, citysite_city.Site_Id, citysite_city.City_name, site.Site_name });
                var citysite_results = citysite_city_site.OrderBy(res => res.City_name).ThenBy(res => res.Site_name).
                    Select(res => new { res.City_name, res.Site_name }).ToList();
                i = 0;
                citysite_results[i].City_name.Should().Be("ATH");
                citysite_results[i].Site_name.Should().Be("ACCU");
                i++;
                citysite_results[i].City_name.Should().Be("ATH");
                citysite_results[i].Site_name.Should().Be("METEO");
                i++;
                citysite_results[i].City_name.Should().Be("HERAK");
                citysite_results[i].Site_name.Should().Be("EMY");
                i++;
                citysite_results[i].City_name.Should().Be("HERAK");
                citysite_results[i].Site_name.Should().Be("WMS");
                i++;
                citysite_results[i].City_name.Should().Be("THESS");
                citysite_results[i].Site_name.Should().Be("ACCU");
                i++;
                citysite_results[i].City_name.Should().Be("THESS");
                citysite_results[i].Site_name.Should().Be("EMY");
                i++;
                citysite_results[i].City_name.Should().Be("THESS");
                citysite_results[i].Site_name.Should().Be("METEO");
                i++;
                citysite_results[i].City_name.Should().Be("THESS");
                citysite_results[i].Site_name.Should().Be("WMS");
                i++;
                dbContext.CitySites.Count().Should().Be(i);

                var usercitysite_results = citysite_city_site.Join(dbContext.UserSiteCities,
                    ccs => new { ccs.City_Id, ccs.Site_Id }, usc => new { usc.City_Id, usc.Site_Id },
                    (ccs, usc) => new { usc.Email, ccs.City_name, ccs.Site_name }).
                    OrderBy(res => res.Email).ThenBy(res => res.City_name).ThenBy(res => res.Site_name).ToList();

                i = 0;
                usercitysite_results[i].Email.Should().Be("mmorf@gmail.com");
                usercitysite_results[i].City_name.Should().Be("ATH");
                usercitysite_results[i].Site_name.Should().Be("ACCU");
                i++;
                usercitysite_results[i].Email.Should().Be("mmorf@gmail.com");
                usercitysite_results[i].City_name.Should().Be("ATH");
                usercitysite_results[i].Site_name.Should().Be("METEO");
                i++;
                usercitysite_results[i].Email.Should().Be("mmorf@gmail.com");
                usercitysite_results[i].City_name.Should().Be("THESS");
                usercitysite_results[i].Site_name.Should().Be("ACCU");
                i++;
                usercitysite_results[i].Email.Should().Be("mmorf@gmail.com");
                usercitysite_results[i].City_name.Should().Be("THESS");
                usercitysite_results[i].Site_name.Should().Be("METEO");
                i++;
                usercitysite_results[i].Email.Should().Be("stelios@gmail.com");
                usercitysite_results[i].City_name.Should().Be("HERAK");
                usercitysite_results[i].Site_name.Should().Be("EMY");
                i++;
                usercitysite_results[i].Email.Should().Be("stelios@gmail.com");
                usercitysite_results[i].City_name.Should().Be("HERAK");
                usercitysite_results[i].Site_name.Should().Be("WMS");
                i++;
                usercitysite_results[i].Email.Should().Be("stelios@gmail.com");
                usercitysite_results[i].City_name.Should().Be("THESS");
                usercitysite_results[i].Site_name.Should().Be("EMY");
                i++;
                usercitysite_results[i].Email.Should().Be("stelios@gmail.com");
                usercitysite_results[i].City_name.Should().Be("THESS");
                usercitysite_results[i].Site_name.Should().Be("WMS");
                i++;
                dbContext.UserSiteCities.Count().Should().Be(i);

                await base.emptyDB(dbContext);
            }
        }
        
        
        [Fact]
        public async Task Cities_All_Intersection()
        {
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Trace.WriteLine($"TTTTTTTTT Thread: {Thread.CurrentThread.ManagedThreadId}");
            using (var scope = base.CreateScope())
            {
                var provider = scope.ServiceProvider;
                var dbContext = provider.GetRequiredService<WeatherAppDbContext>();
                var client = base.CreateClient();
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
                string[] siteNames = { "ACCU", "METEO", "EMY", "WMS" };
                dbContext.Sites.AddRange(siteNames.Select(sitename => new ForecastSite(sitename)));
                await dbContext.SaveChangesAsync();
                int total_sites = dbContext.Sites.Count();

                var request = new SignUpInput()
                {
                    Email = "mmorf@gmail.com",
                    Hashed_password = "werwq",
                    Username = "mmorf",
                    CityNames = new[] { "ATH", "THESS" },
                    SiteNames = new[] { "ACCU", "METEO" }
                };
                var response = await client.PostAsJsonAsync("https://localhost:7038/Authentication/SignUp", request);
                var responseContent = await response.Content.ReadAsStringAsync();

                request = new SignUpInput()
                {
                    Email = "stelios@gmail.com",
                    Hashed_password = "werwq",
                    Username = "stelios",
                    CityNames = new[] { "ATH", "THESS" },
                    SiteNames = new[] { "EMY", "WMS" }
                };
                response = await client.PostAsJsonAsync("https://localhost:7038/Authentication/SignUp", request);
                responseContent = await response.Content.ReadAsStringAsync();
                var res_users = dbContext.Users.OrderBy(user => user.Email).ToList();
                int i = 0;
                res_users[i].Email.Should().Be("mmorf@gmail.com");
                res_users[i].Hashed_password.Should().Be("werwq");
                res_users[i].Username.Should().Be("mmorf");
                i++;
                res_users[i].Email.Should().Be("stelios@gmail.com");
                res_users[i].Hashed_password.Should().Be("werwq");
                res_users[i].Username.Should().Be("stelios");

                var res_cities = dbContext.Cities.OrderBy(city => city.City_name).ToList();
                i = 0;
                res_cities[i].City_name.Should().Be("ATH");
                i++;
                res_cities[i].City_name.Should().Be("THESS");
                i++;
                dbContext.Cities.Count().Should().Be(i);

                var res_sites = dbContext.Sites.OrderBy(site => site.Site_name).ToList();
                i = 0;
                res_sites[i].Site_name.Should().Be("ACCU");
                i++;
                res_sites[i].Site_name.Should().Be("EMY");
                i++;
                res_sites[i].Site_name.Should().Be("METEO");
                i++;
                res_sites[i].Site_name.Should().Be("WMS");
                i++;
                dbContext.Sites.Count().Should().Be(i);


                var citysite_city = dbContext.CitySites.Join(dbContext.Cities, citysite => citysite.City_Id, city => city.City_Id,
                    (citysite, city) => new { citysite.City_Id, citysite.Site_Id, city.City_name });
                var citysite_city_site = citysite_city.Join(dbContext.Sites, citysite_city => citysite_city.Site_Id, site => site.Site_Id,
                    (citysite_city, site) => new { citysite_city.City_Id, citysite_city.Site_Id, citysite_city.City_name, site.Site_name });
                var citysite_results = citysite_city_site.OrderBy(res => res.City_name).ThenBy(res => res.Site_name).
                    Select(res => new { res.City_name, res.Site_name }).ToList();
                i = 0;
                string cityname = "ATH";
                citysite_results[i].City_name.Should().Be(cityname);
                citysite_results[i].Site_name.Should().Be("ACCU");
                i++;
                citysite_results[i].City_name.Should().Be(cityname);
                citysite_results[i].Site_name.Should().Be("EMY");
                i++;
                citysite_results[i].City_name.Should().Be(cityname);
                citysite_results[i].Site_name.Should().Be("METEO");
                i++;
                citysite_results[i].City_name.Should().Be(cityname);
                citysite_results[i].Site_name.Should().Be("WMS");
                i++;
                cityname = "THESS";
                citysite_results[i].City_name.Should().Be(cityname);
                citysite_results[i].Site_name.Should().Be("ACCU");
                i++;
                citysite_results[i].City_name.Should().Be(cityname);
                citysite_results[i].Site_name.Should().Be("EMY");
                i++;
                citysite_results[i].City_name.Should().Be(cityname);
                citysite_results[i].Site_name.Should().Be("METEO");
                i++;
                citysite_results[i].City_name.Should().Be(cityname);
                citysite_results[i].Site_name.Should().Be("WMS");
                i++;
                dbContext.CitySites.Count().Should().Be(i);

                var usercitysite_results = citysite_city_site.Join(dbContext.UserSiteCities,
                    ccs => new { ccs.City_Id, ccs.Site_Id }, usc => new { usc.City_Id, usc.Site_Id },
                    (ccs, usc) => new { usc.Email, ccs.City_name, ccs.Site_name }).
                    OrderBy(res => res.Email).ThenBy(res => res.City_name).ThenBy(res => res.Site_name).ToList();

                i = 0;
                usercitysite_results[i].Email.Should().Be("mmorf@gmail.com");
                usercitysite_results[i].City_name.Should().Be("ATH");
                usercitysite_results[i].Site_name.Should().Be("ACCU");
                i++;
                usercitysite_results[i].Email.Should().Be("mmorf@gmail.com");
                usercitysite_results[i].City_name.Should().Be("ATH");
                usercitysite_results[i].Site_name.Should().Be("METEO");
                i++;
                usercitysite_results[i].Email.Should().Be("mmorf@gmail.com");
                usercitysite_results[i].City_name.Should().Be("THESS");
                usercitysite_results[i].Site_name.Should().Be("ACCU");
                i++;
                usercitysite_results[i].Email.Should().Be("mmorf@gmail.com");
                usercitysite_results[i].City_name.Should().Be("THESS");
                usercitysite_results[i].Site_name.Should().Be("METEO");
                i++;
                usercitysite_results[i].Email.Should().Be("stelios@gmail.com");
                usercitysite_results[i].City_name.Should().Be("ATH");
                usercitysite_results[i].Site_name.Should().Be("EMY");
                i++;
                usercitysite_results[i].Email.Should().Be("stelios@gmail.com");
                usercitysite_results[i].City_name.Should().Be("ATH");
                usercitysite_results[i].Site_name.Should().Be("WMS");
                i++;
                usercitysite_results[i].Email.Should().Be("stelios@gmail.com");
                usercitysite_results[i].City_name.Should().Be("THESS");
                usercitysite_results[i].Site_name.Should().Be("EMY");
                i++;
                usercitysite_results[i].Email.Should().Be("stelios@gmail.com");
                usercitysite_results[i].City_name.Should().Be("THESS");
                usercitysite_results[i].Site_name.Should().Be("WMS");
                i++;
                dbContext.UserSiteCities.Count().Should().Be(i);
                await base.emptyDB(dbContext);
            }
        }
        
    }
}
