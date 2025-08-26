namespace WeatherForecastApplication.Web.Data
{
    public class SignUpInput
    {
        public string? Email { get; set; }
        public string? Hashed_password { get; set; }
        public string? Username { get; set; }
        public string[]? CityNames { get; set; }
        public string[]? SiteNames { get; set; }

       public bool HasNull()
        {
            return Email == null || Hashed_password==null || Username==null;
        }
        
    }
}
