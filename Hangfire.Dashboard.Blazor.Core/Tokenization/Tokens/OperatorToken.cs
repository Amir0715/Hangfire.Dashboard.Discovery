using System;

namespace Hangfire.Dashboard.Blazor.Core.Tokenization.Tokens;

public class OperatorToken : Token, IEquatable<OperatorToken>
{
    public override TokenType Type => TokenType.Operator;
    public OperatorType Operator { get; }

    public OperatorToken(OperatorType operatorType)
    {
        Operator = operatorType;
    }

    public bool Equals(OperatorToken? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Operator == other.Operator;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((OperatorToken)obj);
    }

    public override int GetHashCode()
    {
        return (int)Operator;
    }
}