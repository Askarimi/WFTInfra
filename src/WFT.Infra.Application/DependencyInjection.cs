using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Services;

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



            #region Add Services
            services.AddScoped<ICountryService, CountryService>();

            #endregion


            return services;
        }
    }
}
