namespace AllergyScan.DAL.Extensions
{

    public static class Registrator
    {
        public static IServiceCollection RegisterDataAcessServices(this IServiceCollection services)
        {
            services.RegisterContexts();
            services.RegisterRepositories();
            services.RegisterUnitOfWork();
            return services;
        }
        private static IServiceCollection RegisterContexts(this IServiceCollection services)
        {

            services.AddDbContext<GameContext>(options => options.UseSqlServer("name=ConnectionStrings:GameStore"));

            return services;
        }

        private static IServiceCollection RegisterRepositories(this IServiceCollection services)
        {

            services.AddScoped<IPersonRepository, PersonRepository>();
            services.AddScoped<IStoreRepository, StoreRepository>();
            return services;
        }

        private static IServiceCollection RegisterUnitOfWork(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services;
        }
    }
}
