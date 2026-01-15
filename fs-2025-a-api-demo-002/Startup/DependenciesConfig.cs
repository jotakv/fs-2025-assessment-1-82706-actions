using fs_2025_a_api_demo_002.Data;
using fs_2025_a_api_demo_002.Services;

namespace fs_2025_a_api_demo_002.Startup
{
    public static class DependenciesConfig
    {
        public static void AddDependencies(this WebApplicationBuilder builder)
        {
            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton<BikeData>();
            builder.Services.AddSingleton<StationService>();
            builder.Services.AddHostedService<RandomBikeUpdateService>();
        }
    }
}
