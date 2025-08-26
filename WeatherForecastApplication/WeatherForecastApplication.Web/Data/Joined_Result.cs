namespace WeatherForecastApplication.Web.Data
{
    public class Joined_Result
    {
        public Joined_Result(int city_Id, int site_Id, int timeslot_Id, string? city_name, string? site_name,
            string? weather, bool danger, int people_Interested, TimeOnly time, DateOnly date,
            double max_Rating, int total_Ratings)
        {
            City_Id = city_Id;
            Site_Id = site_Id;
            Timeslot_Id = timeslot_Id;
            City_name = city_name;
            Site_name = site_name;
            Weather = weather;
            Danger = danger;
            People_Interested = people_Interested;
            Time = time;
            Date = date;
            Max_Rating = max_Rating;
            Total_Ratings = total_Ratings;
        }

        public int City_Id { get; }
        public int Site_Id { get; }
        public int Timeslot_Id { get; }
        public string? City_name { get; }
        public string? Site_name { get; }
        public string? Weather { get; }
        public bool Danger { get; }
        public int People_Interested { get; }
        public TimeOnly Time { get; }
        public DateOnly Date { get; }
        public double Max_Rating { get; }
        public int Total_Ratings { get; }
    }
}
