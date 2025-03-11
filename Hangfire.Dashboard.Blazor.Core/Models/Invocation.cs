namespace Hangfire.Dashboard.Blazor.Core.Models;

public class Invocation
{
    public string Type { get; set; }
    public string Method { get; set; }
    public string[] ParameterTypes { get; set; }
}