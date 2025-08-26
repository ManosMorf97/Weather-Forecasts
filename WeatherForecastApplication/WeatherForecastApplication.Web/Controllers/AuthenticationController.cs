using Lucene.Net.Support;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WeatherForecastApplication.Data;
using WeatherForecastApplication.Web.Data;
using WeatherForecastApplication.Web.Models;

namespace WeatherForecastApplication.Web.Controllers
{
    public class AuthenticationController : Controller
    {
        readonly WeatherAppDbContext weatherAppDbContext;

        public AuthenticationController(WeatherAppDbContext weatherAppDbContext) {
            this.weatherAppDbContext = weatherAppDbContext;
        }
        public async Task UpdateUserNotifications(string Email)
        {
            List<UserNotification> tobeInserted=weatherAppDbContext.UserSiteCities.Where(usc => usc.Email.Equals(Email)).
                Join(weatherAppDbContext.Notifications,
                usc => new { usc.Site_Id, usc.City_Id },no=> new {no.Site_Id,no.City_Id},(usc,no)=>new {usc,no}).
                Join(weatherAppDbContext.Timeslots.Where(ti =>  ti.Date>=TimeSetting.getDate() && ti.Time>=TimeSetting.getTime()),
                joined=> joined.no.Timeslot_Id, timeslot => timeslot.Timeslot_Id, (joined, timeslot) =>
                new UserNotification {
                    Email=joined.usc.Email,
                    Site_Id=joined.no.Site_Id,
                    City_Id=joined.no.City_Id,
                    Timeslot_Id=joined.no.Timeslot_Id,
                    IsRead=false}).ToList();
            if(tobeInserted.Count>=0)
                await weatherAppDbContext.UserNotifications.AddRangeAsync(tobeInserted);
                await weatherAppDbContext.SaveChangesAsync();

        }

        [HttpPost]
        public async Task<IActionResult> SignUp([FromBody] SignUpInput signUpInput)
        {
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            if (!ModelState.IsValid)
                return StatusCode(400, "some fields are missing");
            bool[] conditions = { signUpInput.Email == null || !signUpInput.Email.Contains('@'),
                signUpInput.Username == null || signUpInput.Username.Length == 0 ,
                signUpInput.Hashed_password == null || signUpInput.Hashed_password.Length<5
            };
            string[] danger_messages = { "Non valid Email", "Username is empty", "Password is less than 5 characters" };
            int[] codes = { 400, 400, 406 };
            for (int i = 0; i < 3; i++)
                if (conditions[i])
                    return StatusCode(codes[i], danger_messages[i]);

            User? user = await weatherAppDbContext.Users.FirstOrDefaultAsync(u => u.Email == signUpInput.Email);
            if (user != null)
                return StatusCode(401, "User exists");
            user = await weatherAppDbContext.Users.FirstOrDefaultAsync(u => u.Username == signUpInput.Username);
            if (user != null)
                return StatusCode(401, "Username is taken");
            user = new User(signUpInput.Email, signUpInput.Hashed_password, signUpInput.Username);
            await weatherAppDbContext.Users.AddAsync(user);
            if (signUpInput.CityNames == null || signUpInput.CityNames.IsNullOrEmpty())
                return StatusCode(400, "Cities have not been selected");
            if (signUpInput.SiteNames == null || signUpInput.SiteNames.IsNullOrEmpty())
                return StatusCode(400, "Sites have not been selected");
            await using (var transcation = await weatherAppDbContext.Database.BeginTransactionAsync())
            {
                HashSet<string> inserted_city_names = (weatherAppDbContext.Cities.ToList<City>()).Select(city => city.City_name).ToHashSet();
                HashSet<string> given_city_names = signUpInput.CityNames.ToHashSet<string>();
                HashSet<string> city_names_to_be_inserted = given_city_names.Except(inserted_city_names).ToHashSet();
                if (city_names_to_be_inserted != null && !city_names_to_be_inserted.IsNullOrEmpty())
                    await weatherAppDbContext.Cities.AddRangeAsync(city_names_to_be_inserted.Select(name => new City(name)));
                await weatherAppDbContext.SaveChangesAsync();
                int[] given_city_ids = weatherAppDbContext.Cities.Where(city => given_city_names.Contains(city.City_name)).
                    Select(city => city.City_Id).ToArray<int>();
                int[] given_site_ids = weatherAppDbContext.Sites.Where(fs => signUpInput.SiteNames.Contains(fs.Site_name)).
                    Select(site => site.Site_Id).ToArray<int>();
                List<UserSiteCity>? userSiteCities = new List<UserSiteCity>();
                List<CitySite>? given_citySites_list = new List<CitySite>();
                foreach (int city_id in given_city_ids)
                {
                    foreach (int site_id in given_site_ids)
                    {
                        userSiteCities.Add(new UserSiteCity(signUpInput.Email, city_id, site_id));
                        given_citySites_list.Add(new CitySite(city_id, site_id));
                    }
                }
                HashSet<CitySite> inserted_citysites = (weatherAppDbContext.CitySites.ToList<CitySite>()).ToHashSet();
                HashSet<CitySite> given_citysites = given_citySites_list.ToHashSet<CitySite>();
                HashSet<CitySite> citysites_to_be_inserted = given_citysites.Except(inserted_citysites).ToHashSet();
                if (citysites_to_be_inserted != null && !citysites_to_be_inserted.IsNullOrEmpty())
                    await weatherAppDbContext.CitySites.AddRangeAsync(citysites_to_be_inserted);
                await weatherAppDbContext.UserSiteCities.AddRangeAsync(userSiteCities);
                await weatherAppDbContext.SaveChangesAsync();
                await UpdateUserNotifications(signUpInput.Email);
                await transcation.CommitAsync();
            }
            return Ok("The User has been created");
        }

        public async Task<IActionResult> SignIn([FromBody] SignInInput signInInput)
        {
            if (!ModelState.IsValid || signInInput.Email_Username.Length == 0 || signInInput.HashedPassword.Length == 0)
                return BadRequest("Some fields are empty");
            Func<User, string?> input;

            if (signInInput.Email_Username.Contains('@'))
            {
                input = (x) => { return x.Email; };
            }
            else
            {
                input = (x) => { return x.Username; };
            }
            var credentials = await weatherAppDbContext.Users.FirstOrDefaultAsync
                (x => (x.Email.Equals(signInInput.Email_Username) || x.Username.Equals(signInInput.Email_Username))
                && x.Hashed_password.Equals(signInInput.HashedPassword));
            if (credentials != null)
                return Ok("Logged in succesfully");
            var user_attempted = await weatherAppDbContext.Users.FirstOrDefaultAsync
                (x => x.Email.Equals(signInInput.Email_Username) || x.Username.Equals(signInInput.Email_Username));
            if (user_attempted == null)
                return NotFound("This Email/Username does not respond to a specific user");
            return Unauthorized("Wrong password");



        }
        [HttpPut]
        public async Task PrepareSites(string[] siteNames)
        {
            await weatherAppDbContext.Sites.AddRangeAsync(siteNames.Select(sitename => new ForecastSite(sitename)));
            weatherAppDbContext.SaveChanges();
        }

        [HttpGet]
        public  List<User> GetUsers()
        {
            return weatherAppDbContext.Users.ToList();
        }
        [HttpGet]
        public  List<City> GetCities()
        {
            return weatherAppDbContext.Cities.ToList();
        }
        [HttpGet]
        public List<ForecastSite> GetSites()
        {
            return weatherAppDbContext.Sites.ToList();
        }
        [HttpGet]
        public List<CitySiteNames> GetCitySites()
        {
            var citysite_results = weatherAppDbContext.CitySites.Join(weatherAppDbContext.Cities, citysite => citysite.City_Id, city => city.City_Id,
                     (citysite, city) => new { citysite, city })
                .Join(weatherAppDbContext.Sites, citysite_city => citysite_city.citysite.Site_Id, site => site.Site_Id,
                (citysite_city, site) => new { citysite_city.citysite,citysite_city.city,site }).
                Select(res => new { res.city.City_name, res.site.Site_name }).
                OrderBy(res => res.City_name).ThenBy(res => res.Site_name).ToList();
            return citysite_results.Select(res=>new CitySiteNames(res.City_name, res.Site_name)).ToList();
        }
        [HttpGet]
        public List<UserSiteCityNames> GetUserCitySites()
        {
            var usersitecity_results = weatherAppDbContext.CitySites.Join(weatherAppDbContext.Cities, citysite => citysite.City_Id, city => city.City_Id,
                     (citysite, city) => new { citysite, city })
                .Join(weatherAppDbContext.Sites, citysite_city => citysite_city.citysite.Site_Id, site => site.Site_Id,
                (citysite_city, site) => new { citysite_city.citysite, citysite_city.city, site })
                .Join(weatherAppDbContext.UserSiteCities,
                    ccs => new { ccs.citysite.City_Id, ccs.citysite.Site_Id }, usc => new { usc.City_Id, usc.Site_Id },
                    (ccs, usc) => new { usc.Email, ccs.city.City_name, ccs.site.Site_name }).
                    OrderBy(res => res.Email).ThenBy(res => res.City_name).ThenBy(res => res.Site_name).ToList();
            return usersitecity_results.Select(res => new UserSiteCityNames(res.Email,res.City_name, res.Site_name)).ToList();
        }


    }
 }
