using System;

namespace Hangfire.Dashboard.Blazor.Core.Abstractions.Tokens;

public class StringToken : Token, IEquatable<StringToken>
{
    public StringToken(string value)
    {
        Value = value;
    }

    public override TokenType Type => TokenType.String;
    public string Value { get; }
    
    public bool Equals(StringToken? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((StringToken)obj);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return $"\"{Value}\"";
    }
}