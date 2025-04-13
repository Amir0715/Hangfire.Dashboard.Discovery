using System.Linq.Expressions;
using System.Text.Json;
using Hangfire.Dashboard.Blazor.Core;
using Hangfire.Dashboard.Blazor.Core.Abstractions;
using Hangfire.Dashboard.Blazor.Postgresql.Context;
using Hangfire.Dashboard.Blazor.Postgresql.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Hangfire.Dashboard.Blazor.Postgresql.Implementations;

public class JobRepository : IJobRepository
{
    private readonly HangfirePostgresqlContext _hangfirePostgresqlContext;

    public JobRepository(HangfirePostgresqlContext hangfirePostgresqlContext)
    {
        _hangfirePostgresqlContext = hangfirePostgresqlContext ??
                                     throw new ArgumentNullException(nameof(hangfirePostgresqlContext));
    }

    public async Task<List<JobContext>> SearchAsync(SearchQuery query)
    {
        var schema = _hangfirePostgresqlContext.States.EntityType.GetSchema();
        var stateTable = _hangfirePostgresqlContext.States.EntityType.GetTableName();
        var queryExpression = query.QueryExpression;
        var jobs = await _hangfirePostgresqlContext.Database
            .SqlQueryRaw<JobContext>(
                $"""
                 SELECT j.id                          AS "Id",
                        j.invocationdata ->> 'Type'   AS "Type",
                        j.invocationdata ->> 'Method' AS "Method",
                        j.statename                   AS "State",
                        j.arguments                   AS "Args",
                        j.createdat                   AS "CreatedAt",
                        j.expireat                    AS "ExpireAt",
                        (SELECT s.data
                         FROM {schema}.state AS s
                         WHERE s.jobid = j.id
                         ORDER BY s.createdat DESC
                         LIMIT 1)                     AS "StateData",
                        (SELECT jsonb_object_agg(name, btrim(value, '" ')) AS params_json
                         FROM {schema}.jobparameter
                         WHERE jobId = j.id) as "Params",
                        ((SELECT queue from {schema}.jobqueue where jobid = j.id LIMIT 1)
                        UNION
                        (SELECT 'default')
                        LIMIT 1)                     as "Queue"
                 FROM {schema}.job AS j
                 """)
            .Where(queryExpression)
            .Where(job => job.CreatedAt >= query.StartDateTimeOffset.UtcDateTime)
            .WhereIf(query.EndDateTimeOffset.HasValue, job => job.CreatedAt <= query.EndDateTimeOffset!.Value.UtcDateTime)
            .ToListAsync();

        return jobs;
    }
}