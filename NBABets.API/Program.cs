using NBABets.Services;
using Serilog;

namespace NBABets.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Setup Logger
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs\\API.log");
            Log.Logger = new LoggerConfiguration()
                 .WriteTo.File(logPath)
                 .Enrich.FromLogContext()
                 .MinimumLevel.Debug()
                 .CreateLogger();

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });

            builder.Services.AddNBABetsApi();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            // Setup database + tables if it hasn't been created
            using (AppDBContext dbContext = new AppDBContext())
            {
                dbContext.Database.EnsureCreated();
            }

            app.Run();
        }
    }
}
