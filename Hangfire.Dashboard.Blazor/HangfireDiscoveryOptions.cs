namespace Hangfire.Dashboard.Blazor;

public class HangfireDiscoveryOptions
{
    public TimeSpan StartDateTimeOffsetByNow { get; set; } = TimeSpan.FromMinutes(15).Negate();
    public string StartUpQuery { get; set; } = "State == \"Failed\"";
}