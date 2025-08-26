using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WeatherForecastApplication.Data;
using WeatherForecastApplication.Web.Controllers;

public class Program
{
    public static void Main(string[] args)
    {
        Debug.WriteLine("Begib");
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        // Add services to the container.
        Trace.WriteLine(builder.Environment.EnvironmentName);
        Trace.WriteLine("POOOOOOOOOOOOOOOOOOO" + builder.Environment.IsEnvironment("Testing"));
        Debug.WriteLine(builder.Environment.EnvironmentName);
        Debug.WriteLine("POOOOOOOOOOOOOOOOOOO" + builder.Environment.IsEnvironment("Testing"));
        builder.Services.AddControllersWithViews();
        builder.Services.AddDbContext<WeatherAppDbContext>(options => options.
        UseSqlServer(builder.Configuration.GetConnectionString("WeatherApplicationDBConnectionString")).
        EnableSensitiveDataLogging().LogTo(s => Debug.WriteLine(s), LogLevel.Information));

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        //app.UseDefaultFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
        Debug.WriteLine("Begib");
    }
}