using System;
using System.Linq;
using System.Linq.Expressions;
using FluentValidation;
using Hangfire.Dashboard.Blazor.Core.Dtos;
using Hangfire.Dashboard.Blazor.Core.Tokenization;
using Hangfire.Dashboard.Blazor.Core.Validators;

namespace Hangfire.Dashboard.Blazor.Core;

public class QueryParser
{
    public static Expression<Func<JobContext, bool>> Parse(string query)
    {
        // TODO: throw exception
        new QueryDtoValidator().ValidateAndThrow(new QueryDto() { Query = query });
        
        var tokens = Tokenizer.Tokenize(query).ToList();

        var expressionGenerator = new ExpressionGenerator();
        return expressionGenerator.GenerateExpression(tokens);
    }
}