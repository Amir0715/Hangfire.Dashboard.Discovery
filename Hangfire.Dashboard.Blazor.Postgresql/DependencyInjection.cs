using Hangfire.Dashboard.Blazor.Core.Abstractions;
using Hangfire.Dashboard.Blazor.Postgresql.Context;
using Hangfire.Dashboard.Blazor.Postgresql.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Hangfire.Dashboard.Blazor.Postgresql;

public static class DependencyInjection
{
    public static IServiceCollection AddHangfireDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Надо переиспользовать коннекшен от DataSource
        // TODO: Вытащить схему из конфигурации hangfire postgresql
        
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddDbContext<HangfirePostgresqlContext>(options =>
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("hangfire"));
            dataSourceBuilder.EnableDynamicJson();
            options.UseNpgsql(dataSourceBuilder.Build());
        });

        services.AddScoped<IDashboardBackgroundProcessor, JobArgumentUpdaterProcess>();
        return services;
    }
}