using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherForecastApplication.Web.Models
{
    [PrimaryKey("Timeslot_Id")]
    public class Timeslot
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Timeslot_Id { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly Time { get; set; }
        public List<Prediction>? Predictions { get; set; }
    }
}
