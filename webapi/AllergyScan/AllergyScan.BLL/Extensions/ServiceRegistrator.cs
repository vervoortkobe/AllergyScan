using AllergyScan.BLL.Interfaces;
using AllergyScan.BLL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AllergyScan.BLL.Extensions
{
    public static class ServiceRegistrator
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            services.AddScoped<IPerson, PersonService>();
            services.AddScoped<IStore, StoreService>();

            return services;
        }
    }
}
