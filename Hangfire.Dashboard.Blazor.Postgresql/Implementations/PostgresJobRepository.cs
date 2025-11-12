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
    private readonly string _jobContextSql;

    public PostgresJobRepository(
        PostgreSqlStorageOptions postgreSqlStorageOptions,
        HangfirePostgresqlContext postgresqlContext
    )
    {
        _postgreSqlStorageOptions = postgreSqlStorageOptions;
        _postgresqlContext = postgresqlContext;

        var schema = _postgreSqlStorageOptions.SchemaName;
        _jobContextSql = $"""
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
    }

    public async Task<TimePaginationResult<JobContext>> SearchAsync(TimePaginationQuery<SearchQuery> query, CancellationToken cancellationToken = default)
    {
        var searchQuery = query.Data;
        var queryExpression = searchQuery.QueryExpression;

        var q = _postgresqlContext.Database
            .SqlQueryRaw<JobContext>(_jobContextSql)
            .Where(queryExpression)
            .Where(job => job.CreatedAt >= searchQuery.StartDateTimeOffset.UtcDateTime)
            .WhereIf(searchQuery.EndDateTimeOffset.HasValue,
                job => job.CreatedAt <= searchQuery.EndDateTimeOffset!.Value.UtcDateTime);

        var jobs = await q
            .WhereIf(query is { Offset: not null, Direction: TimePaginationDirection.Newer },
                job => job.CreatedAt >= query.Offset)
            .WhereIf(query is { Offset: not null, Direction: TimePaginationDirection.Older },
                job => job.CreatedAt <= query.Offset)
            .OrderByTimeDirection(x => x.CreatedAt, query.Direction)
            .Take(query.Limit)
            .ToListAsync(cancellationToken);

        var total = await q.CountAsync(cancellationToken);
        var nextOffset = query.Direction switch
        {
            TimePaginationDirection.Newer => jobs.MaxBy(d => d.CreatedAt)?.CreatedAt,
            TimePaginationDirection.Older => jobs.MinBy(d => d.CreatedAt)?.CreatedAt,
            _ => throw new ArgumentOutOfRangeException(nameof(query), "Provided not supported sort direction")
        };
        return new TimePaginationResult<JobContext>(jobs, nextOffset, query.Limit, total);
    }

    public async Task<JobHints> GetHintsAsync(IntervalQuery query, CancellationToken cancellationToken = default)
    {
        // TODO: Точно нужен кэш
        var schema = _postgreSqlStorageOptions.SchemaName;
        var jobContexts = await _postgresqlContext.Database
            .SqlQueryRaw<JobContext>(
                $"""
                 with DiscoveryJobs as (SELECT j.id AS "Id",
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
                    WHERE s."key" like '{Constants.DiscoverySetKeyPrefix}:%') AS ds ON jobId = j.id)
                 select distinct on ("Type", "Method", "State") * from DiscoveryJobs
                 """)
            .WhereIf(query.EndDateTimeOffset.HasValue,
                job => job.CreatedAt <= query.EndDateTimeOffset!.Value.UtcDateTime)
            .Where(job => job.CreatedAt >= query.StartDateTimeOffset.UtcDateTime)
            .ToListAsync(cancellationToken);
        var types = jobContexts.Select(x => x.Type).Distinct().ToList();
        var methods = jobContexts.Select(x => x.Method).Distinct().ToList();
        var states = jobContexts.Select(x => x.State).Distinct().ToList();

        return new JobHints
        {
            Types = types,
            Methods = methods,
            States = states
        };
    }
}