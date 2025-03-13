using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Hangfire.Dashboard.Blazor.Core.Abstractions;

public interface IJobRepository
{
    public Task<List<JobContext>> SearchAsync(Expression<Func<JobContext, bool>> queryExpression);
}