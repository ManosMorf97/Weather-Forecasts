using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherForecastApplication.Web.Models
{
    [PrimaryKey("Site_Id")]
    public class ForecastSite
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Site_Id { get; set; }
        public string Site_name { get; set; }
        public List<CitySite>? CitySites { get; set; }

        public ForecastSite(string Site_name ) { 
            this.Site_name = Site_name;
        }

    }
}
