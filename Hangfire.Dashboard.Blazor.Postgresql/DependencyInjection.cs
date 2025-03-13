using Hangfire.Dashboard.Blazor.Core.Abstractions;
using Hangfire.Dashboard.Blazor.Postgresql.Context;
using Hangfire.Dashboard.Blazor.Postgresql.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hangfire.Dashboard.Blazor.Postgresql;

public static class DependencyInjection
{
    public static IServiceCollection AddHangfireDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddDbContext<HangfirePostgresqlContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("hangfire"));
        });

        return services;
    }
}