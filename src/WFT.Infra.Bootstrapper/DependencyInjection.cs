using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WFT.Infra.Application;
using WFT.Infra.Infrastructure;

namespace WFT.Infra.Bootstrapper
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddProjectDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplication();      // لایه Application
            services.AddInfrastructure(configuration);  // لایه Infrastructure
            return services;
        }
    }
}
