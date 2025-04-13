using System.Threading.Tasks;
using Hangfire.Dashboard.Blazor.Core.Dtos;
using Hangfire.Dashboard.Blazor.Core.Validators;

namespace Hangfire.Dashboard.Blazor.Core.Abstractions;

public interface IJobProvider
{
    public ValueTask<Result<TimePaginationResult<JobContext>>> SearchJobs(TimePaginationQuery<QueryDto> paginationQueryDto);
}