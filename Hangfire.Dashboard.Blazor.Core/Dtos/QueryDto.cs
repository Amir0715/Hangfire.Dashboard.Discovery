using System;

namespace Hangfire.Dashboard.Blazor.Core.Dtos;

public class QueryDto
{
    public string Query { get; set; }
    public DateTimeOffset StartDateTimeOffset  { get; set; }
    public DateTimeOffset? EndDateTimeOffset  { get; set; }
}