namespace WeatherForecastApplication.Web.Models
{
    public class Notification
    {
        public int Site_Id { get; set; }
        public int City_Id { get; set; }
        public int Timeslot_Id { get; set; }
        public string? Description { get; set; }
        public DateOnly DateNotification { get; set; }
        public TimeOnly TimeNotification { get; set; }
        public Prediction? Prediction_ { get; set; }

        public List<UserNotification>? UserNotifications_ { get; set; }
    }
}
