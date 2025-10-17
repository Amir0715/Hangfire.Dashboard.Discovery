using Hangfire.Dashboard.Blazor.Core;

namespace Hangfire.Dashboard.Blazor.Models;

internal class JobModel
{
    public JobModel(JobContext jobContext)
    {
        JobContext = jobContext;
    }

    public JobContext JobContext { get; private set; }
    public bool Expanded { get; set; }
    
    public static implicit operator JobModel(JobContext jobContext) => new(jobContext);
}