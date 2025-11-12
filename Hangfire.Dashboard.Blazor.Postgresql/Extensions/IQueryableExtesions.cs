using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using Hangfire.Dashboard.Blazor.Core.Dtos;

namespace Hangfire.Dashboard.Blazor.Postgresql.Extensions;

public static class IQueryableExtesions
{
    [Pure]
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> queryable, bool condition, Expression<Func<T, bool>> predicate)
    {
        if (condition)
        {
            return queryable.Where(predicate);
        }

        return queryable;
    }
    
    [Pure]
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> queryable, Func<bool> condition, Expression<Func<T, bool>> predicate)
    {
        return queryable.WhereIf(condition(), predicate);
    }
    
    [Pure]
    public static IQueryable<T> OrderByTimeDirection<T, TKey>(this IQueryable<T> queryable, Expression<Func<T, TKey>> keySelector, TimePaginationDirection direction)
    {
        return direction switch
        {
            TimePaginationDirection.Newer => queryable.OrderBy(keySelector),
            TimePaginationDirection.Older => queryable.OrderByDescending(keySelector),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }
}