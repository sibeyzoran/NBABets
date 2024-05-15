using NBABets.UI.Components;
using Serilog;


namespace NBABets.UI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Setup Logger
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs\\UI.log");
            Log.Logger = new LoggerConfiguration()
                 .WriteTo.File(logPath)
                 .Enrich.FromLogContext()
                 .MinimumLevel.Error()
                 .CreateLogger();

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            builder.Services.AddNBABetsUIServices();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
