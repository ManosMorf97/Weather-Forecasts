using FluentAssertions;
using Lucene.Net.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
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

    public class NotificationsTesting:InMemoryDatabase
    {
        public NotificationsTesting(WeatherForecastFactory _factory) : base(_factory) { }

        [Fact]
        public async Task TestNotifications() {

            using (var scope = CreateScope())
            {
                var client = CreateClient();
                var scopeProvider = scope.ServiceProvider;
                var dbContext = scopeProvider.GetRequiredService<WeatherAppDbContext>();
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
                string[] site_names = { "ACCU", "EMY", "METEO" };
                dbContext.Sites.AddRange(site_names.Select(sitename => new ForecastSite(sitename)));
                await dbContext.SaveChangesAsync();
                var manos = new SignUpInput
                {
                    Email = "mmorf@gmail.com",
                    Hashed_password = "pperwa",
                    Username = "manos",
                    CityNames = new[] { "ATH", "THESS" },
                    SiteNames = new[] { "ACCU", "METEO" }
                };
                var stelios = new SignUpInput
                {
                    Email = "stelios@gmail.com",
                    Hashed_password = "ppe2rwa",
                    Username = "stelios",
                    CityNames = new[] { "THESS", "HERAK" },
                    SiteNames = new[] { "EMY", "METEO" }
                };
                await client.PostAsJsonAsync("https://localhost:7038/Authentication/SignUp", manos);
                await client.PostAsJsonAsync("https://localhost:7038/Authentication/SignUp", stelios);

                Timeslot[] testTimeslots = { new Timeslot { Date = new DateOnly(2025,1,21), Time= new TimeOnly(15,5,5) },
            new Timeslot { Date = new DateOnly(2025,2,23), Time= new TimeOnly(20,0,0) }};
                dbContext.Timeslots.AddRange(testTimeslots);
                await dbContext.SaveChangesAsync();
                HashMap<string, int> cities_data = new HashMap<string, int>();
                string[] city_names = { "ATH", "THESS", "HERAK" };
                foreach (string city_name in city_names)
                {
                    var city_rec = await dbContext.Cities.FirstOrDefaultAsync(city => city.City_name.Equals(city_name));
                    cities_data.Add(city_name, city_rec.City_Id);
                }
                HashMap<string, int> sites_data = new HashMap<string, int>();
                site_names = ["ACCU", "METEO", "EMY"];
                foreach (string site_name in site_names)
                {
                    var site_rec = await dbContext.Sites.FirstOrDefaultAsync(site => site.Site_name.Equals(site_name));
                    sites_data.Add(site_name, site_rec.Site_Id);
                }
                List<Prediction> predictions = new List<Prediction>();
                int[] timeslots_ids = dbContext.Timeslots.OrderBy(ts => ts.Date).Select(timeslot => timeslot.Timeslot_Id).ToArray();
                int[] city_ids = { cities_data["ATH"], cities_data["THESS"], cities_data["HERAK"] };
                int[] sites_ids = { sites_data["ACCU"], sites_data["METEO"], sites_data["EMY"] };
                CitySite[] helpCS = {new CitySite(){City_Id=cities_data["ATH"],Site_Id=sites_data["EMY"] },
                            new CitySite(){City_Id=cities_data["HERAK"],Site_Id=sites_data["ACCU"]}};
                dbContext.CitySites.AddRange(helpCS);
                await dbContext.SaveChangesAsync();
                foreach (int site_id in sites_ids)
                {
                    foreach (int city_id in city_ids)
                    {
                        foreach (int timeslot_id in timeslots_ids)
                        {
                            Prediction prediction = new Prediction
                            {
                                City_Id = city_id,
                                Site_Id = site_id,
                                Timeslot_Id = timeslot_id,
                                Danger = false
                            };
                            if (prediction.Site_Id == sites_data["ACCU"] && prediction.City_Id == cities_data["ATH"]
                                && prediction.Timeslot_Id == timeslots_ids[0])
                                prediction.Danger = true;
                            if (prediction.Site_Id == sites_data["METEO"] && prediction.City_Id == cities_data["THESS"]
                                && prediction.Timeslot_Id == timeslots_ids[1])
                                prediction.Danger = true;
                            predictions.Add(prediction);
                        }
                    }
                }
                string[] weather = { "10C", "8C", "9C", "7C", "11C", "9C",//ACCU
                                "9C", "7C", "8C", "6C", "10C", "8C",//METEO
                                "11C", "9C", "10C", "8C", "12C", "10C",};//EMY
                for (int i = 0; i < predictions.Count(); i++)
                {
                    predictions[i].Weather = weather[i];
                }
                dbContext.Predictions.AddRange(predictions);
                await dbContext.SaveChangesAsync();

                dbContext.Notifications.AddRange([
                    new Notification { City_Id = cities_data["ATH"],
                    Site_Id = sites_data["ACCU"], Timeslot_Id = timeslots_ids[0],
                    DateNotification = new DateOnly(2025,3,3), TimeNotification = new TimeOnly(15, 5, 5),Description="Athens is on fire" },
                    new Notification { City_Id = cities_data["THESS"],
                    Site_Id = sites_data["METEO"], Timeslot_Id = timeslots_ids[0],
                    DateNotification = new DateOnly(2025,3,3), TimeNotification = new TimeOnly(16, 30, 0),Description="Thessaloniki is on fire" },
                    new Notification { City_Id = cities_data["HERAK"],
                    Site_Id = sites_data["EMY"], Timeslot_Id = timeslots_ids[1],
                    DateNotification = new DateOnly(2025,4,4), TimeNotification = new TimeOnly(15, 5, 0),Description="Heraklion is on fire" },

                ]);
                await dbContext.SaveChangesAsync();
                dbContext.UserNotifications.AddRange([
                    new UserNotification{Email="mmorf@gmail.com",City_Id = cities_data["ATH"],
                    Site_Id = sites_data["ACCU"], Timeslot_Id = timeslots_ids[0],IsRead=false},
                    new UserNotification{Email="mmorf@gmail.com", City_Id = cities_data["THESS"],
                    Site_Id = sites_data["METEO"], Timeslot_Id = timeslots_ids[0],IsRead=false},
                    new UserNotification{Email="stelios@gmail.com", City_Id = cities_data["THESS"],
                    Site_Id = sites_data["METEO"], Timeslot_Id = timeslots_ids[0],IsRead=false},
                    new UserNotification{Email="stelios@gmail.com", City_Id = cities_data["HERAK"],
                    Site_Id = sites_data["EMY"], Timeslot_Id = timeslots_ids[1],IsRead=false}
                    ]);
                await dbContext.SaveChangesAsync();

                var response_m = await client.PostAsJsonAsync("https://localhost:7038/Notifications","mmorf@gmail.com");
                var responseContent_m =await response_m.Content.ReadAsStringAsync();
                responseContent_m = responseContent_m.Substring(2, responseContent_m.Length - 4);
                string[] test_notifications_m = responseContent_m.Split("},{");
                test_notifications_m[0].Should().Contain("\"description\":\"Athens is on fire\"");
                test_notifications_m[0].Should().Contain("\"city_name\":\"ATH\"");
                test_notifications_m[0].Should().Contain("\"site_name\":\"ACCU\"");
                test_notifications_m[0].Should().Contain("\"date\":\"2025-01-21\"");
                test_notifications_m[0].Should().Contain("\"time\":\"15:05:05\"");
                test_notifications_m[1].Should().Contain("\"description\":\"Thessaloniki is on fire\"");
                test_notifications_m[1].Should().Contain("\"city_name\":\"THESS\"");
                test_notifications_m[1].Should().Contain("\"site_name\":\"METEO\"");
                test_notifications_m[1].Should().Contain("\"date\":\"2025-01-21\"");
                test_notifications_m[1].Should().Contain("\"time\":\"15:05:05\"");
                response_m = await client.PostAsJsonAsync("https://localhost:7038/Notifications/GetCountNotifications"
                    , "mmorf@gmail.com");
                int countResponse_m = Int32.Parse(await response_m.Content.ReadAsStringAsync());
                countResponse_m.Should().Be(2);

                var response_s = await client.PostAsJsonAsync("https://localhost:7038/Notifications", "stelios@gmail.com");
                var responseContent_s = await response_s.Content.ReadAsStringAsync();
                responseContent_s = responseContent_s.Substring(2, responseContent_s.Length - 4);
                string[] test_notifications_s = responseContent_s.Split("},{");
                test_notifications_s[0].Should().Contain("\"description\":\"Thessaloniki is on fire\"");
                test_notifications_s[0].Should().Contain("\"city_name\":\"THESS\"");
                test_notifications_s[0].Should().Contain("\"site_name\":\"METEO\"");
                test_notifications_s[0].Should().Contain("\"date\":\"2025-01-21\"");
                test_notifications_s[0].Should().Contain("\"time\":\"15:05:05\"");
                test_notifications_s[1].Should().Contain("\"description\":\"Heraklion is on fire\"");
                test_notifications_s[1].Should().Contain("\"city_name\":\"HERAK\"");
                test_notifications_s[1].Should().Contain("\"site_name\":\"EMY\"");
                test_notifications_s[1].Should().Contain("\"date\":\"2025-02-23\"");
                test_notifications_s[1].Should().Contain("\"time\":\"20:00:00\"");
                response_s = await client.PostAsJsonAsync("https://localhost:7038/Notifications/GetCountNotifications"
                    , "stelios@gmail.com");
                int countResponse_s = Int32.Parse(await response_s.Content.ReadAsStringAsync());
                countResponse_s.Should().Be(2);

                await client.PostAsJsonAsync("https://localhost:7038/Notifications/ReadNotifications", "mmorf@gmail.com");

                response_m = await client.PostAsJsonAsync("https://localhost:7038/Notifications", "mmorf@gmail.com");
                responseContent_m = await response_m.Content.ReadAsStringAsync();
                test_notifications_m = responseContent_m.Split("},{");
                Trace.WriteLine("XASXASgfYHFHVGHV" + test_notifications_m[0]);
                test_notifications_m.Length.Should().Be(1);
                test_notifications_m[0].Should().Be("[]");
                response_m = await client.PostAsJsonAsync("https://localhost:7038/Notifications/GetCountNotifications"
                    , "mmorf@gmail.com");
                countResponse_m = Int32.Parse(await response_m.Content.ReadAsStringAsync());
                countResponse_m.Should().Be(0);

                response_s = await client.PostAsJsonAsync("https://localhost:7038/Notifications", "stelios@gmail.com");
                responseContent_s = await response_s.Content.ReadAsStringAsync();
                responseContent_s = responseContent_s.Substring(2, responseContent_s.Length - 4);
                test_notifications_s = responseContent_s.Split("},{");
                test_notifications_s[0].Should().Contain("\"description\":\"Thessaloniki is on fire\"");
                test_notifications_s[0].Should().Contain("\"city_name\":\"THESS\"");
                test_notifications_s[0].Should().Contain("\"site_name\":\"METEO\"");
                test_notifications_s[0].Should().Contain("\"date\":\"2025-01-21\"");
                test_notifications_s[0].Should().Contain("\"time\":\"15:05:05\"");
                test_notifications_s[1].Should().Contain("\"description\":\"Heraklion is on fire\"");
                test_notifications_s[1].Should().Contain("\"city_name\":\"HERAK\"");
                test_notifications_s[1].Should().Contain("\"site_name\":\"EMY\"");
                test_notifications_s[1].Should().Contain("\"date\":\"2025-02-23\"");
                test_notifications_s[1].Should().Contain("\"time\":\"20:00:00\"");
                response_s = await client.PostAsJsonAsync("https://localhost:7038/Notifications/GetCountNotifications"
                    , "stelios@gmail.com");
                countResponse_s = Int32.Parse(await response_s.Content.ReadAsStringAsync());
                countResponse_s.Should().Be(2);

                await client.PostAsJsonAsync("https://localhost:7038/Notifications/ReadNotifications", "stelios@gmail.com");

                response_s = await client.PostAsJsonAsync("https://localhost:7038/Notifications", "stelios@gmail.com");
                responseContent_s = await response_s.Content.ReadAsStringAsync();
                test_notifications_s = responseContent_s.Split("},{");
                test_notifications_s.Count().Should().Be(1);
                test_notifications_s[0].Should().Be("[]");
                response_s = await client.PostAsJsonAsync("https://localhost:7038/Notifications/GetCountNotifications"
                    , "stelios@gmail.com");
                countResponse_s = Int32.Parse(await response_s.Content.ReadAsStringAsync());
                countResponse_s.Should().Be(0);

                response_m = await client.PostAsJsonAsync("https://localhost:7038/Notifications", "mmorf@gmail.com");
                responseContent_m = await response_m.Content.ReadAsStringAsync();
                test_notifications_m = responseContent_m.Split("},{");
                test_notifications_m.Length.Should().Be(1);
                test_notifications_m[0].Should().Be("[]");
                response_m = await client.PostAsJsonAsync("https://localhost:7038/Notifications/GetCountNotifications"
                    , "mmorf@gmail.com");
                countResponse_m = Int32.Parse(await response_m.Content.ReadAsStringAsync());
                countResponse_m.Should().Be(0);

                await base.emptyDB(dbContext);
            }
            
        
        
        
        }
    }
}
