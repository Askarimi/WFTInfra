using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Reflection;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Interfaces.UserManagment;
using WFT.Infra.Application.Helper;
using WFT.Infra.Application.Services;
using WFT.Infra.Application.Services.UserManagment;
using WFT.Infra.Application.Settings;

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

            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // تنظیمات JwtTokenGenerator
             services.AddSingleton<JwtTokenGenerator>();

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
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            #endregion

            return services;
        }
    }
}
