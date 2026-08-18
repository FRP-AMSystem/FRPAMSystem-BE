using FRPAMSystem.DataTier.Abstractions;
using FRPAMSystem.DataTier.Configuration;
using FRPAMSystem.DataTier.Interceptors;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Repository.Implement;
using FRPAMSystem.DataTier.Repository.Interfaces;
using FRPAMSystem.DataTier.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FRPAMSystem.DataTier
{
    public static class DataServiceRegistration
    {
        public static IServiceCollection AddDataServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.Configure<AppClockOptions>(
                configuration.GetSection(AppClockOptions.SectionName));

            services.AddSingleton<IClock, AppClock>();
            services.AddSingleton<AuditTimestampSaveChangesInterceptor>();

            services.AddDbContext<ForestryResourcePlanningDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(connectionString);
                options.AddInterceptors(
                    serviceProvider.GetRequiredService<AuditTimestampSaveChangesInterceptor>());
            });

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
