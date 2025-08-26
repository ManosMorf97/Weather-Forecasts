namespace WeatherForecastApplication.Web.Data
{
    public class SignInInput
    {
        public string? Email_Username { get; set; }
        public string? HashedPassword { get; set; }

        public bool HasNull()
        {
            return Email_Username == null  || HashedPassword == null || Email_Username.Length==0 || HashedPassword.Length==0;
        }
    }
}
