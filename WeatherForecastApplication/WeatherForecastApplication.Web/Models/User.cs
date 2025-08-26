using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WeatherForecastApplication.Web.Models
{
    [PrimaryKey("Email")]
    public class User
    {
        public string Email { get; set; }
        public string Hashed_password { get; set; }
        [Required]
        public string Username { get; set; }

        public List<UserSiteCity>? UserSiteCities { get; set; }
        public List<Rating>? Ratings { get; set; }
        public List<UserNotification>? UserNotification { get; set; }
        
        public User(string Email,string Hashed_password,string Username)
        {
            this.Email = Email;
            this.Hashed_password = Hashed_password;
            this.Username = Username;
        }

       
    }
}
