using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WeatherForecastApplication.Web.Models;

namespace WeatherForecastApplication.Web.Controllers
{
    
    public class FirstController : Controller
    {
        [HttpGet("Hello")]
        public String Indexa()
        {
            Debug.WriteLine("BOO");
            return "Hello";
        }
        [HttpPost("HelloPost")]
       public IActionResult HelloPost([FromBody] DataPutter dataPutter)
        {
          
            Debug.WriteLine(dataPutter);
            return Ok(dataPutter);
        }
        public String Index()
        {
            Debug.WriteLine("BOO");
            return "HelloINDEX";
        }
    }
    
}
