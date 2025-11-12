using System;
using System.Linq.Expressions;

namespace Hangfire.Dashboard.Blazor.Core.Abstractions;

public class SearchQuery : IntervalQuery
{
    public required Expression<Func<JobContext, bool>> QueryExpression { get; set; }
}