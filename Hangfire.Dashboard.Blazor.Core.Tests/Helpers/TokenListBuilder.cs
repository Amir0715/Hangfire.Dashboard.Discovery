using System.Collections;
using Hangfire.Dashboard.Blazor.Core.Tokenization.Tokens;

namespace Hangfire.Dashboard.Blazor.Core.Tests.Helpers;

public class TokenListBuilder : IEnumerable<Token>
{
    private List<Token> _tokens = [];

    public TokenListBuilder FieldAccess(string fieldPath)
    {
        _tokens.Add(new FieldAccessToken(fieldPath));
        return this;
    }
    
    public TokenListBuilder Constant(string constant)
    {
        _tokens.Add(new ConstantToken(constant));
        return this;
    }
    
    public TokenListBuilder Equal()
    {
        _tokens.Add(new OperatorToken(OperatorType.Equal));
        return this;
    }
    
    public TokenListBuilder NotEqual()
    {
        _tokens.Add(new OperatorToken(OperatorType.NotEqual));
        return this;
    }
    
    public TokenListBuilder Like()
    {
        _tokens.Add(new OperatorToken(OperatorType.Like));
        return this;
    }
    
    public TokenListBuilder Greater()
    {
        _tokens.Add(new OperatorToken(OperatorType.GreaterThan));
        return this;
    }
    
    public TokenListBuilder GreaterOrEqual()
    {
        _tokens.Add(new OperatorToken(OperatorType.GreaterThanOrEqual));
        return this;
    }
    
    public TokenListBuilder Less()
    {
        _tokens.Add(new OperatorToken(OperatorType.LessThan));
        return this;
    }
    
    public TokenListBuilder LessOrEqual()
    {
        _tokens.Add(new OperatorToken(OperatorType.LessThanOrEqual));
        return this;
    }
    
    public TokenListBuilder And()
    {
        _tokens.Add(new OperatorToken(OperatorType.And));
        return this;
    }
    
    public TokenListBuilder Or()
    {
        _tokens.Add(new OperatorToken(OperatorType.Or));
        return this;
    }
    
    public TokenListBuilder Open()
    {
        _tokens.Add(new ParenToken(ParenType.Open));
        return this;
    }
    
    public TokenListBuilder Close()
    {
        _tokens.Add(new ParenToken(ParenType.Close));
        return this;
    }

    public IEnumerator<Token> GetEnumerator()
    {
        return _tokens.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}