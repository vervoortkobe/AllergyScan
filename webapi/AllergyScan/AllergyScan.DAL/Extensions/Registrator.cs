using AllergyScan.DAL.Contexts;
using AllergyScan.DAL.Repositories;
using AllergyScan.DAL.UOW;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AllergyScan.DAL.Extensions
{
    public static class Registrator
    {
        public static IServiceCollection RegisterDataAccessServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AllergyScanContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("Default") ?? "Data Source=allergyscan.db"));

            services.AddScoped<IAllergyRepository, AllergyRepository>();
            services.AddScoped<ILanguageRepository, LanguageRepository>();
            services.AddScoped<IUserSettingsRepository, UserSettingsRepository>();
            services.AddScoped<IScanHistoryRepository, ScanHistoryRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
