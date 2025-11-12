using System.Threading;
using System.Threading.Tasks;
using Hangfire.Dashboard.Blazor.Core.Dtos;

namespace Hangfire.Dashboard.Blazor.Core.Abstractions;

public interface IJobRepository
{
    public Task<TimePaginationResult<JobContext>> SearchAsync(TimePaginationQuery<SearchQuery> query, CancellationToken cancellationToken = default);

    public Task<JobHints> GetHintsAsync(IntervalQuery query, CancellationToken cancellationToken = default);
}