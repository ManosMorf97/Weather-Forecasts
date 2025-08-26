using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherForecastApplication.Web.Models
{
    public class UserNotification
    {
        public string? Email { get; set; }
        public int City_Id { get; set; }
        public int Site_Id { get; set; }
        public int Timeslot_Id { get; set; }
        public bool IsRead { get; set; }
        [ForeignKey("Email")]
        public User? User_ { get; set; }
        public Notification? Notification_ { get; set; }
    }
}
