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
    .UseBlazorDiscoveryPanel()
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
    if (string.IsNullOrWhiteSpace(q))
    {
        for (var i = 0; i < c.GetValueOrDefault(1); i++)
        {
            backgroundJobClient.Enqueue<JobClass>(job => job.Success(d));
        }
    }
    else
    {
        for (var i = 0; i < c.GetValueOrDefault(1); i++)
        {
            backgroundJobClient.Enqueue<JobClass>(q, job => job.Success(d));
        }
    }
    
});

app.MapGet("/error", (
    [FromQuery] string? q,
    [FromQuery] int? c,
    [FromQuery] int? d,
    IBackgroundJobClient backgroundJobClient
) =>
{
    if (string.IsNullOrWhiteSpace(q))
    {
        for (var i = 0; i < c.GetValueOrDefault(1); i++)
        {
            backgroundJobClient.Enqueue<JobClass>(job => job.Error(d));
        }
    }
    else
    {
        for (var i = 0; i < c.GetValueOrDefault(1); i++)
        {
            backgroundJobClient.Enqueue<JobClass>(q, job => job.Error(d));
        }
    }
});

app.MapGet("/many_args", (
    [FromQuery] string? q,
    [FromQuery] int? c,
    [FromQuery] int? d,
    IBackgroundJobClient backgroundJobClient
) =>
{
    ComplexArgument complexArgument = new ComplexArgument()
    {
        Value1 = Guid.NewGuid().ToString(),
        Value2 = Guid.NewGuid(),
        InnerArgument = new ComplexArgument()
        {
            Value1 = Guid.NewGuid().ToString(),
            Value2 = Guid.Empty,
            InnerArgument = new ComplexArgument()
            {
                Value1 = String.Empty,
                Value2 = Guid.Parse("07d1e637-0f76-4e02-a5ab-d6afd7bb4c4f"),
                InnerArgument = null
            }
        }
    };
    if (string.IsNullOrWhiteSpace(q))
    {
        for (var i = 0; i < c.GetValueOrDefault(1); i++)
        {
            backgroundJobClient.Enqueue<JobClass>(job => job.ManyArgs(q, q + " " + q, complexArgument, d));
        }
    }
    else
    {
        for (var i = 0; i < c.GetValueOrDefault(1); i++)
        {
            backgroundJobClient.Enqueue<JobClass>(q, job => job.ManyArgs(q, q + " " + q, complexArgument, d));
        }
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
    
    public async ValueTask ManyArgs(string arg1, string arg2, ComplexArgument complex, int? delaySeconds = null)
    {
        if (delaySeconds.HasValue)
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds.Value));

        _logger.LogInformation("{job} done after {delaySeconds}, {arg1}, {arg2}, {@complex}", nameof(Success), delaySeconds, arg1, arg2, complex);
    }
}

public class ComplexArgument
{
    public string Value1 { get; set; }
    public Guid Value2 { get; set; }
    public ComplexArgument? InnerArgument { get; set; }
}