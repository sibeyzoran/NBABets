using Microsoft.AspNetCore.Mvc;
using NBABets.Services;
using Serilog;

namespace NBABets.API
{
    public static class ServiceCollectionExtensions
    {
        internal static IServiceCollection AddNBABetsApi(this IServiceCollection services)
        {
            // Add services required for the API
            services.AddSerilog();
            services.AddMappers();
            services.AddScoped<IUserAdapter, UsersAdapter>();
            services.AddScoped<IBetsAdapter, BetsAdapter>();
            services.AddScoped<IGameAdapter, GamesAdapter>();

            // This catches json input errors
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(e => e.Value.Errors.Count > 0)
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );

                    return new BadRequestObjectResult(new
                    {
                        Code = 400,
                        Message = "Validation errors occurred",
                        Errors = errors
                    });
                };
            });
            return services;
        }

        private static void AddMappers(this IServiceCollection services)
        {
            // find all types in the assembly that implement the IMapper interface
            var mapperInterfaceType = typeof(IMapper<,>);
            var mapperTypes = typeof(ServiceCollectionExtensions).Assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false } && t.GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == mapperInterfaceType))
                .ToList();

            foreach (var mapperType in mapperTypes)
            {
                // Get the actual mapper interface types
                var interfaceTypes = mapperType.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == mapperInterfaceType);

                foreach (var interfaceType in interfaceTypes)
                {
                    services.AddSingleton(interfaceType, mapperType);
                }
            }
        }
        
    }
}
