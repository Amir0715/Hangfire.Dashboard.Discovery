using System;
using System.ComponentModel;

namespace Hangfire.Dashboard.Blazor.Core.Dtos;

public class TimePaginationQuery<T> : TimePaginationQuery
{
    public T Data { get; set; }
    
    public TimePaginationQuery(TimePaginationQuery timePagination) : base(timePagination)
    {
        
    }
    
    public TimePaginationQuery()
    {
        
    }
}

public class TimePaginationQuery
{
    public ListSortDirection Direction { get; set; }

    /// <summary>
    /// Data Time will be less than TargetTime
    /// </summary>
    public DateTimeOffset? Offset { get; set; }

    /// <summary>
    /// Count of element than less TargetTime
    /// </summary>
    public int Limit { get; set; } = 50;

    public TimePaginationQuery(TimePaginationQuery timePagination)
    {
        Direction = timePagination.Direction;
        Offset = timePagination.Offset;
        Limit = timePagination.Limit;
    }

    public TimePaginationQuery()
    {
    }
}