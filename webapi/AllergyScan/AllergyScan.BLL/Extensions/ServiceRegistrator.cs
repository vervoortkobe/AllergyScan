using MyGameStore.BLL.Interfaces;
using MyGameStore.BLL.Services;

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
