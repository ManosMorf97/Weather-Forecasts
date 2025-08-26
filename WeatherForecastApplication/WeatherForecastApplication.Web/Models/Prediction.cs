using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherForecastApplication.Web.Models
{
    public class Prediction
    {
        public int City_Id { get; set; }
        public int Site_Id {get; set;}
        public int Timeslot_Id { get; set; }
        public string? Weather { get; set; }
        public bool Danger {  get; set; }

        public List<Rating>? Ratings { get; set; }
        [ForeignKey("Timeslot_Id")]
        public Timeslot? Timeslot_ { get; set; }
        public Notification? Notification_ { get; set; }
        public CitySite? CitySite_ { get; set; }

    }
}
