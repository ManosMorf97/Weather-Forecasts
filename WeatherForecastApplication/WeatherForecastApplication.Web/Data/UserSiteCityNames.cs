namespace WeatherForecastApplication.Web.Data
{
    public class UserSiteCityNames
    {
        public string Email { get; set; }
        public string Site_name { get; set; }
        public string City_name { get; set; }

        public UserSiteCityNames(string Email, string Site_name,string City_name)
        {
            this.Email=Email;
            this.Site_name=Site_name;
            this.City_name=City_name;

        }

    }
}
