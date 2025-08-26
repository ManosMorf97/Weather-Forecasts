using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherForecastApplication.Web.Models
{
    public class UserSiteCity
    {
        
        public string Email { get; set; }
        public int City_Id { get; set; }
        public int Site_Id { get; set; }

        public CitySite? CitySite_ { get; set; }

        [ForeignKey("Email")]
        public User? User_ { get; set; }

        public UserSiteCity(string Email, int City_Id, int Site_Id)
        {
            this.Email = Email;
            this.City_Id = City_Id;
            this.Site_Id = Site_Id;
        }
    }
}
