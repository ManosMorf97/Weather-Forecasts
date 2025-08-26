using Azure;
using Azure.Core;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    public class UserAuthenticationTesting : InMemoryDatabase
    {
        private WeatherForecastFactory? _factory;

        public UserAuthenticationTesting(WeatherForecastFactory _factory) : base(_factory) { }
        [Fact]
        public async Task UserAuthentication()
        {

            using (var scope = base.CreateScope())
            {
                var provider = scope.ServiceProvider;
                var dbContext = provider.GetRequiredService<WeatherAppDbContext>();
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
                string[] siteNames = { "ACCU", "METEO" };
                dbContext.Sites.AddRange(siteNames.Select(sitename => new ForecastSite(sitename)));
                await dbContext.SaveChangesAsync();

                dbContext.Users.Count().Should().Be(0);
                dbContext.Cities.Count().Should().Be(0);
                dbContext.Sites.Count().Should().Be(2);

                List<ForecastSite> sites = dbContext.Sites.OrderBy(site => site.Site_name).ToList();
                sites.Count().Should().Be(2);
                sites[0].Site_name.Should().Be("ACCU");
                sites[1].Site_name.Should().Be("METEO");
                var client = base.CreateClient();


                SignUpInput request = new SignUpInput()
                {
                    Email = "mmorf@gmail.com",
                    Hashed_password = "werwq",
                    Username = "mmorf",
                    CityNames = ["ATH", "THESS"],
                    SiteNames = null
                };
                var response = await client.PostAsJsonAsync("https://localhost:7038/Authentication/SignUp", request);
                var responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
                responseContent.Should().Be("Sites have not been selected");
                dbContext.Cities.Count().Should().Be(0);
                dbContext.Users.Count().Should().Be(0);
                dbContext.CitySites.Count().Should().Be(0);
                dbContext.UserSiteCities.Count().Should().Be(0);
                dbContext.Sites.Count().Should().Be(2);
                request = new SignUpInput()
                {
                    Email = "mmorf@gmail.com",
                    Hashed_password = "werwq",
                    Username = "mmorf",
                    CityNames = null,
                    SiteNames = ["ACCU", "METEO"]
                };
                response = await client.PostAsJsonAsync("https://localhost:7038/Authentication/SignUp", request);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
                responseContent.Should().Be("Cities have not been selected");
                dbContext.Cities.Count().Should().Be(0);
                dbContext.Users.Count().Should().Be(0);
                dbContext.CitySites.Count().Should().Be(0);
                dbContext.UserSiteCities.Count().Should().Be(0);
                dbContext.Sites.Count().Should().Be(2);

                request = new SignUpInput()
                {
                    Email = "mmorf@gmail.com",
                    Hashed_password = "werwq",
                    Username = "mmorf",
                    CityNames = new[] { "ATH", "THESS" },
                    SiteNames = new[] { "ACCU", "METEO" }
                };
                response = await client.PostAsJsonAsync("https://localhost:7038/Authentication/SignUp", request);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
                responseContent.Should().Be("The User has been created");

                List<User> users = dbContext.Users.ToList();
                users.Count().Should().Be(1);
                users[0].Email.Should().Be("mmorf@gmail.com");
                users[0].Username.Should().Be("mmorf");

                List<City> cities = dbContext.Cities.OrderBy(city => city.City_name).ToList();
                cities.Count().Should().Be(2);
                cities[0].City_name.Should().Be("ATH");
                cities[1].City_name.Should().Be("THESS");

                List<ForecastSite> sites_after = dbContext.Sites.OrderBy(site => site.Site_name).ToList();
                sites_after.Count().Should().Be(2);
                sites_after[0].Site_name.Should().Be("ACCU");
                sites_after[1].Site_name.Should().Be("METEO");

                request = new SignUpInput()
                {
                    Email = "mmorf@gmail.com",
                    Hashed_password = "werwq",
                    Username = "mmorf",
                    CityNames = new[] { "ATH", "THESS" },
                    SiteNames = new[] { "ACCU", "METEO" }
                };
                response = await client.PostAsJsonAsync("https://localhost:7038/Authentication/SignUp", request);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
                responseContent.Should().Be("User exists");

                request = new SignUpInput()
                {
                    Email = "mmorf@gmail.com",
                    Hashed_password = "werwq",
                    Username = "mmoarf",
                    CityNames = new[] { "ATH", "THESS" },
                    SiteNames = new[] { "ACCU", "METEO" }
                };
                response = await client.PostAsJsonAsync("https://localhost:7038/Authentication/SignUp", request);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
                responseContent.Should().Be("User exists");

                request = new SignUpInput()
                {
                    Email = "mmo1rf@gmail.com",
                    Hashed_password = "werwq",
                    Username = "mmorf",
                    CityNames = new[] { "ATH", "THESS" },
                    SiteNames = new[] { "ACCU", "METEO" }
                };
                response = await client.PostAsJsonAsync("https://localhost:7038/Authentication/SignUp", request);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
                responseContent.Should().Be("Username is taken");
                users.Count().Should().Be(1);

                request = new SignUpInput()
                {
                    Email = "mmoarf@gmail.com",
                    Hashed_password = "werw1q",
                    Username = "mmoarf",
                    CityNames = new[] { "ATH", "THESS" },
                    SiteNames = new[] { "ACCU", "METEO" }
                };
                response = await client.PostAsJsonAsync("https://localhost:7038/Authentication/SignUp", request);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
                responseContent.Should().Be("The User has been created");

                users = dbContext.Users.OrderBy(user=>user.Email).ToList();
                users.Count().Should().Be(2);
                users[0].Email.Should().Be("mmoarf@gmail.com");
                users[0].Username.Should().Be("mmoarf");
                users[1].Email.Should().Be("mmorf@gmail.com");
                users[1].Username.Should().Be("mmorf");

                request = new SignUpInput()
                {
                    Email = "",
                    Hashed_password = "werwq",
                    Username = "mmoarf",
                    CityNames = new[] { "ATH", "THESS" },
                    SiteNames = new[] { "ACCU", "METEO" }
                };
                response = await client.PostAsJsonAsync("https://localhost:7038/Authentication/SignUp", request);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
                responseContent.Should().Be("Non valid Email");

                users = dbContext.Users.ToList();
                users.Count().Should().Be(2);

                request = new SignUpInput()
                {
                    Email = "ammmoorff",
                    Hashed_password = "werwq",
                    Username = "mmoarf",
                    CityNames = new[] { "ATH", "THESS" },
                    SiteNames = new[] { "ACCU", "METEO" }
                };
                response = await client.PostAsJsonAsync("https://localhost:7038/Authentication/SignUp", request);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
                responseContent.Should().Be("Non valid Email");

                users = dbContext.Users.ToList();
                users.Count().Should().Be(2);

                request = new SignUpInput()
                {
                    Email = "mimorf@gmail.com",
                    Hashed_password = "werwq",
                    Username = "",
                    CityNames = new[] { "ATH", "THESS" },
                    SiteNames = new[] { "ACCU", "METEO" }
                };


                response = await client.PostAsJsonAsync("https://localhost:7038/Authentication/SignUp", request);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
                responseContent.Should().Be("Username is empty");

                users = dbContext.Users.ToList();
                users.Count().Should().Be(2);

                request = new SignUpInput()
                {
                    Email = "mimorf@gmail.com",
                    Hashed_password = "wewq",
                    Username = "mimorf",
                    CityNames = new[] { "ATH", "THESS" },
                    SiteNames = new[] { "ACCU", "METEO" }
                };
                response = await client.PostAsJsonAsync("https://localhost:7038/Authentication/SignUp", request);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotAcceptable);
                responseContent.Should().Be("Password is less than 5 characters");

                users = dbContext.Users.ToList();
                users.Count().Should().Be(2);
                await base.emptyDB(dbContext);
            }
            
        }

        [Fact]
        public async Task CitiesSites_No_Intersection()
        {
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
                    CityNames = new[] { "HERAK", "PATRA" },
                    SiteNames = new[] { "EMY", "WMS" }
                };
                 response = await client.PostAsJsonAsync("https://localhost:7038/Authentication/SignUp", request);
                 responseContent = await response.Content.ReadAsStringAsync();
                 var res_users=dbContext.Users.OrderBy(user=>user.Email).ToList();
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
                res_cities[i].City_name.Should().Be("PATRA");
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
                var citysite_results = citysite_city_site.OrderBy(res=>res.City_name).ThenBy(res=>res.Site_name).
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
                citysite_results[i].City_name.Should().Be("PATRA");
                citysite_results[i].Site_name.Should().Be("EMY");
                i++;
                citysite_results[i].City_name.Should().Be("PATRA");
                citysite_results[i].Site_name.Should().Be("WMS");
                i++;
                citysite_results[i].City_name.Should().Be("THESS");
                citysite_results[i].Site_name.Should().Be("ACCU");
                i++;
                citysite_results[i].City_name.Should().Be("THESS");
                citysite_results[i].Site_name.Should().Be("METEO");
                i++;
                dbContext.CitySites.Count().Should().Be(i);

                var usercitysite_results = citysite_city_site.Join(dbContext.UserSiteCities,
                    ccs => new { ccs.City_Id, ccs.Site_Id }, usc => new { usc.City_Id, usc.Site_Id },
                    (ccs, usc) => new { usc.Email, ccs.City_name, ccs.Site_name }).
                    OrderBy(res=>res.Email).ThenBy(res=>res.City_name).ThenBy(res=>res.Site_name).ToList();

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
                usercitysite_results[i].City_name.Should().Be("PATRA");
                usercitysite_results[i].Site_name.Should().Be("EMY");
                i++;
                usercitysite_results[i].Email.Should().Be("stelios@gmail.com");
                usercitysite_results[i].City_name.Should().Be("PATRA");
                usercitysite_results[i].Site_name.Should().Be("WMS");
                i++;
                dbContext.UserSiteCities.Count().Should().Be(i);
                await base.emptyDB(dbContext);
            }
        }
        [Fact]
        public async Task CitiesSites_Some_Intersection()
        {
            using (var scope = base.CreateScope())
            {
                var provider = scope.ServiceProvider;
                var dbContext = provider.GetRequiredService<WeatherAppDbContext>();
                var client = base.CreateClient();
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
                string[] siteNames = { "ACCU", "METEO", "EMY" };
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
                    SiteNames = new[] { "EMY", "METEO" }
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
                citysite_results[i].Site_name.Should().Be("METEO");
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
                usercitysite_results[i].Site_name.Should().Be("METEO");
                i++;
                usercitysite_results[i].Email.Should().Be("stelios@gmail.com");
                usercitysite_results[i].City_name.Should().Be("THESS");
                usercitysite_results[i].Site_name.Should().Be("EMY");
                i++;
                usercitysite_results[i].Email.Should().Be("stelios@gmail.com");
                usercitysite_results[i].City_name.Should().Be("THESS");
                usercitysite_results[i].Site_name.Should().Be("METEO");
                i++;
                dbContext.UserSiteCities.Count().Should().Be(i);
                await base.emptyDB(dbContext);
            }
        }
        [Fact]
        public async Task CitySite_All_Intersection()
        {
            using (var scope = base.CreateScope())
            {
                var provider = scope.ServiceProvider;
                var dbContext = provider.GetRequiredService<WeatherAppDbContext>();
                var client = base.CreateClient();
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
                string[] siteNames = { "ACCU", "METEO" };
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
                    SiteNames = new[] { "ACCU", "METEO" }
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
                res_sites[i].Site_name.Should().Be("METEO");
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
                citysite_results[i].City_name.Should().Be("THESS");
                citysite_results[i].Site_name.Should().Be("ACCU");
                i++;
                citysite_results[i].City_name.Should().Be("THESS");
                citysite_results[i].Site_name.Should().Be("METEO");
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
                usercitysite_results[i].Site_name.Should().Be("ACCU");
                i++;
                usercitysite_results[i].Email.Should().Be("stelios@gmail.com");
                usercitysite_results[i].City_name.Should().Be("ATH");
                usercitysite_results[i].Site_name.Should().Be("METEO");
                i++;
                usercitysite_results[i].Email.Should().Be("stelios@gmail.com");
                usercitysite_results[i].City_name.Should().Be("THESS");
                usercitysite_results[i].Site_name.Should().Be("ACCU");
                i++;
                usercitysite_results[i].Email.Should().Be("stelios@gmail.com");
                usercitysite_results[i].City_name.Should().Be("THESS");
                usercitysite_results[i].Site_name.Should().Be("METEO");
                i++;
                dbContext.UserSiteCities.Count().Should().Be(i);
                await base.emptyDB(dbContext);
            }
        }
        [Fact]
        public async Task SignInTesting()
        {
            using (var scope = base.CreateScope())
            {
                var provider = scope.ServiceProvider;
                var dbContext = provider.GetRequiredService<WeatherAppDbContext>();
                var client = base.CreateClient();
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
                string[] siteNames = { "ACCU", "METEO" };
                dbContext.Sites.AddRange(siteNames.Select(sitename => new ForecastSite(sitename)));
                await dbContext.SaveChangesAsync();
                var requestSignUp = new SignUpInput
                {
                    Email = "manosmorf97@gmail.com",
                    Hashed_password = "petraw",
                    Username = "manolis",
                    CityNames = ["ATH", "THESS"],
                    SiteNames = ["ACCU", "METEO"]

                };

                var response = await client.PostAsJsonAsync("http://localhost:7038/Authentication/SignUp", requestSignUp);
                var responseContent = await response.Content.ReadAsStringAsync();

                var requestSignIn = new SignInInput
                {
                    Email_Username = "",
                    HashedPassword = ""
                };
                response = await client.PostAsJsonAsync("http://localhost:7038/Authentication/SignIn", requestSignIn);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
                responseContent.Should().Be("Some fields are empty");

                requestSignIn = new SignInInput
                {
                    Email_Username = "",
                    HashedPassword = "ppoewq"
                };
                response = await client.PostAsJsonAsync("http://localhost:7038/Authentication/SignIn", requestSignIn);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
                responseContent.Should().Be("Some fields are empty");

                requestSignIn = new SignInInput
                {
                    Email_Username = "mmorf",
                    HashedPassword = ""
                };
                response = await client.PostAsJsonAsync("http://localhost:7038/Authentication/SignIn", requestSignIn);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
                responseContent.Should().Be("Some fields are empty");
                requestSignIn = new SignInInput
                {
                    Email_Username = "mmorf@gmail.com",
                    HashedPassword = ""
                };
                response = await client.PostAsJsonAsync("http://localhost:7038/Authentication/SignIn", requestSignIn);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
                responseContent.Should().Be("Some fields are empty");

                requestSignIn = new SignInInput
                {
                    Email_Username = "manosmorf97@gmail.com",
                    HashedPassword = "bbpposdasb"
                };
                response = await client.PostAsJsonAsync("http://localhost:7038/Authentication/SignIn", requestSignIn);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
                responseContent.Should().Be("Wrong password");

                requestSignIn = new SignInInput
                {
                    Email_Username = "manolis",
                    HashedPassword = "bbpposdasb"
                };
                response = await client.PostAsJsonAsync("http://localhost:7038/Authentication/SignIn", requestSignIn);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
                responseContent.Should().Be("Wrong password");
                requestSignIn = new SignInInput
                {
                    Email_Username = "manosmorf92@gmail.com",
                    HashedPassword = "bbpposdasb"
                };
                response = await client.PostAsJsonAsync("http://localhost:7038/Authentication/SignIn", requestSignIn);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
                responseContent.Should().Be("This Email/Username does not respond to a specific user");

                requestSignIn = new SignInInput
                {
                    Email_Username = "manos92",
                    HashedPassword = "bbpposdasb"
                };
                response = await client.PostAsJsonAsync("http://localhost:7038/Authentication/SignIn", requestSignIn);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
                responseContent.Should().Be("This Email/Username does not respond to a specific user");

                requestSignIn = new SignInInput
                {
                    Email_Username = "manosmorf97@gmail.com",
                    HashedPassword = "petraw"
                };
                response = await client.PostAsJsonAsync("http://localhost:7038/Authentication/SignIn", requestSignIn);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
                responseContent.Should().Be("Logged in succesfully");

                requestSignIn = new SignInInput
                {
                    Email_Username = "manolis",
                    HashedPassword = "petraw"
                };
                response = await client.PostAsJsonAsync("http://localhost:7038/Authentication/SignIn", requestSignIn);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
                responseContent.Should().Be("Logged in succesfully");
                await base.emptyDB(dbContext);
            }
        }

        
    }
}
