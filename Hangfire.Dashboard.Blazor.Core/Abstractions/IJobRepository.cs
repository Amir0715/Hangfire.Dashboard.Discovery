using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Hangfire.Dashboard.Blazor.Core.Abstractions;

public interface IJobRepository
{
    public Task<List<JobContext>> SearchAsync(SearchQuery query);
}

public class SearchQuery
{
    public required Expression<Func<JobContext, bool>> QueryExpression { get; set; }
    public required DateTimeOffset StartDateTimeOffset  { get; set; }
    public DateTimeOffset? EndDateTimeOffset  { get; set; }
}