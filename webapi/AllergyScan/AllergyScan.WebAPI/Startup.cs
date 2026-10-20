using System.Text.Json.Serialization;
using AllergyScan.BLL.Extensions;
using AllergyScan.DAL.Contexts;
using AllergyScan.DAL.Extensions;
using AllergyScan.WebAPI.Middleware;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

namespace AllergyScan.WebAPI
{
    public class Startup
    {
        private const long MaxUploadBytes = 15 * 1024 * 1024;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "AllergyScan API",
                    Version = "v1",
                    Description = "Single-user backend for the AllergyScan MAUI app."
                });
            });

            services.AddProblemDetails();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy => policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });

            services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = MaxUploadBytes);
            services.Configure<KestrelServerOptions>(options => options.Limits.MaxRequestBodySize = MaxUploadBytes);

            services.RegisterDataAccessServices(Configuration);
            services.RegisterServices(Configuration);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
        {
            EnsureDatabaseDirectory(environment);

            using (var scope = app.ApplicationServices.CreateScope())
            {
                var database = scope.ServiceProvider.GetRequiredService<AllergyScanContext>();
                database.Database.Migrate();
            }

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "AllergyScan API v1");
            });

            app.UseRouting();
            app.UseCors("AllowAll");
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void EnsureDatabaseDirectory(IWebHostEnvironment environment)
        {
            var connectionString = Configuration.GetConnectionString("Default");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return;
            }

            var dataSource = new SqliteConnectionStringBuilder(connectionString).DataSource;

            if (string.IsNullOrWhiteSpace(dataSource)
                || dataSource.Equals(":memory:", StringComparison.OrdinalIgnoreCase)
                || dataSource.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var path = Path.IsPathRooted(dataSource)
                ? dataSource
                : Path.Combine(environment.ContentRootPath, dataSource);

            var directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
