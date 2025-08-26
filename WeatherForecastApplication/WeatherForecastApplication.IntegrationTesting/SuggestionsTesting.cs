using FluentAssertions;
using Lucene.Net.Support;
using Microsoft.EntityFrameworkCore;
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
    public class SuggestionsTesting : InMemoryDatabase
    {

        public SuggestionsTesting(WeatherForecastFactory _factory) : base(_factory) { }

        [Fact]
        public async Task TestSuggestions()
        {
            using (var scope = base.CreateScope())
            {
                var provider = scope.ServiceProvider;
                var dbContext = provider.GetRequiredService<WeatherAppDbContext>();
                var client = base.CreateClient();
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
                string[] sites = { "ACCU", "METEO", "EMY" };
                dbContext.Sites.AddRange(sites.Select(sitename => new ForecastSite(sitename)));
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

                request = new SignUpInput()
                {
                    Email = "stelios@gmail.com",
                    Hashed_password = "werwq",
                    Username = "stelios",
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

                var request_user_info = "mmorf@gmail.com";
                response = await client.PostAsJsonAsync("https://localhost:7038/Suggestions/", request_user_info);
                var responseContent=await response.Content.ReadAsStringAsync();
                Trace.WriteLine("SUGGGGGGGGGGG" + responseContent);
                responseContent = responseContent.Substring(2, responseContent.Length - 4);
                string[] test_predictions = responseContent.Split("},{");
                string[] cityNames_m = { "ATH", "ATH", "THESS", "THESS" };
                int[] people_ineterested_m = {1,1,1,1 };
                string[] dates_m = { "2025-01-21", "2025-11-23", "2025-01-21", "2025-11-23" };
                string[] times_m = { "15:05:05", "20:00:00", "15:05:05", "20:00:00" };
                bool[] test_danger_m = {true,false,false,false };
                string[] test_weather_m = { "10C", "8C", "9C", "7C" };
                for (int i = 0; i < 4; i++) {
                    test_predictions[i].Should().Contain("\"city_name\":\"" + cityNames_m[i] + "\"");
                    test_predictions[i].Should().Contain("\"date\":\"" + dates_m[i] + "\"");
                    test_predictions[i].Should().Contain("\"time\":\"" + times_m[i] + "\"");
                    test_predictions[i].Should().Contain("\"site_name\":\"" + "ACCU" + "\"");
                    test_predictions[i].Should().Contain("\"weather\":\"" + test_weather_m[i] + "\"");
                    test_predictions[i].Should().Contain(test_danger_m[i].ToString().ToLower());
                    test_predictions[i].Should().Contain("\"max_Rating\":0");
                    test_predictions[i].Should().Contain("\"total_Ratings\":0");
                    test_predictions[i].Should().Contain("\"people_Interested\":" + people_ineterested_m[i]);
                }
                test_predictions.Count().Should().Be(4);

                request_user_info = "stelios@gmail.com";
                response = await client.PostAsJsonAsync("https://localhost:7038/Suggestions/", request_user_info);
                responseContent = await response.Content.ReadAsStringAsync();
                Trace.WriteLine("SUGGGGGGGGGGG" + responseContent);
                responseContent = responseContent.Substring(2, responseContent.Length - 4);
                test_predictions = responseContent.Split("},{");
                string[] cityNames_s =  { "HERAK","HERAK","THESS","THESS" };
                int[] people_ineterested_s = { 1, 1, 1, 1 };
                string[] dates_s = { "2025-01-21", "2025-11-23", "2025-01-21", "2025-11-23" };
                string[] times_s = { "15:05:05", "20:00:00", "15:05:05", "20:00:00" };
                bool[] test_danger_s = { false, false, false, false };
                string[] test_weather_s = { "12C","10C","10C","8C"};
                for (int i = 0; i < 4; i++)
                {
                    test_predictions[i].Should().Contain("\"city_name\":\"" + cityNames_s[i] + "\"");
                    test_predictions[i].Should().Contain("\"date\":\"" + dates_s[i] + "\"");
                    test_predictions[i].Should().Contain("\"time\":\"" + times_s[i] + "\"");
                    test_predictions[i].Should().Contain("\"site_name\":\"" + "EMY" + "\"");
                    test_predictions[i].Should().Contain("\"weather\":\"" + test_weather_s[i] + "\"");
                    test_predictions[i].Should().Contain(test_danger_s[i].ToString().ToLower());
                    test_predictions[i].Should().Contain("\"max_Rating\":0");
                    test_predictions[i].Should().Contain("\"total_Ratings\":0");
                    test_predictions[i].Should().Contain("\"people_Interested\":" + people_ineterested_s[i]);
                }
                

                var rating_m = new Rating
                {
                    City_Id = cities_data["THESS"],
                    Site_Id = sites_data["METEO"],
                    Email = "mmorf@gmail.com",
                    Timeslot_Id = timeslots_ids[0],
                    Rating_Value = 4
                };
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/AddRating", rating_m);
                var rating_s = new Rating
                {
                    City_Id = cities_data["THESS"],
                    Site_Id = sites_data["METEO"],
                    Email = "stelios@gmail.com",
                    Timeslot_Id = timeslots_ids[1],
                    Rating_Value = 4
                };
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/AddRating", rating_s);

                request_user_info = "mmorf@gmail.com";
                response = await client.PostAsJsonAsync("https://localhost:7038/Suggestions/", request_user_info);
                responseContent = await response.Content.ReadAsStringAsync();
                Trace.WriteLine("SUGGGGGGGGGGG" + responseContent);
                responseContent = responseContent.Substring(2, responseContent.Length - 4);
                test_predictions = responseContent.Split("},{");
                people_ineterested_m = [1, 1, 2, 2];
                var total_ratings_m = new int[] { 0, 0, 2, 2 };
                var max_ratings_m = new double[] { 0, 0, 4, 4 };
                var sites_m = new string[] { "ACCU", "ACCU", "METEO", "METEO" };
                test_danger_m = [ true, false, false, true];
                test_weather_m = [ "10C", "8C", "8C", "6C" ];
                for (int i = 0; i < 4; i++)
                {
                    test_predictions[i].Should().Contain("\"city_name\":\"" + cityNames_m[i] + "\"");
                    test_predictions[i].Should().Contain("\"date\":\"" + dates_m[i] + "\"");
                    test_predictions[i].Should().Contain("\"time\":\"" + times_m[i] + "\"");
                    test_predictions[i].Should().Contain("\"site_name\":\"" + sites_m[i] + "\"");
                    test_predictions[i].Should().Contain("\"weather\":\"" + test_weather_m[i] + "\"");
                    test_predictions[i].Should().Contain(test_danger_m[i].ToString().ToLower());
                    test_predictions[i].Should().Contain("\"max_Rating\":" + max_ratings_m[i]);
                    test_predictions[i].Should().Contain("\"total_Ratings\":" + total_ratings_m[i]);
                    test_predictions[i].Should().Contain("\"people_Interested\":" + people_ineterested_m[i]);
                }
                test_predictions.Count().Should().Be(4);

                request_user_info = "stelios@gmail.com";
                response = await client.PostAsJsonAsync("https://localhost:7038/Suggestions/", request_user_info);
                responseContent = await response.Content.ReadAsStringAsync();
                Trace.WriteLine("SUGGGGGGGGGGG" + responseContent);
                responseContent = responseContent.Substring(2, responseContent.Length - 4);
                test_predictions = responseContent.Split("},{");
                people_ineterested_s = [ 1, 1, 2, 2 ];
                var total_ratings_s = new int[] { 0, 0, 2, 2 };
                var max_ratings_s = new double[] { 0, 0, 4, 4 };
                var sites_s = new string[] { "EMY", "EMY", "METEO", "METEO" };
                test_danger_s = [ false, false, false, true ];
                test_weather_s = [ "12C", "10C", "8C", "6C" ];
                for (int i = 0; i < 4; i++)
                {
                    test_predictions[i].Should().Contain("\"city_name\":\"" + cityNames_s[i] + "\"");
                    test_predictions[i].Should().Contain("\"date\":\"" + dates_s[i] + "\"");
                    test_predictions[i].Should().Contain("\"time\":\"" + times_s[i] + "\"");
                    test_predictions[i].Should().Contain("\"site_name\":\"" + sites_s[i] + "\"");
                    test_predictions[i].Should().Contain("\"weather\":\"" + test_weather_s[i] + "\"");
                    test_predictions[i].Should().Contain(test_danger_s[i].ToString().ToLower());
                    test_predictions[i].Should().Contain("\"max_Rating\":" + max_ratings_s[i]);
                    test_predictions[i].Should().Contain("\"total_Ratings\":" + total_ratings_s[i]);
                    test_predictions[i].Should().Contain("\"people_Interested\":" + people_ineterested_s[i]);
                }

                 rating_m = new Rating
                {
                    City_Id = cities_data["THESS"],
                    Site_Id = sites_data["ACCU"],
                    Email = "mmorf@gmail.com",
                    Timeslot_Id = timeslots_ids[1],
                    Rating_Value = 3
                };
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/AddRating", rating_m);

                request_user_info = "mmorf@gmail.com";
                response = await client.PostAsJsonAsync("https://localhost:7038/Suggestions/", request_user_info);
                responseContent = await response.Content.ReadAsStringAsync();
                test_predictions = responseContent.Split("},{");
                for (int i = 0; i < 4; i++)
                {
                    test_predictions[i].Should().Contain("\"city_name\":\"" + cityNames_m[i] + "\"");
                    test_predictions[i].Should().Contain("\"date\":\"" + dates_m[i] + "\"");
                    test_predictions[i].Should().Contain("\"time\":\"" + times_m[i] + "\"");
                    test_predictions[i].Should().Contain("\"site_name\":\"" + sites_m[i] + "\"");
                    test_predictions[i].Should().Contain("\"weather\":\"" + test_weather_m[i] + "\"");
                    test_predictions[i].Should().Contain(test_danger_m[i].ToString().ToLower());
                    test_predictions[i].Should().Contain("\"max_Rating\":" + max_ratings_m[i]);
                    test_predictions[i].Should().Contain("\"total_Ratings\":" + total_ratings_m[i]);
                    test_predictions[i].Should().Contain("\"people_Interested\":" + people_ineterested_m[i]);
                }
                test_predictions.Count().Should().Be(4);

                request_user_info = "stelios@gmail.com";
                response = await client.PostAsJsonAsync("https://localhost:7038/Suggestions/", request_user_info);
                responseContent = await response.Content.ReadAsStringAsync();
                test_predictions = responseContent.Split("},{");

                for (int i = 0; i < 4; i++)
                {
                    test_predictions[i].Should().Contain("\"city_name\":\"" + cityNames_s[i] + "\"");
                    test_predictions[i].Should().Contain("\"date\":\"" + dates_s[i] + "\"");
                    test_predictions[i].Should().Contain("\"time\":\"" + times_s[i] + "\"");
                    test_predictions[i].Should().Contain("\"site_name\":\"" + sites_s[i] + "\"");
                    test_predictions[i].Should().Contain("\"weather\":\"" + test_weather_s[i] + "\"");
                    test_predictions[i].Should().Contain(test_danger_s[i].ToString().ToLower());
                    test_predictions[i].Should().Contain("\"max_Rating\":" + max_ratings_s[i]);
                    test_predictions[i].Should().Contain("\"total_Ratings\":" + total_ratings_s[i]);
                    test_predictions[i].Should().Contain("\"people_Interested\":" + people_ineterested_s[i]);
                }

                rating_m = new Rating
                {
                    City_Id = cities_data["THESS"],
                    Site_Id = sites_data["ACCU"],
                    Email = "mmorf@gmail.com",
                    Timeslot_Id = timeslots_ids[1],
                    Rating_Value = 4
                };
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/AddRating", rating_m);

                request_user_info = "mmorf@gmail.com";
                response = await client.PostAsJsonAsync("https://localhost:7038/Suggestions/", request_user_info);
                responseContent = await response.Content.ReadAsStringAsync();
                Trace.WriteLine("SUGGGGGGGGGGG" + responseContent);
                responseContent = responseContent.Substring(2, responseContent.Length - 4);
                test_predictions = responseContent.Split("},{");
                cityNames_m = [ "ATH", "ATH", "THESS", "THESS" ];
                people_ineterested_m = [ 1, 1, 1, 1 ];
                test_danger_m = [ true, false, false, false ];
                test_weather_m = [ "10C", "8C", "9C", "7C" ];
                total_ratings_m = [0, 0, 1, 1];
                max_ratings_m = [0, 0, 4, 4];
                for (int i = 0; i < 4; i++)
                {
                    test_predictions[i].Should().Contain("\"city_name\":\"" + cityNames_m[i] + "\"");
                    test_predictions[i].Should().Contain("\"date\":\"" + dates_m[i] + "\"");
                    test_predictions[i].Should().Contain("\"time\":\"" + times_m[i] + "\"");
                    test_predictions[i].Should().Contain("\"site_name\":\"" + "ACCU" + "\"");
                    test_predictions[i].Should().Contain("\"weather\":\"" + test_weather_m[i] + "\"");
                    test_predictions[i].Should().Contain(test_danger_m[i].ToString().ToLower());
                    test_predictions[i].Should().Contain("\"max_Rating\":" + max_ratings_m[i]);
                    test_predictions[i].Should().Contain("\"total_Ratings\":" + total_ratings_m[i]);
                    test_predictions[i].Should().Contain("\"people_Interested\":" + people_ineterested_m[i]);
                }
                test_predictions.Count().Should().Be(4);

                request_user_info = "stelios@gmail.com";
                response = await client.PostAsJsonAsync("https://localhost:7038/Suggestions/", request_user_info);
                responseContent = await response.Content.ReadAsStringAsync();
                Trace.WriteLine("SUGGGGGGGGGGG" + responseContent);
                responseContent = responseContent.Substring(2, responseContent.Length - 4);
                test_predictions = responseContent.Split("},{");
                for (int i = 0; i < 4; i++)
                {
                    test_predictions[i].Should().Contain("\"city_name\":\"" + cityNames_s[i] + "\"");
                    test_predictions[i].Should().Contain("\"date\":\"" + dates_s[i] + "\"");
                    test_predictions[i].Should().Contain("\"time\":\"" + times_s[i] + "\"");
                    test_predictions[i].Should().Contain("\"site_name\":\"" + sites_s[i] + "\"");
                    test_predictions[i].Should().Contain("\"weather\":\"" + test_weather_s[i] + "\"");
                    test_predictions[i].Should().Contain(test_danger_s[i].ToString().ToLower());
                    test_predictions[i].Should().Contain("\"max_Rating\":" + max_ratings_s[i]);
                    test_predictions[i].Should().Contain("\"total_Ratings\":" + total_ratings_s[i]);
                    test_predictions[i].Should().Contain("\"people_Interested\":" + people_ineterested_s[i]);
                }
                test_predictions.Count().Should().Be(4);

                rating_m = new Rating
                {
                    City_Id = cities_data["THESS"],
                    Site_Id = sites_data["ACCU"],
                    Email = "mmorf@gmail.com",
                    Timeslot_Id = timeslots_ids[0],
                    Rating_Value = 5
                };
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/AddRating", rating_m);
                request_user_info = "mmorf@gmail.com";
                response = await client.PostAsJsonAsync("https://localhost:7038/Suggestions/", request_user_info);
                responseContent = await response.Content.ReadAsStringAsync();
                Trace.WriteLine("SUGGGGGGGGGGG" + responseContent);
                responseContent = responseContent.Substring(2, responseContent.Length - 4);
                test_predictions = responseContent.Split("},{");

                total_ratings_m = [0, 0, 2, 2];
                max_ratings_m = [0, 0, 4.5, 4.5];
                for (int i = 0; i < 4; i++)
                {
                    test_predictions[i].Should().Contain("\"city_name\":\"" + cityNames_m[i] + "\"");
                    test_predictions[i].Should().Contain("\"date\":\"" + dates_m[i] + "\"");
                    test_predictions[i].Should().Contain("\"time\":\"" + times_m[i] + "\"");
                    test_predictions[i].Should().Contain("\"site_name\":\"" + "ACCU" + "\"");
                    test_predictions[i].Should().Contain("\"weather\":\"" + test_weather_m[i] + "\"");
                    test_predictions[i].Should().Contain(test_danger_m[i].ToString().ToLower());
                    test_predictions[i].Should().Contain("\"max_Rating\":" + max_ratings_m[i]);
                    test_predictions[i].Should().Contain("\"total_Ratings\":" + total_ratings_m[i]);
                    test_predictions[i].Should().Contain("\"people_Interested\":" + people_ineterested_m[i]);
                }
                test_predictions.Count().Should().Be(4);
               
                request_user_info = "stelios@gmail.com";
                response = await client.PostAsJsonAsync("https://localhost:7038/Suggestions/", request_user_info);
                responseContent = await response.Content.ReadAsStringAsync();
                Trace.WriteLine("SUGGGGGGGGGGG" + responseContent);
                responseContent = responseContent.Substring(2, responseContent.Length - 4);
                test_predictions = responseContent.Split("},{");
                for (int i = 0; i < 4; i++)
                {
                    test_predictions[i].Should().Contain("\"city_name\":\"" + cityNames_s[i] + "\"");
                    test_predictions[i].Should().Contain("\"date\":\"" + dates_s[i] + "\"");
                    test_predictions[i].Should().Contain("\"time\":\"" + times_s[i] + "\"");
                    test_predictions[i].Should().Contain("\"site_name\":\"" + sites_s[i] + "\"");
                    test_predictions[i].Should().Contain("\"weather\":\"" + test_weather_s[i] + "\"");
                    test_predictions[i].Should().Contain(test_danger_s[i].ToString().ToLower());
                    test_predictions[i].Should().Contain("\"max_Rating\":" + max_ratings_s[i]);
                    test_predictions[i].Should().Contain("\"total_Ratings\":" + total_ratings_s[i]);
                    test_predictions[i].Should().Contain("\"people_Interested\":" + people_ineterested_s[i]);
                }
                test_predictions.Count().Should().Be(4);

                await dbContext.Ratings.ExecuteDeleteAsync();

                rating_m = new Rating
                {
                    City_Id = cities_data["ATH"],
                    Site_Id = sites_data["ACCU"],
                    Email = "mmorf@gmail.com",
                    Timeslot_Id = timeslots_ids[0],
                    Rating_Value = 5
                };
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/AddRating", rating_m);

                rating_m = new Rating
                {
                    City_Id = cities_data["THESS"],
                    Site_Id = sites_data["ACCU"],
                    Email = "mmorf@gmail.com",
                    Timeslot_Id = timeslots_ids[0],
                    Rating_Value = 1
                };
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/AddRating", rating_m);

                rating_m = new Rating
                {
                    City_Id = cities_data["THESS"],
                    Site_Id = sites_data["METEO"],
                    Email = "mmorf@gmail.com",
                    Timeslot_Id = timeslots_ids[0],
                    Rating_Value = 3
                };
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/AddRating", rating_m);

                rating_s = new Rating
                {
                    City_Id = cities_data["HERAK"],
                    Site_Id = sites_data["EMY"],
                    Email = "stelios@gmail.com",
                    Timeslot_Id = timeslots_ids[0],
                    Rating_Value = 4
                };
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/AddRating", rating_s);

                rating_s = new Rating
                {
                    City_Id = cities_data["THESS"],
                    Site_Id = sites_data["EMY"],
                    Email = "stelios@gmail.com",
                    Timeslot_Id = timeslots_ids[0],
                    Rating_Value = 2
                };
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/AddRating", rating_s);

                rating_s = new Rating
                {
                    City_Id = cities_data["THESS"],
                    Site_Id = sites_data["METEO"],
                    Email = "stelios@gmail.com",
                    Timeslot_Id = timeslots_ids[0],
                    Rating_Value = 4
                };
                response = await client.PostAsJsonAsync("https://localhost:7038/Predictions/AddRating", rating_s);


                request_user_info = "mmorf@gmail.com";
                response = await client.PostAsJsonAsync("https://localhost:7038/Suggestions/", request_user_info);
                responseContent = await response.Content.ReadAsStringAsync();
                Trace.WriteLine("SUGGGGGGGGGGG" + responseContent);
                responseContent = responseContent.Substring(2, responseContent.Length - 4);
                test_predictions = responseContent.Split("},{");
                cityNames_m = ["ATH", "ATH", "THESS", "THESS"];
                sites_m = ["ACCU", "ACCU", "METEO", "METEO"];
                people_ineterested_m = [1, 1, 2, 2];
                test_danger_m = [true, false, false, true];
                test_weather_m = ["10C", "8C", "8C", "6C"];
                total_ratings_m = [1, 1, 2, 2];
                max_ratings_m = [5, 5, 3.5, 3.5];
                for (int i = 0; i < 4; i++)
                {
                    test_predictions[i].Should().Contain("\"city_name\":\"" + cityNames_m[i] + "\"");
                    test_predictions[i].Should().Contain("\"date\":\"" + dates_m[i] + "\"");
                    test_predictions[i].Should().Contain("\"time\":\"" + times_m[i] + "\"");
                    test_predictions[i].Should().Contain("\"site_name\":\"" + sites_m[i] + "\"");
                    test_predictions[i].Should().Contain("\"weather\":\"" + test_weather_m[i] + "\"");
                    test_predictions[i].Should().Contain(test_danger_m[i].ToString().ToLower());
                    test_predictions[i].Should().Contain("\"max_Rating\":" + max_ratings_m[i]);
                    test_predictions[i].Should().Contain("\"total_Ratings\":" + total_ratings_m[i]);
                    test_predictions[i].Should().Contain("\"people_Interested\":" + people_ineterested_m[i]);
                }
                test_predictions.Count().Should().Be(4);

                request_user_info = "stelios@gmail.com";
                response = await client.PostAsJsonAsync("https://localhost:7038/Suggestions/", request_user_info);
                responseContent = await response.Content.ReadAsStringAsync();
                Trace.WriteLine("SUGGGGGGGGGGG" + responseContent);
                responseContent = responseContent.Substring(2, responseContent.Length - 4);
                test_predictions = responseContent.Split("},{");
                cityNames_s = ["HERAK", "HERAK", "THESS", "THESS"];
                sites_s = ["EMY", "EMY", "METEO", "METEO"];
                people_ineterested_s = [1, 1, 2, 2];
                test_danger_s = [false, false, false, true];
                test_weather_s = ["12C", "10C", "8C", "6C"];
                total_ratings_s = [1, 1, 2, 2];
                max_ratings_s = [4, 4, 3.5, 3.5];
                for (int i = 0; i < 4; i++)
                {
                    test_predictions[i].Should().Contain("\"city_name\":\"" + cityNames_s[i] + "\"");
                    test_predictions[i].Should().Contain("\"date\":\"" + dates_s[i] + "\"");
                    test_predictions[i].Should().Contain("\"time\":\"" + times_s[i] + "\"");
                    test_predictions[i].Should().Contain("\"site_name\":\"" + sites_s[i] + "\"");
                    test_predictions[i].Should().Contain("\"weather\":\"" + test_weather_s[i] + "\"");
                    test_predictions[i].Should().Contain(test_danger_s[i].ToString().ToLower());
                    test_predictions[i].Should().Contain("\"max_Rating\":" + max_ratings_s[i]);
                    test_predictions[i].Should().Contain("\"total_Ratings\":" + total_ratings_s[i]);
                    test_predictions[i].Should().Contain("\"people_Interested\":" + people_ineterested_s[i]);
                }
                test_predictions.Count().Should().Be(4);


                await base.emptyDB(dbContext);


            }
        }
    }
}
