using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hangfire.Dashboard.Blazor.Core.Abstractions;

public interface IDashboardBackgroundProcessor
{
    public TimeSpan ExecuteInterval { get; }
    public Task ExecuteAsync(CancellationToken cancellationToken);
}
