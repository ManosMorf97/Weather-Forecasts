using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol;
using WeatherForecastApplication.Data;
using WeatherForecastApplication.Web.Models;

namespace WeatherForecastApplication.Web.Controllers
{
    public class PredictionsController : Controller
    {
        private readonly WeatherAppDbContext _context;

        public PredictionsController(WeatherAppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public JsonResult UserInfo([FromBody] string email)
        {
            var their_city_sites = _context.UserSiteCities.Where(usc => usc.Email.Equals(email)).
                Select(usc=> new { usc.City_Id, usc.Site_Id })
                .Join(_context.CitySites,
                usc => new { usc.City_Id, usc.Site_Id }, cs => new { cs.City_Id, cs.Site_Id },
                (usc, cs) => new { usc,cs})
                .Join(_context.Cities, joined => joined.cs.City_Id, city => city.City_Id,
                (joined, city) => new { joined.usc,joined.cs,city })
                .Join(_context.Sites, joined => joined.cs.Site_Id, site => site.Site_Id,
                (joined, site) => new { joined.usc, joined.cs, joined.city, site }).ToList();
            var their_prediction_timeslot = _context.Predictions.Join(_context.Timeslots, pr => pr.Timeslot_Id, ti => ti.Timeslot_Id,
                (prediction, timeslot) => new { prediction, timeslot}).ToList();
            var their_predictions = their_city_sites.Join(their_prediction_timeslot,
                joined => new { joined.cs.City_Id, joined.cs.Site_Id }, joined2 => new { joined2.prediction.City_Id, joined2.prediction.Site_Id },
                (joined,joined2) => new
                {
                    joined.cs.City_Id,
                    joined.cs.Site_Id,
                    joined.city.City_name,
                    joined.site.Site_name,
                    joined2.timeslot.Timeslot_Id,
                    joined2.timeslot.Date,
                    joined2.timeslot.Time,
                    joined2.prediction.Weather,
                    joined2.prediction.Danger,
                    Rating_Value = 0
                }).ToList();
            var their_ratings = _context.Ratings.Where(ra => ra.Email.Equals(email)).ToList();
            if (their_ratings.Count()==0)
                return new JsonResult(their_predictions.OrderBy(cisi => cisi.Site_name).
                    ThenBy(cisi => cisi.City_name).ThenBy(cisi => cisi.Date).ThenBy(cisi => cisi.Time).ToList());
            var their_predictions2 = their_predictions.GroupJoin(their_ratings, tp => new { tp.City_Id, tp.Site_Id, tp.Timeslot_Id },
                tr => new { tr.City_Id, tr.Site_Id, tr.Timeslot_Id }, (tp, tr) => new { tp, tr }).ToList();
            //Trace.WriteLine
            var their_predictions3 = their_predictions2.Select(tp2 => new
            {
                tp2.tp.City_Id,
                tp2.tp.Site_Id,
                tp2.tp.City_name,
                tp2.tp.Site_name,
                tp2.tp.Timeslot_Id,
                tp2.tp.Date,
                tp2.tp.Time,
                tp2.tp.Weather,
                tp2.tp.Danger,
                Rating_Value = !tp2.tr.IsNullOrEmpty() ? tp2.tr.First().Rating_Value : tp2.tp.Rating_Value
            });
            return new JsonResult(their_predictions3.OrderBy(res => res.Site_name).
                ThenBy(res => res.City_name).ThenBy(res => res.Date).ThenBy(res => res.Time).ToList());
        }
        [HttpPost]
        public async Task<IActionResult> AddRating([FromBody] Rating rating)
        {
            if (!ModelState.IsValid)
                return BadRequest("We are sorry,something has happened.\nTry again later");
            try {
                if (rating.Rating_Value<1||rating.Rating_Value>5)
                    return StatusCode(406, "Rating must be integer from 1 to 5");
                Rating? rating_to_DB = await _context.Ratings.FirstOrDefaultAsync(ra => ra.Email == rating.Email &&
                ra.City_Id == rating.City_Id && ra.Site_Id == rating.Site_Id && ra.Timeslot_Id == rating.Timeslot_Id);
                if (rating_to_DB == null)
                {
                    await _context.Ratings.AddAsync(rating);
                    await _context.SaveChangesAsync();
                    return Ok("Your rating has been submitted");
                }
                else
                {
                        await _context.Ratings.Where(ra => ra.Equals(rating_to_DB)).
                            ExecuteUpdateAsync(record => record.SetProperty(ra => ra.Rating_Value, ra => rating.Rating_Value));
                    
                    return Ok("Your rating has been updated");
                }
            }
            catch(InvalidCastException e) {
                return StatusCode(406, "Rating must be integer from 1 to 5");
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteRating([FromBody] Rating rating)
        {
            await _context.Ratings.Where(ra=>ra.Equals(rating)).ExecuteDeleteAsync();
            await _context.SaveChangesAsync();
            return Ok("Rating has been deleted");

        }
    }
}
