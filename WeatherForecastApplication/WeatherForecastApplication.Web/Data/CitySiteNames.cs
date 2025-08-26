namespace WeatherForecastApplication.Web.Data
{
    public class CitySiteNames
    {
        public string City_name { get; set; }
        public string Site_name { get; set; }

        public CitySiteNames(string city_name, string site_name  )
        {
            City_name = city_name;
            Site_name = site_name;
        }
    }
}
