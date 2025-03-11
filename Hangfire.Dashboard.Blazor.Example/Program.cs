using System.Diagnostics;
using Hangfire;
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

builder.Services.AddHangfireDbContext(builder.Configuration);

var app = builder.Build();

app.MapGet("/test", async ([FromServices] HangfireContext hangfireContext) =>
{
    var query = """
                Method == "ErrorMethod" || Method == "SuccessMethod" || State == "Enqueued"
                """;
    var tokens = Tokenizer.Tokenize(query);
    var expression = new ExpressionGenerator().GenerateExpression(tokens);
    var jobs = await hangfireContext.Jobs
        .AsNoTracking()
        .Select(x => new JobContext()
        {
            Id = x.Id,
            Type = x.Invocation.Type,
            Method = x.Invocation.Method,
            State = x.State,
            Arguments = x.Arguments,
            CreatedAt = x.Createdat,
            ExpireAt = x.Expireat
        })
        .Where(expression)
        .ToListAsync();

    return jobs;
});
app.MapGet("/error/{q?}", (
    string? q,
    [FromQuery] int? d,
    [FromQuery] int? c,
    [FromServices] IBackgroundJobClient backgroundJobClient
) =>
{
    for (int i = 0; i < c.GetValueOrDefault(1); i++)
    {
        if (string.IsNullOrWhiteSpace(q))
            backgroundJobClient.Enqueue<JobClass>(j => j.ErrorMethod(d));
        else
            backgroundJobClient.Enqueue<JobClass>(q, j => j.ErrorMethod(d));
    }
});

app.MapGet("/success/{q?}", (
    string? q,
    [FromQuery] int? d,
    [FromQuery] int? c,
    [FromServices] IBackgroundJobClient backgroundJobClient
) =>
{
    for (int i = 0; i < c.GetValueOrDefault(1); i++)
    {
        if (string.IsNullOrWhiteSpace(q))
            backgroundJobClient.Enqueue<JobClass>(j => j.SuccessMethod(d));
        else
            backgroundJobClient.Enqueue<JobClass>(q, j => j.SuccessMethod(d));
    }
});

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