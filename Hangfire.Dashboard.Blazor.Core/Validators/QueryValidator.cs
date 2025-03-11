using FluentValidation;
using Hangfire.Dashboard.Blazor.Core.Dtos;

namespace Hangfire.Dashboard.Blazor.Core.Validators;

public class QueryValidator : AbstractValidator<QueryDto>
{
    public QueryValidator()
    {
        RuleFor(x => x.Query).IsValidParenthesisSequence();
    }
}