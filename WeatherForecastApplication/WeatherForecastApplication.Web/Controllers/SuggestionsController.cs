using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Linq;
using WeatherForecastApplication.Data;
using WeatherForecastApplication.Web.Data;
using WeatherForecastApplication.Web.Models;

namespace WeatherForecastApplication.Web.Controllers
{
    public class SuggestionsController : Controller
    {
        private WeatherAppDbContext weatherAppDbContext;

        public SuggestionsController(WeatherAppDbContext weatherAppDbContext)
        {
            this.weatherAppDbContext = weatherAppDbContext;
        }

        public void FillResultQ(List<Joined_Result> combined_result, List<Joined_Result> results_Q) {
            for (int i = 0; i < results_Q.Count(); i++)
            {
                if (i > 0 && results_Q[i].City_Id == results_Q[i - 1].City_Id && results_Q[i].Date == results_Q[i - 1].Date &&
                    results_Q[i].Time == results_Q[i - 1].Time && results_Q[i].Site_name != results_Q[i - 1].Site_name)
                    continue;
                combined_result.Add(results_Q[i]);
            }
        }

        [HttpPost]
        public JsonResult Index([FromBody] string Email)
        {
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            var their_predictions = weatherAppDbContext.UserSiteCities.Where(usc => usc.Email.Equals(Email))
                .Join(weatherAppDbContext.CitySites, usc => new {usc.City_Id,usc.Site_Id},cs=> new {cs.City_Id,cs.Site_Id}, (usc, cs) =>
                new {
                    usc,
                    cs
                }).Join(weatherAppDbContext.Cities, res => res.cs.City_Id, c => c.City_Id, (res, c) => new
            {
               res.usc,
               res.cs,
               c
            }).Join(weatherAppDbContext.Sites, res => res.cs.Site_Id, si => si.Site_Id, (res, si) => new
            {
                res.usc,
                res.cs,
                res.c,
                si
            }).Join(weatherAppDbContext.Predictions, res => new { res.cs.City_Id, res.cs.Site_Id },
                pr => new { pr.City_Id, pr.Site_Id }, (res, pr) => new
                {
                    res.usc,
                    res.cs,
                    res.c,
                    res.si,
                    pr

                }).Join(weatherAppDbContext.Timeslots, res => res.pr.Timeslot_Id, ti => ti.Timeslot_Id, (res, ti) => new
            {
                res.cs.City_Id,
                res.cs.Site_Id,
                res.pr.Timeslot_Id,
                res.c.City_name,
                res.si.Site_name,
                res.pr.Weather,
                res.pr.Danger,
                //res.usc.People_Interested,
                ti.Time,
                ti.Date
            }).ToList();
            var interested_citysites = weatherAppDbContext.UserSiteCities.GroupBy(g => new { g.City_Id, g.Site_Id }).
                Select(g => new { g.Key.City_Id, g.Key.Site_Id, People_Interested = g.Count() }).ToList();
            var their_predictions_update = their_predictions.Join(interested_citysites, tp => new { tp.City_Id, tp.Site_Id },
                ics => new { ics.City_Id, ics.Site_Id }, (tp, isc) => new
                {
                    tp.City_Id,
                    tp.Site_Id,
                    tp.Timeslot_Id,
                    tp.City_name,
                    tp.Site_name,
                    tp.Weather,
                    tp.Danger,
                    isc.People_Interested,
                    tp.Time,
                    tp.Date

                });
            var their_site_ids = weatherAppDbContext.UserSiteCities.Where(usc => usc.Email.Equals(Email)).Select(usc => usc.Site_Id).ToHashSet();
            var average_ratings = weatherAppDbContext.Ratings.
                Where(ra=>their_site_ids.Contains(ra.Site_Id)).GroupBy(ra => new { ra.City_Id, ra.Site_Id }).
                 Select(g => new { g.Key.City_Id, g.Key.Site_Id, Average_Rating = g.Average(ra => ra.Rating_Value), Total_Ratings=g.Count() }).ToList();
            var max_r = average_ratings.GroupBy(ra => ra.City_Id).Select(g => new { City_Id=g.Key, Max_Rating = g.Max(ra => ra.Average_Rating) }).ToList();
            var max_ratings = max_r.Join(average_ratings, mr => new { mr.City_Id, mr.Max_Rating }, 
                ar => new { ar.City_Id, Max_Rating=ar.Average_Rating }, (mr, ar) =>
            new {
                mr.City_Id,
                mr.Max_Rating,
                ar.Total_Ratings,
                ar.Site_Id
            }).ToList();

            var final_predictions = their_predictions_update.Join(max_ratings, fp => new { fp.City_Id, fp.Site_Id }, mr => new { mr.City_Id, mr.Site_Id },
                (fp, mr) => new
                {
                    fp.City_Id,
                    fp.Site_Id,
                    fp.Timeslot_Id,
                    fp.City_name,
                    fp.Site_name,
                    fp.Weather,
                    fp.Danger,
                    fp.People_Interested,
                    fp.Time,
                    fp.Date,
                    mr.Max_Rating,
                    mr.Total_Ratings,
                }).ToList();
            var results_Q1 = final_predictions.Select(res => new
            Joined_Result(
                res.City_Id,
                res.Site_Id,
                res.Timeslot_Id,
                res.City_name,
                res.Site_name,
                res.Weather,
                res.Danger,
                res.People_Interested,
                res.Time,
                res.Date,
                res.Max_Rating,
                res.Total_Ratings))
                .OrderBy(res => res.City_name).ThenBy(res => res.Date).ThenBy(res => res.Time).ThenBy(res => res.Site_name).ToList();

            HashSet<int> their_city_ids = weatherAppDbContext.UserSiteCities.Where(usc => usc.Email.Equals(Email)).Select(c => c.City_Id).ToHashSet();
            HashSet<int> rated_city_ids = results_Q1.Select(c => c.City_Id).ToHashSet();
            HashSet<int> no_rated_city_ids=their_city_ids.Except(rated_city_ids).ToHashSet();
            var results_Q2 = their_predictions_update.Where(tp => no_rated_city_ids.Contains(tp.City_Id))
            .Select(res => new
            Joined_Result(
                res.City_Id,
                res.Site_Id,
                res.Timeslot_Id,
                res.City_name,
                res.Site_name,
                res.Weather,
                res.Danger,
                res.People_Interested,
                res.Time,
                res.Date,
                0.0,
                0)).
                OrderBy(res => res.City_name).ThenBy(res => res.Date).ThenBy(res => res.Time).ThenBy(res => res.Site_name).ToList();

            List<Joined_Result> combined_result=new List<Joined_Result>();
            FillResultQ(combined_result, results_Q1);
            FillResultQ(combined_result, results_Q2);
            return new JsonResult(combined_result.OrderBy(res => res.City_name).ThenBy(res => res.Date).ThenBy(res => res.Time));
        }
        
    }
}
