using System;

namespace Hangfire.Dashboard.Blazor.Core.Abstractions;

public class IntervalQuery
{
    public required DateTimeOffset StartDateTimeOffset  { get; set; }
    public DateTimeOffset? EndDateTimeOffset  { get; set; }
}