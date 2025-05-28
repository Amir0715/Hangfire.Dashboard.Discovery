using System;

namespace Hangfire.Dashboard.Blazor.Core.Abstractions.Tokens;

public class NumberToken : Token, IEquatable<NumberToken>
{
    public override TokenType Type => TokenType.Number;
    public float Value { get; }

    public NumberToken(float value)
    {
        Value = value;
    }
    
    public bool Equals(NumberToken? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Math.Abs(Value - other.Value) < 0.000000001;
    }
    
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return $"{Value}";
    }
}