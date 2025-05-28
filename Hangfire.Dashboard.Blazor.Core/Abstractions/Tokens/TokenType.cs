namespace Hangfire.Dashboard.Blazor.Core.Abstractions.Tokens;

public enum TokenType
{
    FieldAccess,
    Operator,
    String,
    Number,
    DateTime,
    Paren,
}