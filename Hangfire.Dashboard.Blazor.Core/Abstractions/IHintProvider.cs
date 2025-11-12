using System.Threading;
using System.Threading.Tasks;

namespace Hangfire.Dashboard.Blazor.Core.Abstractions;

public interface IHintProvider
{
    /// <summary>
    /// Provide query hints that's available in interval.
    /// </summary>
    /// <param name="intervalQuery">Time interval for getting hints.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>Instance of <see cref="JobHints"/> that represent available hints in interval.</returns>
    public Task<JobHints> GetHintsAsync(IntervalQuery intervalQuery, CancellationToken cancellationToken = default);
}