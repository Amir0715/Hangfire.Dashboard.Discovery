using System.Text.Json;
using Hangfire.Dashboard.Blazor.Postgresql.Context;
using Hangfire.Dashboard.Blazor.Postgresql.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hangfire.Dashboard.Blazor.Postgresql;

[DisableConcurrentExecution(10 * 60)]
public class JobArgumentInfoUpdateRecurrentJob
{
    private readonly HangfirePostgresqlContext _context;
    private readonly ILogger<JobArgumentInfoUpdateRecurrentJob> _logger;

    public JobArgumentInfoUpdateRecurrentJob(
        HangfirePostgresqlContext context,
        ILogger<JobArgumentInfoUpdateRecurrentJob> logger
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
                // TODO: add cache
                var jobType = Type.GetType(invocation.Type);
                if (jobType == null)
                {
                    _logger.LogDebug("Type {type} not found in assembly", invocation.Type);
                    continue;
                }

                var jobMethod = jobType.GetMethod(invocation.Method);
                if (jobMethod == null)
                {
                    _logger.LogDebug("Method {method} not found in type {type}", invocation.Method, invocation.Type);
                    continue;
                }

                var parameterTypeNames = DeserializeParameterTypes(invocation.ParameterTypes);
                if (parameterTypeNames.Count != 0)
                {
                    var parameterTypes = new List<Type>(parameterTypeNames.Count);
                    var breakLoop = false;
                    foreach (var parameterTypeName in parameterTypeNames)
                    {
                        var parameterType = Type.GetType(parameterTypeName);
                        if (parameterType == null)
                        {
                            _logger.LogDebug("Parameter {parameter} of method {method} not found in type {type}",
                                parameterTypeName, invocation.Method, invocation.Type);
                            breakLoop = true;
                            break;
                        }

                        parameterTypes.Add(parameterType);
                    }
                    if (breakLoop) break;

                    var methodParameterInfos = jobMethod.GetParameters();
                    var arguments = new Dictionary<string, JsonElement?>();
                    
                    foreach (var (methodParameterInfo, jobParameterType, jsonValueElement) in methodParameterInfos.Zip(parameterTypes, GetJsonArguments(invocation.Arguments)))
                    {
                        if (!methodParameterInfo.ParameterType.IsEquivalentTo(jobParameterType))
                        {
                            _logger.LogDebug(
                                "Parameters types {jobParameterType} {methodParameterType} not equal of method {method} in type {type}",
                                jobParameterType, methodParameterInfo.ParameterType, invocation.Method, invocation.Type);

                            breakLoop = true;
                            break;
                        }

                        if (string.IsNullOrWhiteSpace(methodParameterInfo.Name))
                        {
                            _logger.LogDebug(
                                "Name of method parameter {methodParameterType} is null of method {method} in type {type}",
                                jobParameterType, invocation.Method, invocation.Type);
                            
                            breakLoop = true;
                            break;
                        }
                        
                        arguments[methodParameterInfo.Name] = jsonValueElement;
                    }
                    if (breakLoop) break;

                    jobsWithoutArg.Arguments = JsonSerializer.SerializeToDocument(arguments);
                }

                _logger.LogDebug("Collected job info");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception while collecting types");
            }
        }
        
        await _context.SaveChangesAsync(cancellationToken);
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

    private static List<string> DeserializeParameterTypes(string parameterTypes)
    {
        return JsonSerializer.Deserialize<List<string>>(parameterTypes) ?? [];
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