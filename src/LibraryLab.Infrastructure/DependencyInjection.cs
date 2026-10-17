using LibraryLab.Application.Contracts;
using LibraryLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryLab.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLibraryLabInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("LibraryLab")
            ?? "Server=localhost;Port=3306;Database=library_lab;User=library_lab;Password=library_lab;";
        var serverVersion = Version.Parse(configuration["Database:ServerVersion"] ?? "8.0.46");
        services.AddDbContext<LibraryLabDbContext>(options =>
            options.UseMySql(connectionString, new MySqlServerVersion(serverVersion)));
        services.AddScoped<ILibraryUserRepository, LibraryUserRepository>();
        services.AddScoped<ILibraryResourceRepository, LibraryResourceRepository>();
        services.AddScoped<ILoanRepository, LoanRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        return services;
    }
}
