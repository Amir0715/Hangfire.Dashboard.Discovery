using FluentValidation;
using Hangfire.Dashboard.Blazor.Core.Dtos;

namespace Hangfire.Dashboard.Blazor.Core.Validators;

public class QueryDtoValidator : AbstractValidator<QueryDto>
{
    public QueryDtoValidator()
    {
        RuleFor(x => x.Query).NotEmpty();
    }
}