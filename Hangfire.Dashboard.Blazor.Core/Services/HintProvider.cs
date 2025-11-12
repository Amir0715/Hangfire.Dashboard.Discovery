using System.Threading;
using System.Threading.Tasks;
using Hangfire.Dashboard.Blazor.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace Hangfire.Dashboard.Blazor.Core.Services;

public class HintProvider : IHintProvider
{
    private readonly IJobRepository _jobRepository;
    private readonly ILogger<HintProvider> _logger;

    public HintProvider(IJobRepository jobRepository, ILogger<HintProvider> logger)
    {
        _jobRepository = jobRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<JobHints> GetHintsAsync(IntervalQuery intervalQuery, CancellationToken cancellationToken = default)
    {
        var hints = await _jobRepository.GetHintsAsync(intervalQuery, cancellationToken);
        _logger.LogDebug("For query {@query} loaded hints {@hints}", intervalQuery, hints);

        return hints;
    }
}