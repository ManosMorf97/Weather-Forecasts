using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherForecastApplication.Web.Models
{
    [PrimaryKey("City_Id")]
    public class City: IEquatable<City>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int City_Id { get; }
        public string City_name { get; set; }
        public List<CitySite>? CitySites { get; set; }

        public City(string City_name) {
            this.City_name = City_name;
        }

        public bool Equals(City? other)
        {
            if (other == null)
                return false;
            if (other.City_name == null || this.City_name==null)
                return false;
            return this.City_name.Equals(other.City_name);
        }
        public override int GetHashCode()
        {
            return City_name?.GetHashCode() ?? 0;
        }
    }
}
