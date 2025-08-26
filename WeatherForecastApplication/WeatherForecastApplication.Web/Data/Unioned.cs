using WeatherForecastApplication.Web.Models;

namespace WeatherForecastApplication.Web.Data
{
    public class Unioned: IEquatable<Unioned>
    {
       

        public double Rating_Value { get; }
        public int Total_People { get; }
        public double Percentage { get; }
        public string? City_name { get; }
        public string? Site_name { get; }
        public DateOnly Date { get; }
        public TimeOnly Time { get; }
        public string? Weather { get; }
        public bool Danger { get; }

        public Unioned(double rating_Value, int total_People, double percentage, 
            string? cityName, string? site_Name, DateOnly date, TimeOnly time,
            string? weather, bool danger)
        {
            Rating_Value = rating_Value;
            Total_People = total_People;
            Percentage = percentage;
            City_name = cityName;
            Site_name = site_Name;
            Date = date;
            Time = time;
            Weather = weather;
            Danger = danger;
        }

      

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Rating_Value);
            hash.Add(Total_People);
            hash.Add(Percentage);
            hash.Add(City_name);
            hash.Add(Site_name);
            hash.Add(Date);
            hash.Add(Time);
            hash.Add(Weather);
            hash.Add(Danger);
            return hash.ToHashCode();
        }

        public bool Equals(Unioned? obj)
        {
            return obj is Unioned unioned &&
                   Rating_Value == unioned.Rating_Value &&
                   Total_People == unioned.Total_People &&
                   Percentage == unioned.Percentage &&
                   City_name == unioned.City_name &&
                   Site_name == unioned.Site_name &&
                   Date.Equals(unioned.Date) &&
                   Time.Equals(unioned.Time) &&
                   Weather == unioned.Weather &&
                   Danger == unioned.Danger;
        }
    }
}
