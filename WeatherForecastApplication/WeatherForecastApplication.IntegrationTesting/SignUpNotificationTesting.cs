using FluentAssertions;
using Lucene.Net.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using WeatherForecastApplication.Data;
using WeatherForecastApplication.Web.Data;
using WeatherForecastApplication.Web.Models;
using static System.Formats.Asn1.AsnWriter;

namespace WeatherForecastApplication.IntegrationTesting
{
    public class SignUpNotificationTesting : InMemoryDatabase
    {
        public SignUpNotificationTesting(WeatherForecastFactory _factory) : base(_factory) { }

        [Fact]
        public async Task TestUserNotifications()
        {
            using (var scope = base.CreateScope())
            {
                var scopeProvider = scope.ServiceProvider;
                var dbContext = scopeProvider.GetRequiredService<WeatherAppDbContext>();
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
                string[] siteNames = { "ACCU", "EMY", "METEO" };
                dbContext.Sites.AddRange(siteNames.Select(sitename => new ForecastSite(sitename)));
                await dbContext.SaveChangesAsync();
                string[] cityNames = { "ATH", "HERAK", "KOMOT", "THESS" };
                dbContext.Cities.AddRange(cityNames.Select(cityname => new City(cityname)));
                await dbContext.SaveChangesAsync();
                HashMap<string, int> cities_data = new HashMap<string, int>();
                foreach (string cityName in cityNames)
                {
                    var city_rec = await dbContext.Cities.FirstOrDefaultAsync(city => city.City_name.Equals(cityName));
                    cities_data.Add(cityName, city_rec.City_Id);
                }
                HashMap<string, int> sites_data = new HashMap<string, int>();
                foreach (string siteName in siteNames)
                {
                    var site_rec = await dbContext.Sites.FirstOrDefaultAsync(site => site.Site_name.Equals(siteName));
                    sites_data.Add(siteName, site_rec.Site_Id);
                }
                CitySite[] citysites = new CitySite[12];
                int i = 0;
                foreach (string siteName in siteNames)
                    foreach (string cityName in cityNames) {
                        citysites[i] = new CitySite(cities_data[cityName], sites_data[siteName]);
                        i++;
                    }
                dbContext.CitySites.AddRange(citysites);
                await dbContext.SaveChangesAsync();
                Timeslot [] timeslots = {new Timeslot { Date = new DateOnly(2025,1,21), Time= new TimeOnly(15,5,5) },
                    new Timeslot { Date = new DateOnly(2025,11,23), Time= new TimeOnly(20,0,0) } };
                dbContext.Timeslots.AddRange(timeslots);
                await dbContext.SaveChangesAsync();
                List<Prediction> predictions=new List<Prediction>();
                List<Notification> notifications = new List<Notification>();
                foreach (string siteName in siteNames)
                    foreach (string cityName in cityNames)
                        for(int j=0; j<2; j++) {
                            bool danger = false;
                            if ((cityName.Equals("ATH") && siteName.Equals("ACCU") && j == 0)||
                                (cityName.Equals("KOMOT") && siteName.Equals("ACCU") && j == 0)||
                                (cityName.Equals("THESS") && siteName.Equals("METEO") && j == 1)||
                                (cityName.Equals("HERAK") && siteName.Equals("EMY") && j == 1)||
                                (cityName.Equals("ATH") && siteName.Equals("EMY") && j == 1))
                                danger = true;
                            predictions.Add(new Prediction
                            {
                                Site_Id = sites_data[siteName],
                                City_Id = cities_data[cityName],
                                Timeslot_Id = timeslots[j].Timeslot_Id,
                                Danger=danger,
                                Weather="RANDOM"
                            });
                            if (danger)
                                notifications.Add(new Notification
                                {
                                    Site_Id = sites_data[siteName],
                                    City_Id = cities_data[cityName],
                                    Timeslot_Id = timeslots[j].Timeslot_Id,
                                    Description = siteName+" "+cityName+ "Danger",
                                    DateNotification = TimeSetting.getDate(),
                                    TimeNotification=TimeSetting.getTime()
                                });
                        }
                dbContext.Predictions.AddRange(predictions);
                await dbContext.SaveChangesAsync();
                dbContext.Notifications.AddRange(notifications);
                await dbContext.SaveChangesAsync();
                TimeSetting.ChangeTimer(new DateOnly(2025, 1, 1), new TimeOnly(14, 0, 0));

                var client = base.CreateClient();
                var request = new SignUpInput
                {
                    Email = "mmorf@gmail.com",
                    Hashed_password = "werwq",
                    Username = "mmorf",
                    CityNames = new[] { "ATH", "THESS" },
                    SiteNames = new[] { "ACCU", "METEO" }
                };
                var response = await client.PostAsJsonAsync("https://localhost:7038/Authentication/SignUp", request);

                request = new SignUpInput
                {
                    Email = "smart@gmail.com",
                    Hashed_password = "wqqterd",
                    Username = "smart",
                    CityNames = new[] { "HERAK", "THESS" },
                    SiteNames = new[] { "EMY", "METEO" }
                };

                response = await client.PostAsJsonAsync("https://localhost:7038/Authentication/SignUp", request);

                var response_m = await client.PostAsJsonAsync("https://localhost:7038/Notifications", "mmorf@gmail.com");
                var responseContent_m = await response_m.Content.ReadAsStringAsync();
                responseContent_m = responseContent_m.Substring(2, responseContent_m.Length - 4);
                string[] test_notifications_m = responseContent_m.Split("},{");
                string[] m_sites = { "ACCU", "METEO" };
                string[] m_cities = { "ATH", "THESS" };
                string[] m_dates = { "2025-01-21", "2025-11-23" };
                string[] m_times = { "15:05:05", "20:00:00" };
                for (int j = 0; j < 2; j++)
                {
                    test_notifications_m[j].Should().Contain("\"description\":\"" + m_sites[j] +" " + m_cities[j] + "Danger");
                    test_notifications_m[j].Should().Contain("\"city_name\":\"" + m_cities[j]);
                    test_notifications_m[j].Should().Contain("\"site_name\":\"" + m_sites[j]);
                    test_notifications_m[j].Should().Contain("\"date\":\"" + m_dates[j]);
                    test_notifications_m[j].Should().Contain("\"time\":\"" + m_times[j]);
                }
                test_notifications_m.Count().Should().Be(2);
                var response_s = await client.PostAsJsonAsync("https://localhost:7038/Notifications", "smart@gmail.com");
                var responseContent_s = await response_s.Content.ReadAsStringAsync();
                responseContent_s = responseContent_s.Substring(2, responseContent_s.Length - 4);
                string[] test_notifications_s = responseContent_s.Split("},{");
                string[] s_sites = { "EMY", "METEO" };
                string[] s_cities = { "HERAK", "THESS" };
                string s_date =  "2025-11-23" ;
                string s_time = "20:00:00";
                for (int j = 0; j < 2; j++)
                {
                    test_notifications_s[j].Should().Contain("\"description\":\"" + s_sites[j] + " " + s_cities[j] + "Danger");
                    test_notifications_s[j].Should().Contain("\"city_name\":\"" + s_cities[j]);
                    test_notifications_s[j].Should().Contain("\"site_name\":\"" + s_sites[j]);
                    test_notifications_s[j].Should().Contain("\"date\":\"" + s_date);
                    test_notifications_s[j].Should().Contain("\"time\":\"" + s_time);
                }
                test_notifications_s.Count().Should().Be(2);
            }
        }
        [Fact]
        public async Task TestUserCurrentNotifications()
        {
            using (var scope = base.CreateScope())
            {
                var scopeProvider = scope.ServiceProvider;
                var dbContext = scopeProvider.GetRequiredService<WeatherAppDbContext>();
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
                string[] siteNames = { "ACCU", "EMY", "METEO" };
                dbContext.Sites.AddRange(siteNames.Select(sitename => new ForecastSite(sitename)));
                await dbContext.SaveChangesAsync();
                string[] cityNames = { "ATH", "HERAK", "THESS" };
                dbContext.Cities.AddRange(cityNames.Select(cityname => new City(cityname)));
                await dbContext.SaveChangesAsync();
                HashMap<string, int> cities_data = new HashMap<string, int>();
                foreach (string cityName in cityNames)
                {
                    var city_rec = await dbContext.Cities.FirstOrDefaultAsync(city => city.City_name.Equals(cityName));
                    cities_data.Add(cityName, city_rec.City_Id);
                }
                HashMap<string, int> sites_data = new HashMap<string, int>();
                foreach (string siteName in siteNames)
                {
                    var site_rec = await dbContext.Sites.FirstOrDefaultAsync(site => site.Site_name.Equals(siteName));
                    sites_data.Add(siteName, site_rec.Site_Id);
                }
                CitySite[] citysites = new CitySite[9];
                int i = 0;
                foreach (string siteName in siteNames)
                    foreach (string cityName in cityNames)
                    {
                        citysites[i] = new CitySite(cities_data[cityName], sites_data[siteName]);
                        i++;
                    }
                dbContext.CitySites.AddRange(citysites);
                await dbContext.SaveChangesAsync();
                Timeslot[] timeslots = {new Timeslot { Date = new DateOnly(2025,1,21), Time= new TimeOnly(15,5,5) },
                    new Timeslot { Date = new DateOnly(204,12,12), Time= new TimeOnly(20,0,0) } };
                dbContext.Timeslots.AddRange(timeslots);
                await dbContext.SaveChangesAsync();
                List<Prediction> predictions = new List<Prediction>();
                List<Notification> notifications = new List<Notification>();
                foreach (string siteName in siteNames)
                    foreach (string cityName in cityNames)
                        for (int j = 0; j < 2; j++)
                        {
                            bool danger = false;
                            if ((cityName.Equals("ATH") && siteName.Equals("ACCU") && j == 0)||
                                (cityName.Equals("THESS") && siteName.Equals("METEO") && j == 1) ||
                                (cityName.Equals("HERAK") && siteName.Equals("EMY") && j == 0))
                                danger = true;
                            predictions.Add(new Prediction
                            {
                                Site_Id = sites_data[siteName],
                                City_Id = cities_data[cityName],
                                Timeslot_Id = timeslots[j].Timeslot_Id,
                                Danger = danger,
                                Weather = "RANDOM"
                            });
                            if (danger)
                                notifications.Add(new Notification
                                {
                                    Site_Id = sites_data[siteName],
                                    City_Id = cities_data[cityName],
                                    Timeslot_Id = timeslots[j].Timeslot_Id,
                                    Description = siteName + " " + cityName + "Danger",
                                    DateNotification = TimeSetting.getDate(),
                                    TimeNotification = TimeSetting.getTime()
                                });
                        }
                dbContext.Predictions.AddRange(predictions);
                await dbContext.SaveChangesAsync();
                dbContext.Notifications.AddRange(notifications);
                await dbContext.SaveChangesAsync();
                TimeSetting.ChangeTimer(new DateOnly(2025, 1, 1), new TimeOnly(14, 0, 0));

                var client = base.CreateClient();
                var request = new SignUpInput
                {
                    Email = "mmorf@gmail.com",
                    Hashed_password = "werwq",
                    Username = "mmorf",
                    CityNames = new[] { "ATH", "THESS" },
                    SiteNames = new[] { "ACCU", "METEO" }
                };
                var response = await client.PostAsJsonAsync("https://localhost:7038/Authentication/SignUp", request);

                request = new SignUpInput
                {
                    Email = "smart@gmail.com",
                    Hashed_password = "wqqterd",
                    Username = "smart",
                    CityNames = new[] { "HERAK", "THESS" },
                    SiteNames = new[] { "EMY", "METEO" }
                };

                response = await client.PostAsJsonAsync("https://localhost:7038/Authentication/SignUp", request);

                var response_m = await client.PostAsJsonAsync("https://localhost:7038/Notifications", "mmorf@gmail.com");
                var responseContent_m = await response_m.Content.ReadAsStringAsync();
                responseContent_m = responseContent_m.Substring(2, responseContent_m.Length - 4);
                string[] test_notifications_m = responseContent_m.Split("},{");
                string m_site =  "ACCU";
                string m_city = "ATH";
                string m_date = "2025-01-21";
                string m_time = "15:05:05";
      
                test_notifications_m[0].Should().Contain("\"description\":\"" + m_site + " " + m_city + "Danger");
                test_notifications_m[0].Should().Contain("\"city_name\":\"" + m_city);
                test_notifications_m[0].Should().Contain("\"site_name\":\"" + m_site);
                test_notifications_m[0].Should().Contain("\"date\":\"" + m_date);
                test_notifications_m[0].Should().Contain("\"time\":\"" + m_time);
                
                test_notifications_m.Count().Should().Be(1);
                var response_s = await client.PostAsJsonAsync("https://localhost:7038/Notifications", "smart@gmail.com");
                var responseContent_s = await response_s.Content.ReadAsStringAsync();
                responseContent_s = responseContent_s.Substring(2, responseContent_s.Length - 4);
                string[] test_notifications_s = responseContent_s.Split("},{");
                string s_site =  "EMY";
                string s_city =  "HERAK";
                string s_date = "2025-01-21";
                string s_time = "15:05:05";

                test_notifications_s[0].Should().Contain("\"description\":\"" + s_site + " " + s_city + "Danger");
                test_notifications_s[0].Should().Contain("\"city_name\":\"" + s_city);
                test_notifications_s[0].Should().Contain("\"site_name\":\"" + s_site);
                test_notifications_s[0].Should().Contain("\"date\":\"" + s_date);
                test_notifications_s[0].Should().Contain("\"time\":\"" + s_time);
                test_notifications_s.Count().Should().Be(1);
            }
        }
    }
}
