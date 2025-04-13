using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

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
    public static IQueryable<T> OrderByDirection<T, TKey>(this IQueryable<T> queryable, Expression<Func<T, TKey>> keySelector, ListSortDirection direction)
    {
        return direction switch
        {
            ListSortDirection.Ascending => queryable.OrderBy(keySelector),
            ListSortDirection.Descending => queryable.OrderByDescending(keySelector),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }
}