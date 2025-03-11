using System;

namespace Hangfire.Dashboard.Blazor.Core.Tokenization.Tokens;

public class ConstantToken : Token, IEquatable<ConstantToken>
{
    public ConstantToken(string value)
    {
        Value = value;
    }

    public override TokenType Type => TokenType.Constant;
    public string Value { get; }
    

    public bool Equals(ConstantToken? other)
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
        return Equals((ConstantToken)obj);
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