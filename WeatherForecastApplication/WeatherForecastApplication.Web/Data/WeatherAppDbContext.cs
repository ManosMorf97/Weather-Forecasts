using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Diagnostics;
using WeatherForecastApplication.Web.Models;

namespace WeatherForecastApplication.Data
{
	public class WeatherAppDbContext:DbContext
	{
		public WeatherAppDbContext(DbContextOptions <WeatherAppDbContext> options):base(options)
		{

		}
		public DbSet<User> Users { get; set; }
		public DbSet<City> Cities { get; set; }
		public DbSet<ForecastSite> Sites { get; set; }
		public DbSet<CitySite> CitySites { get; set; }
		public DbSet<UserSiteCity> UserSiteCities { get; set; }
		public DbSet<Rating> Ratings { get; set; }
		public DbSet<Prediction> Predictions { get; set; }
		public DbSet<Timeslot> Timeslots { get; set; }
		public DbSet<Notification> Notifications { get; set; }
		public DbSet<UserNotification> UserNotifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
			modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
			modelBuilder.Entity<City>().HasIndex(c => c.City_name).IsUnique();
			modelBuilder.Entity<ForecastSite>().HasIndex(fs => fs.Site_name).IsUnique();
            modelBuilder.Entity<CitySite>().HasKey(cs => new { cs.City_Id, cs.Site_Id });
            modelBuilder.Entity<UserSiteCity>().HasKey(usc => new { usc.Email, usc.City_Id, usc.Site_Id });
            modelBuilder.Entity<UserSiteCity>().HasOne(usc => usc.CitySite_).
				WithMany(sc => sc.UserSiteCities).HasForeignKey(usc => new { usc.City_Id,usc.Site_Id});
			modelBuilder.Entity<Rating>().HasKey(ra => new { ra.Email, ra.City_Id, ra.Site_Id, ra.Timeslot_Id });
			modelBuilder.Entity<Rating>().HasOne(ra => ra.Prediction_).
				WithMany(pr => pr.Ratings).HasForeignKey(ra => new { ra.City_Id, ra.Site_Id, ra.Timeslot_Id });
			modelBuilder.Entity<Prediction>().HasKey(pr => new { pr.City_Id, pr.Site_Id, pr.Timeslot_Id });
			modelBuilder.Entity<Prediction>().HasOne(pr => pr.CitySite_).WithMany(cs => cs.Predictions).
				HasForeignKey(pr => new { pr.City_Id, pr.Site_Id });
           
			modelBuilder.Entity<Notification>().HasKey(notif => new { notif.Site_Id, notif.City_Id, notif.Timeslot_Id });
			modelBuilder.Entity<Notification>().HasOne(notif => notif.Prediction_).WithOne(pr => pr.Notification_).
				HasForeignKey<Notification>(notif => new { notif.City_Id, notif.Site_Id, notif.Timeslot_Id});
			modelBuilder.Entity<UserNotification>().HasKey(usno => new { usno.Email, usno.City_Id, usno.Site_Id, usno.Timeslot_Id });
			modelBuilder.Entity<UserNotification>().HasOne(usno => usno.Notification_).
				WithMany(notif => notif.UserNotifications_).
				HasForeignKey(usno => new { usno.Site_Id,usno.City_Id, usno.Timeslot_Id });
            modelBuilder.Entity<Timeslot>().HasIndex(ts=> new { ts.Date,ts.Time }).IsUnique();
            base.OnModelCreating(modelBuilder);

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
			
        }

    }
}
