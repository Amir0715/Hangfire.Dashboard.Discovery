using Hangfire;
using Hangfire.Dashboard.Blazor;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Mvc;

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

app.MapGet("/success", (
    [FromQuery] string? q,
    [FromQuery] int? c,
    [FromQuery] int? d,
    IBackgroundJobClient backgroundJobClient
) =>
{
    for (var i = 0; i < c.GetValueOrDefault(1); i++)
    {
        backgroundJobClient.Enqueue<JobClass>(job => job.Success(d));
    }
});

app.MapGet("/error", (
    [FromQuery] string? q,
    [FromQuery] int? c,
    [FromQuery] int? d,
    IBackgroundJobClient backgroundJobClient
) =>
{
    for (var i = 0; i < c.GetValueOrDefault(1); i++)
    {
        backgroundJobClient.Enqueue<JobClass>(job => job.Error(d));
    }
});

app.Run();

public class JobClass
{
    private readonly ILogger<JobClass> _logger;

    public JobClass(ILogger<JobClass> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask Error(int? delaySeconds = null)
    {
        if (delaySeconds.HasValue)
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds.Value));

        throw new ArgumentException("testing argument exception");
    }

    public async ValueTask Success(int? delaySeconds = null)
    {
        if (delaySeconds.HasValue)
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds.Value));

        _logger.LogInformation("{job} done after {delaySeconds}", nameof(Success), delaySeconds);
    }
}