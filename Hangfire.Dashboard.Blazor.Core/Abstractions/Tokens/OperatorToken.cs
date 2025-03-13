using System;

namespace Hangfire.Dashboard.Blazor.Core.Abstractions.Tokens;

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

    public override string ToString()
    {
        return Operator switch
        {
            OperatorType.And => "&&",
            OperatorType.Or => "||",
            OperatorType.Equal => "==",
            OperatorType.NotEqual => "!=",
            OperatorType.GreaterThan => ">",
            OperatorType.GreaterThanOrEqual => ">=",
            OperatorType.LessThan => "<",
            OperatorType.LessThanOrEqual => "<=",
            OperatorType.Like => "~=",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}