using System;

namespace Hangfire.Dashboard.Blazor.Core.Abstractions.Tokens;

public class DateTimeOffsetToken : Token, IEquatable<DateTimeOffsetToken>
{
    public override TokenType Type => TokenType.DateTime;
    public DateTimeOffset Value { get; }

    public DateTimeOffsetToken(DateTimeOffset value)
    {
        Value = value;
    }

    public bool Equals(DateTimeOffsetToken? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value.Equals(other.Value);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((DateTimeOffsetToken)obj);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}