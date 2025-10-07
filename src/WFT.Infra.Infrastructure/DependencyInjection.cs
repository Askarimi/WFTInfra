using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WFT.Infra.Application.Contracts.Interfaces;
using WFT.Infra.Application.Contracts.Repositories;
using WFT.Infra.Infrastructure.Security;
using WFT.Infra.Infrastructure.Data;
using WFT.Infra.Infrastructure.Repositories;

namespace WFT.Infra.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();

            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            // اتصال به دیتابیس با استفاده از ApplicationDbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));


            //// اجرای خودکار مایگریشن‌ها هنگام شروع پروژه
            //using (var serviceProvider = services.BuildServiceProvider())
            //{
            //    var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            //    dbContext.Database.Migrate();  // اجرای خودکار مایگریشن‌ها
            //}

            return services;
        }
    }
}