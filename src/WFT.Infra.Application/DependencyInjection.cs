using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Helper;
using WFT.Infra.Application.InitialData;
using WFT.Infra.Application.Services;
using WFT.Infra.Application.Services.UserManagment;
using WFT.Infra.Contracts.Interfaces;

namespace WFT.Infra.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });

            services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));

            // تنظیمات و وابستگی‌های لایه Application (بدون ارجاع به زیرساخت)



            services.AddAuthorizationCore(options =>
            {
                // var serviceProvider = services.b;
                // فرض کنیم که PermissionService از دیتابیس Permission‌ها رو می‌خونه
                // var permissionService = services.BuildServiceProvider().GetService<PermissionService>();
                //   var permissions = permissionService.GetAllPermissionsAsync().Result;

                //foreach (var permission in permissions)
                //{
                //    options.AddPolicy($"Permission:{permission}", policy =>
                //    {
                //        policy.RequireClaim("permission", permission);
                //    });
                //}
            });

            #region Add Scoped
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<ITokenService, TokenService>();
            // ثبت IJwtTokenGenerator باید در لایه زیرساخت/Bootstrapper انجام شود
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IWorkContext, WorkContext>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserPasswordService, UserPasswordService>();
            
            // ABAC Services
            services.AddScoped<IABACService, ABACService>();
            services.AddScoped<IPolicyRuleService, PolicyRuleService>();
            services.AddScoped<IAttributeService, AttributeService>();
            services.AddScoped<IConditionOperatorService, ConditionOperatorService>();
            services.AddScoped<IAttributeGroupService, AttributeGroupService>();
            
            // Authorization Service
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            // services.AddScoped(typeof(IPagedList<>), typeof(PagedList<>));
            services.AddScoped<InitialDataSeeder>();

            #endregion

            return services;
        }
    }
}
