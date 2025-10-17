using System.ComponentModel;
using Hangfire.Dashboard.Blazor.Core;
using Hangfire.Dashboard.Blazor.Core.Abstractions;
using Hangfire.Dashboard.Blazor.Core.Dtos;
using Hangfire.Dashboard.Blazor.Postgresql.Context;
using Hangfire.Dashboard.Blazor.Postgresql.Extensions;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;

namespace Hangfire.Dashboard.Blazor.Postgresql.Implementations;

public class PostgresJobRepository : IJobRepository
{
    private readonly PostgreSqlStorageOptions _postgreSqlStorageOptions;
    private readonly HangfirePostgresqlContext _postgresqlContext;

    public PostgresJobRepository(PostgreSqlStorageOptions postgreSqlStorageOptions, HangfirePostgresqlContext postgresqlContext)
    {
        _postgreSqlStorageOptions = postgreSqlStorageOptions;
        _postgresqlContext = postgresqlContext;
    }

    public async Task<TimePaginationResult<JobContext>> SearchAsync(TimePaginationQuery<SearchQuery> timePagination)
    {
        var schema = _postgreSqlStorageOptions.SchemaName;
        var sql = $"""
                   SELECT j.id AS "Id",
                          j.invocationdata ->> 'Type' AS "Type",
                          j.invocationdata ->> 'Method' AS "Method",
                          j.statename AS "State",
                          j.createdat AS "CreatedAt",
                          j.expireat AS "ExpireAt",
                          ds.value AS "Args",
                   
                     (SELECT s.data
                      FROM {schema}.state AS s
                      WHERE s.jobid = j.id
                      ORDER BY s.createdat DESC
                      LIMIT 1) AS "StateData",
                   
                     (SELECT jsonb_object_agg(name, btrim(value, '" ')) AS params_json
                      FROM {schema}.jobparameter
                      WHERE jobId = j.id) AS "Params",
                          (
                             (SELECT queue
                              FROM {schema}.jobqueue
                              WHERE jobid = j.id
                              ORDER BY createdat DESC
                              LIMIT 1)
                           UNION
                             (SELECT 'default')
                           LIMIT 1) AS "Queue"
                   FROM {schema}.job AS j
                   LEFT JOIN
                     (SELECT substring(s.key FROM '\d+')::bigint AS jobId,
                             s.value::JSONB
                      FROM {schema}."set" AS s
                      WHERE s."key" like '{Constants.DiscoverySetKeyPrefix}:%') AS ds ON jobId = j.id
                   """;
        var query = timePagination.Data;
        var queryExpression = query.QueryExpression;
            
        var q = _postgresqlContext.Database
            .SqlQueryRaw<JobContext>(sql)
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
            _ => throw new ArgumentOutOfRangeException(nameof(timePagination.Direction), "Provided not supported sort direction")
        };
        return new TimePaginationResult<JobContext>(jobs, nextOffset, timePagination.Limit, total);
    }
}