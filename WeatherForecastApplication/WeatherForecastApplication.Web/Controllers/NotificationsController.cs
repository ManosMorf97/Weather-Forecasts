using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeatherForecastApplication.Data;
using WeatherForecastApplication.Web.Data;
using WeatherForecastApplication.Web.Models;

namespace WeatherForecastApplication.Web.Controllers
{
    
    public class NotificationsController : Controller
    {
        private WeatherAppDbContext _context;

        public NotificationsController(WeatherAppDbContext context) { 
            _context = context;
        }
        [HttpPost]
        public int GetCountNotifications([FromBody] string? email) {
            return _context.UserNotifications.Where(un => un.Email.Equals(email) && !un.IsRead).Count();
        }

        [HttpPost]
        public JsonResult Index([FromBody] string? email)
        {
            return new JsonResult(_context.UserNotifications.Where(un => un.Email.Equals(email) && !un.IsRead).
                Join(_context.Notifications, un => new { un.City_Id, un.Site_Id, un.Timeslot_Id },
                notif => new { notif.City_Id, notif.Site_Id, notif.Timeslot_Id }, 
                (un, notif) =>
                new {
                    un,notif
                }).Join(_context.Cities, joined => joined.notif.City_Id, city => city.City_Id,
                (joined, city) => new { joined.un,joined.notif, city })
                .Join(_context.Sites, joined => joined.notif.Site_Id, site => site.Site_Id,
                (joined, site) => new { joined.un, joined.notif, joined.city, site }).
                Join(_context.Timeslots, joined => joined.notif.Timeslot_Id, ti => ti.Timeslot_Id,
                (joined, timeslot) => new { joined.un, joined.notif, joined.city, joined.site,timeslot }).
                OrderBy(joined=>joined.notif.DateNotification).ThenBy(joined=> joined.notif.TimeNotification).ThenBy(joined=>joined.notif.Description).
                Select(joined=>new { joined.site.Site_name,joined.city.City_name,
                    joined.timeslot.Date,joined.timeslot.Time,joined.notif.Description }).ToList());
        }

        [HttpPost]
        public async Task<IActionResult> ReadNotifications([FromBody] string? email)
        {
            try
            {
                await _context.UserNotifications.Where(un => un.Email.Equals(email) && !un.IsRead).
                     ExecuteUpdateAsync(rec => rec.SetProperty(rec => rec.IsRead, true));
                return Ok("All Readed");
            }catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        } 
    }
}
