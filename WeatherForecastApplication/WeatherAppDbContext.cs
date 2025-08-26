using Microsoft.EntityFrameworkCore;
using System;

namespace WeatherForecastApplication.Data
{
	public class WeatherAppDbContext:DbContext
	{
		public WeatherAppDbContext(DbContextOptions <WeatherAppDbContext> options):base(options)
		{

		}
	}
}
