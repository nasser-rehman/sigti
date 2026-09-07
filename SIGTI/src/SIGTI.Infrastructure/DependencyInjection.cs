using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SIGTI.Application.Common.Interfaces.Persistence;
using SIGTI.Application.Common.Interfaces.Services;
using SIGTI.Domain.Factories;
using SIGTI.Infrastructure.Authentication;
using SIGTI.Infrastructure.Persistence.Context;
using SIGTI.Infrastructure.Persistence.Queries;
using SIGTI.Infrastructure.Persistence.Repositories;
using SIGTI.Infrastructure.Persistence.Seed;
using SIGTI.Infrastructure.Services;

namespace SIGTI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        //DbContext

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection")
            )
        );

        // Options
        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName)
        );

        //Unit Of Work
        services.AddScoped<IUnitOfWork>(sp =>
            sp.GetRequiredService<ApplicationDbContext>()
        );

        //Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<ISupportQueueRepository, SupportQueueRepository>();

        //Queries
        services.AddScoped<ITechnicianWorkloadQuery, TechnicianWorkloadQuery>();

        //Services
        services.AddScoped<ITicketNumberGenerator, TicketNumberGenerator>();

        // Security Services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        //Factories
        services.AddSingleton<TicketFactory>();

        return services;
    }
}
