using FluentAssertions;
using Lucene.Net.Support;
using Microsoft.CodeAnalysis.Elfie.Model.Structures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NuGet.Configuration;
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
    public class PredictionsTesting : InMemoryDatabase
    {
        public PredictionsTesting(WeatherForecastFactory _factory) : base(_factory) { }
        [Fact]
        public async Task TestPredictions()
        {
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            using (var scope = base.CreateScope())
            {
                var scopeProvider = scope.ServiceProvider;
                var dbContext = scopeProvider.GetRequiredService<WeatherAppDbContext>();
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
                string[] siteNames = { "ACCU", "METEO", "EMY" };
                dbContext.Sites.AddRange(siteNames.Select(sitename => new ForecastSite(sitename)));
                await dbContext.SaveChangesAsync();

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


                Timeslot[] testTimeslots = { new Timeslot { Date = new DateOnly(2025,1,21), Time= new TimeOnly(15,5,5) },
                    new Timeslot { Date = new DateOnly(2025,11,23), Time= new TimeOnly(20,0,0) }};
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
                string[] site_names = { "ACCU", "METEO", "EMY" };
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

                var request_userInfo = "mmorf@gmail.com";
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/UserInfo", request_userInfo);
                var responseContent = await response.Content.ReadAsStringAsync();
                responseContent = responseContent.Substring(2, responseContent.Length - 4);
                string[] test_predictions = responseContent.Split("},{");
                string[] test_weather = { "10C", "8C", "9C", "7C", "9C", "7C", "8C", "6C" };
                bool[] test_danger = { true, false, false, false, false, false, false, true };
                string[] dates = { "2025-01-21", "2025-11-23", "2025-01-21", "2025-11-23", "2025-01-21", "2025-11-23", "2025-01-21", "2025-11-23" };
                string[] times = { "15:05:05", "20:00:00", "15:05:05", "20:00:00", "15:05:05", "20:00:00", "15:05:05", "20:00:00", };
                string[] cities = { "ATH", "ATH", "THESS", "THESS", "ATH", "ATH", "THESS", "THESS" };
                string[] sites = { "ACCU", "ACCU", "ACCU", "ACCU", "METEO", "METEO", "METEO", "METEO" };
                for (int i = 0; i < 8; i++)
                {
                    test_predictions[i].Should().Contain("\"city_name\":\"" + cities[i] + "\"");
                    test_predictions[i].Should().Contain("\"site_name\":\"" + sites[i] + "\"");
                    test_predictions[i].Should().Contain("\"date\":\"" + dates[i] + "\"");
                    test_predictions[i].Should().Contain("\"time\":\"" + times[i] + "\"");
                    test_predictions[i].Should().Contain("\"weather\":\"" + test_weather[i] + "\"");
                    test_predictions[i].Should().Contain(test_danger[i].ToString().ToLower());
                    test_predictions[i].Should().Contain("\"rating_Value\":0");
                }
                test_predictions.Count().Should().Be(8);

                request_userInfo = "smart@gmail.com";
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/UserInfo", request_userInfo);
                responseContent = await response.Content.ReadAsStringAsync();
                responseContent = responseContent.Substring(2, responseContent.Length - 4);
                test_predictions = responseContent.Split("},{");
                string[] test_weather2 = { "12C", "10C", "10C", "8C", "10C", "8C", "8C", "6C" };
                bool[] test_danger2 = { false, false, false, false, false, false, false, true };
                string[] dates2 = { "2025-01-21", "2025-11-23", "2025-01-21", "2025-11-23", "2025-01-21", "2025-11-23", "2025-01-21", "2025-11-23" };
                string[] times2 = { "15:05:05", "20:00:00", "15:05:05", "20:00:00", "15:05:05", "20:00:00", "15:05:05", "20:00:00", };
                string[] cities2 = { "HERAK", "HERAK", "THESS", "THESS", "HERAK", "HERAK", "THESS", "THESS" };
                string[] sites2 = { "EMY", "EMY", "EMY", "EMY", "METEO", "METEO", "METEO", "METEO" };
                for (int i = 0; i < 8; i++)
                {
                    test_predictions[i].Should().Contain("\"city_name\":\"" + cities2[i] + "\"");
                    test_predictions[i].Should().Contain("\"site_name\":\"" + sites2[i] + "\"");
                    test_predictions[i].Should().Contain("\"date\":\"" + dates2[i] + "\"");
                    test_predictions[i].Should().Contain("\"time\":\"" + times2[i] + "\"");
                    test_predictions[i].Should().Contain("\"weather\":\"" + test_weather2[i] + "\"");
                    test_predictions[i].Should().Contain(test_danger2[i].ToString().ToLower());
                    test_predictions[i].Should().Contain("\"rating_Value\":0");
                }
                test_predictions.Count().Should().Be(8);

                Rating rating = new Rating { Email = "mmorf@gmail.com", City_Id = cities_data["ATH"], Site_Id = sites_data["ACCU"], Timeslot_Id = timeslots_ids[0], Rating_Value = -1 };
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/AddRating", rating);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotAcceptable);
                responseContent.Should().Be("Rating must be integer from 1 to 5");

                rating = new Rating { Email = "mmorf@gmail.com", City_Id = cities_data["ATH"], Site_Id = sites_data["ACCU"], Timeslot_Id = timeslots_ids[0], Rating_Value = 6 };
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/AddRating", rating);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotAcceptable);
                responseContent.Should().Be("Rating must be integer from 1 to 5");

                rating = new Rating { Email = "mmorf@gmail.com", City_Id = cities_data["ATH"], Site_Id = sites_data["ACCU"], Timeslot_Id = timeslots_ids[0], Rating_Value = 5 };
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/AddRating", rating);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
                responseContent.Should().Be("Your rating has been submitted");

                rating = new Rating { Email = "smart@gmail.com", City_Id = cities_data["HERAK"], Site_Id = sites_data["EMY"], Timeslot_Id = timeslots_ids[1], Rating_Value = 1 };
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/AddRating", rating);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
                responseContent.Should().Be("Your rating has been submitted");

                rating = new Rating { Email = "mmorf@gmail.com", City_Id = cities_data["THESS"], Site_Id = sites_data["METEO"], Timeslot_Id = timeslots_ids[1], Rating_Value = 3 };
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/AddRating", rating);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
                responseContent.Should().Be("Your rating has been submitted");

                rating = new Rating { Email = "smart@gmail.com", City_Id = cities_data["THESS"], Site_Id = sites_data["METEO"], Timeslot_Id = timeslots_ids[1], Rating_Value = 4 };
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/AddRating", rating);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
                responseContent.Should().Be("Your rating has been submitted");
                request_userInfo = "mmorf@gmail.com";
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/UserInfo", request_userInfo);
                responseContent = await response.Content.ReadAsStringAsync();

                responseContent = responseContent.Substring(2, responseContent.Length - 4);
                test_predictions = responseContent.Split("},{");
                dbContext.Ratings.Count().Should().Be(4);
                int[] rating_values = { 5, 0, 0, 0, 0, 0, 0, 3 };
                int[] rating_values2 = { 0, 1, 0, 0, 0, 0, 0, 4 };

                for (int i = 0; i < 8; i++)
                {
                    Trace.WriteLine("XXXXXX" + i);
                    test_predictions[i].Should().Contain("\"city_name\":\"" + cities[i] + "\"");
                    test_predictions[i].Should().Contain("\"site_name\":\"" + sites[i] + "\"");
                    test_predictions[i].Should().Contain("\"date\":\"" + dates[i] + "\"");
                    test_predictions[i].Should().Contain("\"time\":\"" + times[i] + "\"");
                    test_predictions[i].Should().Contain("\"weather\":\"" + test_weather[i] + "\"");
                    test_predictions[i].Should().Contain(test_danger[i].ToString().ToLower());
                    test_predictions[i].Should().Contain("\"rating_Value\":" + rating_values[i]);
                }
                request_userInfo = "smart@gmail.com";
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/UserInfo", request_userInfo);
                responseContent = await response.Content.ReadAsStringAsync();
                responseContent = responseContent.Substring(2, responseContent.Length - 4);
                test_predictions = responseContent.Split("},{");
                for (int i = 0; i < 8; i++)
                {
                    Trace.WriteLine("XXXXXX" + test_danger[i]);
                    test_predictions[i].Should().Contain("\"city_name\":\"" + cities2[i] + "\"");
                    test_predictions[i].Should().Contain("\"site_name\":\"" + sites2[i] + "\"");
                    test_predictions[i].Should().Contain("\"date\":\"" + dates2[i] + "\"");
                    test_predictions[i].Should().Contain("\"time\":\"" + times2[i] + "\"");
                    test_predictions[i].Should().Contain("\"weather\":\"" + test_weather2[i] + "\"");
                    test_predictions[i].Should().Contain(test_danger2[i].ToString().ToLower());
                    test_predictions[i].Should().Contain("\"rating_Value\":" + rating_values2[i]);
                }
                rating = new Rating { Email = "smart@gmail.com", City_Id = cities_data["THESS"], Site_Id = sites_data["METEO"], Timeslot_Id = timeslots_ids[1], Rating_Value = 4 };
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/DeleteRating", rating);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
                responseContent.Should().Be("Rating has been deleted");

                rating = new Rating { Email = "mmorf@gmail.com", City_Id = cities_data["THESS"], Site_Id = sites_data["METEO"], Timeslot_Id = timeslots_ids[1], Rating_Value = 0 };
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/DeleteRating", rating);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
                responseContent.Should().Be("Rating has been deleted");

                rating_values[7] = 0;
                rating_values2[7] = 0;

                request_userInfo = "mmorf@gmail.com";
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/UserInfo", request_userInfo);
                responseContent = await response.Content.ReadAsStringAsync();
                responseContent = responseContent.Substring(2, responseContent.Length - 4);
                test_predictions = responseContent.Split("},{");
                dbContext.Ratings.Count().Should().Be(2);

                for (int i = 0; i < 8; i++)
                {
                    Trace.WriteLine("XXXXXX" + test_danger[i]);
                    test_predictions[i].Should().Contain("\"city_name\":\"" + cities[i] + "\"");
                    test_predictions[i].Should().Contain("\"site_name\":\"" + sites[i] + "\"");
                    test_predictions[i].Should().Contain("\"date\":\"" + dates[i] + "\"");
                    test_predictions[i].Should().Contain("\"time\":\"" + times[i] + "\"");
                    test_predictions[i].Should().Contain("\"weather\":\"" + test_weather[i] + "\"");
                    test_predictions[i].Should().Contain(test_danger[i].ToString().ToLower());
                    test_predictions[i].Should().Contain("\"rating_Value\":" + rating_values[i]);
                }
                request_userInfo = "smart@gmail.com";
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/UserInfo", request_userInfo);
                responseContent = await response.Content.ReadAsStringAsync();
                responseContent = responseContent.Substring(2, responseContent.Length - 4);
                test_predictions = responseContent.Split("},{");
                for (int i = 0; i < 8; i++)
                {
                    Trace.WriteLine("XXXXXX" + test_danger[i]);
                    test_predictions[i].Should().Contain("\"city_name\":\"" + cities2[i] + "\"");
                    test_predictions[i].Should().Contain("\"site_name\":\"" + sites2[i] + "\"");
                    test_predictions[i].Should().Contain("\"date\":\"" + dates2[i] + "\"");
                    test_predictions[i].Should().Contain("\"time\":\"" + times2[i] + "\"");
                    test_predictions[i].Should().Contain("\"weather\":\"" + test_weather2[i] + "\"");
                    test_predictions[i].Should().Contain(test_danger2[i].ToString().ToLower());
                    test_predictions[i].Should().Contain("\"rating_Value\":" + rating_values2[i]);
                }

                dbContext.Ratings.Count().Should().Be(2);

                rating = new Rating { Email = "smart@gmail.com", City_Id = cities_data["HERAK"], Site_Id = sites_data["EMY"], Timeslot_Id = timeslots_ids[1], Rating_Value = 4 };

                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/AddRating", rating);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
                responseContent.Should().Be("Your rating has been updated");

                request_userInfo = "smart@gmail.com";
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/UserInfo", request_userInfo);
                responseContent = await response.Content.ReadAsStringAsync();
                responseContent = responseContent.Substring(2, responseContent.Length - 4);
                test_predictions = responseContent.Split("},{");

                rating_values2[1] = 4;

                for (int i = 0; i < 8; i++)
                {
                    Trace.WriteLine("XAAAX" + test_predictions[i]);
                    test_predictions[i].Should().Contain("\"city_name\":\"" + cities2[i] + "\"");
                    test_predictions[i].Should().Contain("\"site_name\":\"" + sites2[i] + "\"");
                    test_predictions[i].Should().Contain("\"date\":\"" + dates2[i] + "\"");
                    test_predictions[i].Should().Contain("\"time\":\"" + times2[i] + "\"");
                    test_predictions[i].Should().Contain("\"weather\":\"" + test_weather2[i] + "\"");
                    test_predictions[i].Should().Contain(test_danger2[i].ToString().ToLower());
                    test_predictions[i].Should().Contain("\"rating_Value\":" + rating_values2[i]);
                }
                dbContext.Ratings.Count().Should().Be(2);

                rating = new Rating { Email = "mmorf@gmail.com", City_Id = cities_data["ATH"], Site_Id = sites_data["ACCU"], Timeslot_Id = timeslots_ids[0], Rating_Value = 5 };
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/DeleteRating", rating);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
                responseContent.Should().Be("Rating has been deleted");

                rating = new Rating { Email = "smart@gmail.com", City_Id = cities_data["HERAK"], Site_Id = sites_data["EMY"], Timeslot_Id = timeslots_ids[1], Rating_Value = 4 };
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/DeleteRating", rating);
                responseContent = await response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
                responseContent.Should().Be("Rating has been deleted");

                request_userInfo = "mmorf@gmail.com";
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/UserInfo", request_userInfo);
                responseContent = await response.Content.ReadAsStringAsync();
                responseContent = responseContent.Substring(2, responseContent.Length - 4);
                test_predictions = responseContent.Split("},{");
                for (int i = 0; i < 8; i++)
                {
                    Trace.WriteLine("XXXXXX" + test_danger[i]);
                    test_predictions[i].Should().Contain("\"city_name\":\"" + cities[i] + "\"");
                    test_predictions[i].Should().Contain("\"site_name\":\"" + sites[i] + "\"");
                    test_predictions[i].Should().Contain("\"date\":\"" + dates[i] + "\"");
                    test_predictions[i].Should().Contain("\"time\":\"" + times[i] + "\"");
                    test_predictions[i].Should().Contain("\"weather\":\"" + test_weather[i] + "\"");
                    test_predictions[i].Should().Contain(test_danger[i].ToString().ToLower());
                    test_predictions[i].Should().Contain("\"rating_Value\":0");
                }

                request_userInfo = "smart@gmail.com";
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/UserInfo", request_userInfo);
                responseContent = await response.Content.ReadAsStringAsync();
                responseContent = responseContent.Substring(2, responseContent.Length - 4);
                test_predictions = responseContent.Split("},{");
                for (int i = 0; i < 8; i++)
                {
                    Trace.WriteLine("XXXXXX" + test_danger[i]);
                    test_predictions[i].Should().Contain("\"city_name\":\"" + cities2[i] + "\"");
                    test_predictions[i].Should().Contain("\"site_name\":\"" + sites2[i] + "\"");
                    test_predictions[i].Should().Contain("\"date\":\"" + dates2[i] + "\"");
                    test_predictions[i].Should().Contain("\"time\":\"" + times2[i] + "\"");
                    test_predictions[i].Should().Contain("\"weather\":\"" + test_weather2[i] + "\"");
                    test_predictions[i].Should().Contain(test_danger2[i].ToString().ToLower());
                    test_predictions[i].Should().Contain("\"rating_Value\":0");
                }
                await base.emptyDB(dbContext);


            }
        }
    }
}

