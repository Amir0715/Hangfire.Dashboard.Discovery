using Hangfire;
using Hangfire.Dashboard.Blazor;
using Hangfire.Dashboard.Blazor.Core;
using Hangfire.Dashboard.Blazor.Core.Tokenization;
using Hangfire.Dashboard.Blazor.Postgresql;
using Hangfire.Dashboard.Blazor.Postgresql.Context;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var hangfireConnectionString = builder.Configuration.GetConnectionString("hangfire") ??
                               throw new InvalidOperationException(
                                   "Connection string 'hangfire' was not provided.");

builder.Services.AddHangfire(c => c
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(opt => opt.UseNpgsqlConnection(hangfireConnectionString),
        new PostgreSqlStorageOptions
        {
            SchemaName = "hangfire"
        }
    ));

builder.Services.AddHangfireServer();

builder.Services.AddHangfireBlazorDashboard(builder.Configuration);

var app = builder.Build();
app.UseStaticFiles();
app.MapHangfireBlazorDashboard();
app.MapHangfireDashboard(options: new DashboardOptions { Authorization = [] });

app.Run();

public class JobClass
{
    private readonly ILogger<JobClass> _logger;

    public JobClass(ILogger<JobClass> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask ErrorMethod(int? delaySeconds = null)
    {
        if (delaySeconds.HasValue)
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds.Value));

        throw new ArgumentException("testing argument exception");
    }

    public async ValueTask SuccessMethod(int? delaySeconds = null)
    {
        if (delaySeconds.HasValue)
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds.Value));

        _logger.LogInformation("{job} done after {delaySeconds}", nameof(SuccessMethod), delaySeconds);
    }
}