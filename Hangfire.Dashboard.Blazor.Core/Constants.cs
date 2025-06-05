using System;

namespace Hangfire.Dashboard.Blazor.Core;

public class Constants
{
    public static readonly TimeSpan StartDateTimeOffsetByNow = TimeSpan.FromMinutes(15).Negate();

    public const string DiscoverySetKeyPrefix = "discovery:args";
}