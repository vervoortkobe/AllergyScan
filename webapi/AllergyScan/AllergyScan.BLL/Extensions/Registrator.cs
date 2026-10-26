using AllergyScan.BLL.Interfaces;
using AllergyScan.BLL.Options;
using AllergyScan.BLL.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AllergyScan.BLL.Extensions
{
    public static class Registrator
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ScanOptions>(configuration.GetSection("Scan"));

            services.AddScoped<IAllergyService, AllergyService>();
            services.AddScoped<ILanguageService, LanguageService>();
            services.AddScoped<IScanService, ScanService>();

            return services;
        }
    }
}
