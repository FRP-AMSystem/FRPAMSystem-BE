using FRPAMSystem.BusinessTier.Services.Implements;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FRPAMSystem.BusinessTier
{
    public static class BusinessServiceRegistration
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}
