using FRPAMSystem.BusinessTier.Configuration;
using FRPAMSystem.BusinessTier.Services.Implements;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FRPAMSystem.BusinessTier
{
    public static class BusinessServiceRegistration
    {
        public static IServiceCollection AddBusinessServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection("Email"));

            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ISkillService, SkillService>();
            services.AddScoped<IAreaService, AreaService>();
            services.AddScoped<IEquipmentCategoryService, EquipmentCategoryService>();
            services.AddScoped<IEquipmentTypeService, EquipmentTypeService>();
            services.AddScoped<IEquipmentInstanceService, EquipmentInstanceService>();
            services.AddScoped<IExperimentService, ExperimentService>();
            services.AddScoped<IExperimentPhaseService, ExperimentPhaseService>();
            services.AddScoped<ILandResourceService, LandResourceService>();
            services.AddScoped<IExperimentLandRequirementService, ExperimentLandRequirementService>();
            services.AddScoped<IExperimentEquipmentRequirementService, ExperimentEquipmentRequirementService>();
            services.AddScoped<IExperimentHumanRequirementService, ExperimentHumanRequirementService>();
            services.AddScoped<IHumanResourceProfileService, HumanResourceProfileService>();
            services.AddScoped<IHumanResourceSkillService, HumanResourceSkillService>();
            services.AddScoped<IAllocationPlanService, AllocationPlanService>();
            services.AddScoped<IAllocationLandDetailService, AllocationLandDetailService>();
            services.AddScoped<IAllocationEquipmentDetailService, AllocationEquipmentDetailService>();
            services.AddScoped<IAllocationHumanDetailService, AllocationHumanDetailService>();
            services.AddScoped<IEquipmentShortageLogService, EquipmentShortageLogService>();
            services.AddScoped<IEquipmentSubstitutionService, EquipmentSubstitutionService>();
            services.AddScoped<IScheduleService, ScheduleService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<INotificationService, NotificationService>();
            return services;
        }
    }
}
