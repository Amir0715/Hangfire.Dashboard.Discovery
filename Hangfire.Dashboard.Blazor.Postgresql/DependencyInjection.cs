using Hangfire.Dashboard.Blazor.Postgresql.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hangfire.Dashboard.Blazor.Postgresql;

public static class DependencyInjection
{
    public static IServiceCollection AddHangfireDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<HangfireContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("hangfire"));
        });

        return services;
    }
}