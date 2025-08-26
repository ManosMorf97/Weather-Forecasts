namespace WeatherForecastApplication.Web.Data
{
    public class TimeSetting
    {
        private static DateOnly today_date = DateOnly.FromDateTime(DateTime.Now);
        private static TimeOnly today_time = TimeOnly.FromDateTime(DateTime.Now);

        public static void ChangeTimer(DateOnly today_date_,TimeOnly today_time_)
        {
            today_date = today_date_;
            today_time = today_time_;
        }
        public static void SetDefaultSettings()
        {
            today_date = DateOnly.FromDateTime(DateTime.Now);
            today_time = TimeOnly.FromDateTime(DateTime.Now);
        }
        public static DateOnly getDate()
        {
            return today_date;
        }
        public static TimeOnly getTime()
        {
            return today_time;
        }
    }
}
