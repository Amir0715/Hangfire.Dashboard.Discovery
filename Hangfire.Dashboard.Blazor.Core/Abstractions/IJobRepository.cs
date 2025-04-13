using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hangfire.Dashboard.Blazor.Core.Dtos;

namespace Hangfire.Dashboard.Blazor.Core.Abstractions;

public interface IJobRepository
{
    public Task<TimePaginationResult<JobContext>> SearchAsync(TimePaginationQuery<SearchQuery> query);
}

public class SearchQuery
{
    public required Expression<Func<JobContext, bool>> QueryExpression { get; set; }
    public required DateTimeOffset StartDateTimeOffset  { get; set; }
    public DateTimeOffset? EndDateTimeOffset  { get; set; }
}