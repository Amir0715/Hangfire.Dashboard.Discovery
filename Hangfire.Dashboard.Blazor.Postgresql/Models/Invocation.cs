namespace Hangfire.Dashboard.Blazor.Postgresql.Models;

public class Invocation
{
    public string Type { get; set; }
    public string Method { get; set; }
    public string ParameterTypes { get; set; }

    public string Arguments { get; set; }
}