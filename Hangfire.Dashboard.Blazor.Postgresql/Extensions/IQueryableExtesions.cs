using System.Linq.Expressions;

namespace Hangfire.Dashboard.Blazor.Postgresql.Extensions;

public static class IQueryableExtesions
{
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> queryable, bool condition, Expression<Func<T, bool>> predicate)
    {
        if (condition)
        {
            return queryable.Where(predicate);
        }

        return queryable;
    }
    
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> queryable, Func<bool> condition, Expression<Func<T, bool>> predicate)
    {
        return queryable.WhereIf(condition(), predicate);
    }
}