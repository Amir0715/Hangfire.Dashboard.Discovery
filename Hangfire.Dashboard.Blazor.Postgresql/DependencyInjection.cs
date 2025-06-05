using System.Reflection;
using Hangfire.Dashboard.Blazor.Core.Abstractions;
using Hangfire.Dashboard.Blazor.Postgresql.Context;
using Hangfire.Dashboard.Blazor.Postgresql.Implementations;
using Hangfire.PostgreSql;
using Hangfire.PostgreSql.Factories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;

namespace Hangfire.Dashboard.Blazor.Postgresql;

public static class DependencyInjection
{
    public static IServiceCollection AddHangfireDiscoveryWithPostgresStorage(this IServiceCollection services)
    {
        services.AddHangfireBlazorDashboard();

        services.AddScoped<IJobRepository, PostgresJobRepository>();
        services.AddPostgresServices();

        return services;
    }

    private static FieldInfo ConnectionFactoryFieldInfo =
        typeof(PostgreSqlStorage).GetField("_connectionFactory", BindingFlags.NonPublic | BindingFlags.Instance);

    private static PropertyInfo OptionsPropertyInfo =
        typeof(PostgreSqlStorage).GetProperty("Options", BindingFlags.NonPublic | BindingFlags.Instance);

    private static IServiceCollection AddPostgresServices(this IServiceCollection services)
    {
        services.TryAddSingleton<PostgreSqlStorageOptions>(sp =>
        {
            var jobStorage = sp.GetService<JobStorage>() as PostgreSqlStorage;
            var postgreSqlStorageOptions = OptionsPropertyInfo.GetValue(jobStorage) as PostgreSqlStorageOptions;
            if (postgreSqlStorageOptions is null)
            {
                throw new NotSupportedException("Not found PostgresSqlStorageOptions");
            }

            return postgreSqlStorageOptions;
        });
        
        services.AddDbContext<HangfirePostgresqlContext>((sp, builder) =>
        {
            var jobStorage = sp.GetRequiredService<JobStorage>() as PostgreSqlStorage;
            var npgsqlConnectionFactory = ConnectionFactoryFieldInfo.GetValue(jobStorage) as NpgsqlConnectionFactory;
            var connectionString = npgsqlConnectionFactory.ConnectionString.ToString();

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString).EnableDynamicJson();
            builder.UseNpgsql(dataSourceBuilder.Build());
        });
        
        return services;
    }
}