using System.ComponentModel;
using Hangfire.Dashboard.Blazor.Core;
using Hangfire.Dashboard.Blazor.Core.Abstractions;
using Hangfire.Dashboard.Blazor.Core.Dtos;
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

    public async Task<TimePaginationResult<JobContext>> SearchAsync(TimePaginationQuery<SearchQuery> timePagination)
    {
        // Подсказка значений State
        var schema = _hangfirePostgresqlContext.States.EntityType.GetSchema();
        var stateTable = _hangfirePostgresqlContext.States.EntityType.GetTableName();
        var query = timePagination.Data;
        var queryExpression = query.QueryExpression;

        var q = _hangfirePostgresqlContext.Database
            .SqlQueryRaw<JobContext>(
                $"""
                 SELECT j.id                          AS "Id",
                        j.invocationdata ->> 'Type'   AS "Type",
                        j.invocationdata ->> 'Method' AS "Method",
                        j.statename                   AS "State",
                        j.job_args                    AS "Args",
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
                        ((SELECT queue from {schema}.jobqueue where jobid = j.id order by createdat desc LIMIT 1)
                        UNION
                        (SELECT 'default')
                        LIMIT 1)                     as "Queue"
                 FROM {schema}.job AS j
                 """)
            .Where(queryExpression)
            .Where(job => job.CreatedAt >= query.StartDateTimeOffset.UtcDateTime)
            .WhereIf(query.EndDateTimeOffset.HasValue,
                job => job.CreatedAt <= query.EndDateTimeOffset!.Value.UtcDateTime);
        
        var jobs = await q
            .WhereIf(timePagination is { Offset: not null, Direction: ListSortDirection.Ascending },
                job => job.CreatedAt >= timePagination.Offset)
            .WhereIf(timePagination is { Offset: not null, Direction: ListSortDirection.Descending },
                job => job.CreatedAt <= timePagination.Offset)
            .OrderByDirection(x => x.CreatedAt, timePagination.Direction)
            .Take(timePagination.Limit)
            .ToListAsync();

        var total = await q.CountAsync();
        var nextOffset = timePagination.Direction switch
        {
            ListSortDirection.Ascending => jobs.MaxBy(d => d.CreatedAt)?.CreatedAt,
            ListSortDirection.Descending => jobs.MinBy(d => d.CreatedAt)?.CreatedAt,
            _ => throw new ArgumentOutOfRangeException()
        };
        return new TimePaginationResult<JobContext>(jobs, nextOffset, timePagination.Limit, total);
    }
}