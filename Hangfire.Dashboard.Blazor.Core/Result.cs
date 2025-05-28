using System.Diagnostics.CodeAnalysis;

namespace Hangfire.Dashboard.Blazor.Core;

public class Result
{
    public bool IsSuccess => Error is null;
    public string? Error { get; }

    private Result(string? error)
    {
        Error = error;
    }

    public static Result Success() => new(null);
    public static Result Failed(string error) => new(error);
}

public class Result<T>
{
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => Error is null && Value is not null;

    public T? Value { get; }

    public string? Error { get; }

    private Result(T? value, string? error)
    {
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failed(string? error) => new(default, error);
}