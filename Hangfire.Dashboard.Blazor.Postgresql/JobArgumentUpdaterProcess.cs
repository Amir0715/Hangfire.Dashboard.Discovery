using System.Text.Json;
using Hangfire.Dashboard.Blazor.Core.Abstractions;
using Hangfire.Dashboard.Blazor.Postgresql.Context;
using Hangfire.Dashboard.Blazor.Postgresql.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hangfire.Dashboard.Blazor.Postgresql;

/// <summary>
/// Background process for hangfire that fetch jobs and his argument names with reflection from assembly
/// </summary>
public class JobArgumentUpdaterProcess : IDashboardBackgroundProcessor
{
    public TimeSpan ExecuteInterval => TimeSpan.FromMinutes(1);
    
    private readonly HangfirePostgresqlContext _context;
    private readonly ILogger<JobArgumentUpdaterProcess> _logger;

    public JobArgumentUpdaterProcess(
        HangfirePostgresqlContext context,
        ILogger<JobArgumentUpdaterProcess> logger
    )
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await ConfigureArgColumn(cancellationToken);

        var jobsWithoutArgs = await _context.Jobs
            .Where(x => x.Arguments == null)
            .ToListAsync(cancellationToken);

        foreach (var jobsWithoutArg in jobsWithoutArgs)
        {
            try
            {
                var invocation = jobsWithoutArg.Invocation;

                var methodInfoResult = ReflectionHelper.GetMethod(
                    invocation.Type,
                    invocation.Method,
                    DeserializeParameterTypes(invocation.ParameterTypes)
                );
                if (!methodInfoResult.IsSuccess)
                {
                    _logger.LogError(methodInfoResult.Error);
                    continue;
                }

                var methodInfo = methodInfoResult.Value;
                var methodParameterInfos = methodInfo.GetParameters();
                var arguments = new Dictionary<string, JsonElement?>();

                var breakLoop = false;
                foreach (var (methodParameterInfo, jsonValueElement) in methodParameterInfos.Zip(GetJsonArguments(invocation.Arguments)))
                {
                    if (string.IsNullOrWhiteSpace(methodParameterInfo.Name))
                    {
                        _logger.LogError(
                            "Name of method parameter {methodParameterType} is null of method {method} in type {type}",
                            methodParameterInfo, invocation.Method, invocation.Type);

                        breakLoop = true;
                        break;
                    }

                    arguments[methodParameterInfo.Name] = jsonValueElement;
                }
                if (breakLoop) break;
                
                jobsWithoutArg.Arguments = JsonSerializer.SerializeToDocument(arguments);
                _logger.LogDebug("Saved job info for {jobId}", jobsWithoutArg.Id);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception while collecting types");
            }
        }

        _logger.LogInformation("Processed {jobs} jobs", jobsWithoutArgs.Count);
    }

    private Task ConfigureArgColumn(CancellationToken cancellationToken)
    {
        var schemaQualifiedTableName = _context.Jobs.EntityType.GetSchemaQualifiedTableName();
        var jobSchemaName = _context.Jobs.EntityType.GetSchema();
        var jobTableName = _context.Jobs.EntityType.GetTableName();
        return _context.Database.ExecuteSqlRawAsync(
            $"""
             DO $$
             BEGIN
                 IF NOT EXISTS (
                     SELECT 1 
                     FROM information_schema.columns 
                     WHERE
                         table_schema = '{jobSchemaName}'
                     AND table_name = '{jobTableName}' 
                     AND column_name = 'job_args'
                 ) THEN
                     ALTER TABLE {schemaQualifiedTableName} ADD COLUMN job_args JSONB;
                 END IF;
             END $$;
             CREATE INDEX IF NOT EXISTS idx_job_info_args ON {schemaQualifiedTableName} USING GIN (job_args);
             """, cancellationToken: cancellationToken);
    }

    private static string[] DeserializeParameterTypes(string parameterTypes)
    {
        return JsonSerializer.Deserialize<string[]>(parameterTypes) ?? [];
    }

    private static List<JsonElement?> GetJsonArguments(string argumentsString)
    {
        var argumentValueStrings = JsonSerializer.Deserialize<string?[]>(argumentsString) ?? [];
        var jsonElements = new List<JsonElement?>(argumentValueStrings.Length);
        foreach (var argumentValueString in argumentValueStrings)
        {
            JsonElement? jsonElement = null;
            if (argumentValueString != null)
            {
                jsonElement = JsonDocument.Parse(argumentValueString).RootElement;
            }

            jsonElements.Add(jsonElement);
        }

        return jsonElements;
    }
}