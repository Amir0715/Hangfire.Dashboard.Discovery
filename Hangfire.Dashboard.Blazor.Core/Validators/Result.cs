namespace Hangfire.Dashboard.Blazor.Core.Validators;

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