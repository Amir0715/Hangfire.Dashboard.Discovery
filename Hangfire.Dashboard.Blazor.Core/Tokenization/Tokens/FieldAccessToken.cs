using System;

namespace Hangfire.Dashboard.Blazor.Core.Tokenization.Tokens;

public class FieldAccessToken : Token, IEquatable<FieldAccessToken>
{
    public override TokenType Type => TokenType.FieldAccess;
    public string FieldPath { get; }
    public int Depth => FieldPath.AsSpan().Count('.') + 1;

    public FieldAccessToken(string fieldPath)
    {
        if (string.IsNullOrWhiteSpace(fieldPath))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(fieldPath));
        
        FieldPath = fieldPath;
    }

    public bool Equals(FieldAccessToken? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return FieldPath == other.FieldPath;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((FieldAccessToken)obj);
    }

    public override int GetHashCode()
    {
        return FieldPath.GetHashCode();
    }

    public override string ToString()
    {
        return FieldPath;
    }
}