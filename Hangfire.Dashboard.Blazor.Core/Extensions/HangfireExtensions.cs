namespace Hangfire.Dashboard.Blazor.Core.Extensions;

public static class HangfireExtensions
{
    public static string GetSetKey(this BackgroundJob backgroundJob) => $"{Constants.DiscoverySetKeyPrefix}:{backgroundJob.Id}";
}