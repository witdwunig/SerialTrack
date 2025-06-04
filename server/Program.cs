using System;
using Serilog;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Services;

internal static class Program
{
    [STAThread]
    static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File("Log/server-log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseSerilog();

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Listen(IPAddress.Any, 5000);
        });
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var exeLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var dbPath = Path.Combine(exeLocation!, "serialnumbers.db");
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}")
        );

        builder.Services.AddHostedService<UdpBroadcastService>();    
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate(); // Ensures DB file and schema exist
        }   

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        // Start the web app but don’t block main thread
        var webAppTask = app.RunAsync();

        // Setup tray icon
        string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string iconPath = Path.Combine(exeDir!, "favicon.ico");
        using var trayIcon = new NotifyIcon()
        {
            Icon = new System.Drawing.Icon(iconPath),
            Text = "SerialTrack",
            Visible = true
        };

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Exit", null, async (sender, e) =>
        {
            trayIcon.Visible = false;
            await app.StopAsync();
            Application.Exit();
        });
        trayIcon.ContextMenuStrip = contextMenu;

        Application.Run();

        await webAppTask;
    }
}
