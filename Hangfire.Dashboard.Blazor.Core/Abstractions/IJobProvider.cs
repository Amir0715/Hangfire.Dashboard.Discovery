using System.Threading.Tasks;
using Hangfire.Dashboard.Blazor.Core.Dtos;

namespace Hangfire.Dashboard.Blazor.Core.Abstractions;

public interface IJobProvider
{
    public ValueTask<Result<TimePaginationResult<JobContext>>> SearchJobs(TimePaginationQuery<QueryDto> paginationQueryDto);
}