using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Hangfire.Dashboard.Blazor.Core.Abstractions.Tokens;

namespace Hangfire.Dashboard.Blazor.Core.Abstractions;

public interface IExpressionGenerator
{
    Expression<Func<JobContext, bool>> GenerateExpression(IEnumerable<Token> tokens);
}