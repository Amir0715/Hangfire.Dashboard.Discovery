using System.Collections.Concurrent;
using System.Reflection;
using Hangfire.Dashboard.Blazor.Core;

namespace Hangfire.Dashboard.Blazor.Postgresql.Helpers;

internal static class ReflectionHelper
{
    private static readonly ConcurrentDictionary<string, Type?> _typeCache = new();
    private static readonly ConcurrentDictionary<(Type, string, Type[]), MethodInfo?> _methodCache = new();
    
    /// <summary>
    /// Return <see cref="MethodInfo"/> with <paramref name="methodName"/> name if can find method in <paramref name="typeName"/> class
    /// </summary>
    /// <param name="typeName"></param>
    /// <param name="methodName"></param>
    /// <param name="argTypes"></param>
    /// <returns></returns>
    public static Result<MethodInfo> GetMethod(string typeName, string methodName, string[] argTypes)
    {
        var type = _typeCache.GetOrAdd(typeName, Type.GetType);
        if (type is null)
            return Result<MethodInfo>.Failed($"Type '{typeName}' doesnt found.");

        var types = argTypes.Select(x => _typeCache.GetOrAdd(typeName, Type.GetType)).ToArray();
        if (types.Length > 0 && types.Any(x => x is null))
            return Result<MethodInfo>.Failed($"One of the type for arguments types doesnt found.");

        var methodInfo = _methodCache.GetOrAdd((type, methodName, types)!, 
            key => key.Item1.GetMethod(key.Item2, key.Item3));
        if (methodInfo is null) 
            return Result<MethodInfo>.Failed($"Public method '{methodName}' with arg types [{string.Join(',', types.Select(x => x!.FullName))}] doesnt found in type {type.FullName}");
        
        return Result<MethodInfo>.Success(methodInfo);
    } 
}