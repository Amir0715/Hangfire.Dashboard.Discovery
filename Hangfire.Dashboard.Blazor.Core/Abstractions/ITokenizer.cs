using System.Collections.Generic;
using Hangfire.Dashboard.Blazor.Core.Abstractions.Tokens;

namespace Hangfire.Dashboard.Blazor.Core.Abstractions;

public interface ITokenizer
{
    public IEnumerable<Token> Tokenize(string query);
}