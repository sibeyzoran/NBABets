using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.FluentUI.AspNetCore.Components;
using NBABets.Client;
using Serilog;

namespace NBABets.UI.Components
{
    public static class ServiceCollectionExtensions
    {
        internal static IServiceCollection AddNBABetsUIServices(this IServiceCollection services)
        {
            // Add services required for the API
            services.AddSerilog();
            services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
                .AddNegotiate();
            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = options.DefaultPolicy;
            });

            services.AddFluentUIComponents();
            services.AddHttpClient();
            services.AddScoped<UserClient>();
            services.AddScoped<BetClient>();
            services.AddScoped<GameClient>();
            services.AddSingleton<ApplicationState>();

            return services;
        }
    }
}
