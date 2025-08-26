using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherForecastApplication.Web.Models
{
    public class Rating : IEquatable<Rating>
    {
        public string? Email { get; set; }
        public int City_Id { get; set; }
        public int Site_Id { get; set; }
        public int Timeslot_Id { get; set; }
        public int Rating_Value{ get; set; }

        [ForeignKey("Email")]
        public User? User_ { get; set; }
        public Prediction? Prediction_ { get; set; }

        public override int GetHashCode()
        {
            return HashCode.Combine(Email,City_Id,Site_Id, Timeslot_Id);
        }
        public bool Equals(Rating? other)
        {
            if (other == null|| this.Email==null)
                return false;
            return this.Email.Equals(other.Email) && this.City_Id == other.City_Id
            && this.Site_Id == other.Site_Id && this.Timeslot_Id == other.Timeslot_Id;
        }
    }
}
