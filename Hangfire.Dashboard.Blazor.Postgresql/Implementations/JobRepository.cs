using System.Linq.Expressions;
using Hangfire.Dashboard.Blazor.Core;
using Hangfire.Dashboard.Blazor.Core.Abstractions;
using Hangfire.Dashboard.Blazor.Postgresql.Context;
using Microsoft.EntityFrameworkCore;

namespace Hangfire.Dashboard.Blazor.Postgresql.Implementations;

public class JobRepository : IJobRepository
{
    private readonly HangfirePostgresqlContext _hangfirePostgresqlContext;

    public JobRepository(HangfirePostgresqlContext hangfirePostgresqlContext)
    {
        _hangfirePostgresqlContext = hangfirePostgresqlContext ?? throw new ArgumentNullException(nameof(hangfirePostgresqlContext));
    }
    
    public async Task<List<JobContext>> SearchAsync(Expression<Func<JobContext, bool>> queryExpression)
    {
        var jobs = await _hangfirePostgresqlContext.Jobs
            .AsNoTracking()
            .Select(x => new JobContext
            {
                Id = x.Id,
                Type = x.Invocation.Type,
                Method = x.Invocation.Method,
                State = x.State,
                Args = x.Arguments,
                CreatedAt = x.Createdat,
                ExpireAt = x.Expireat
            })
            .Where(queryExpression)
            .ToListAsync();

        return jobs;
    }
}