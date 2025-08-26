using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherForecastApplication.Web.Models
{
//    [PrimaryKey("city_id,site_Id")]
    public class CitySite: IEquatable<CitySite>
    {
        public int City_Id { get; set; }
        public int Site_Id { get; set; }

        public List<UserSiteCity> ?UserSiteCities { get; set; }
        public List<Prediction>? Predictions { get; set; }

        [ForeignKey("City_Id")]
        public City? City_ { get; set; }
        [ForeignKey("Site_Id")]
        public ForecastSite? Site { get; set; }

        public CitySite(int City_Id=-1,int Site_Id = -1) {
            this.City_Id = City_Id;
            this.Site_Id = Site_Id;
        }

        public bool Equals(CitySite? other)
        {
            if (other == null)
                return false;
            if (other== null)
                return false;
            return this.City_Id == other.City_Id && this.Site_Id == other.Site_Id;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(City_Id, Site_Id);
        }
    }
}
