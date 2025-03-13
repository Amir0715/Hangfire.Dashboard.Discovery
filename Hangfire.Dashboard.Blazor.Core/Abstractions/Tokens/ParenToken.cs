using System;

namespace Hangfire.Dashboard.Blazor.Core.Abstractions.Tokens;

public class ParenToken : Token, IEquatable<ParenToken>
{
    public override TokenType Type => TokenType.Paren;
    public ParenType Paren { get; }

    public ParenToken(ParenType paren)
    {
        Paren = paren;
    }

    public bool Equals(ParenToken? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Paren == other.Paren;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ParenToken)obj);
    }

    public override int GetHashCode()
    {
        return (int)Paren;
    }

    public override string ToString()
    {
        return Paren switch
        {
            ParenType.Close => ")",
            ParenType.Open => "(",
        };
    }
}