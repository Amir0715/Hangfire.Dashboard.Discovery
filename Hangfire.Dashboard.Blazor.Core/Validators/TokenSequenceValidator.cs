using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using Hangfire.Dashboard.Blazor.Core.Abstractions.Tokens;

namespace Hangfire.Dashboard.Blazor.Core.Validators;

/// <summary>
/// Валидатор на правильную последовательность токенов. 
/// </summary>
public class TokenSequenceValidator : AbstractValidator<IEnumerable<Token>> 
{
    public override ValidationResult Validate(ValidationContext<IEnumerable<Token>> context)
    {
        var validResult = QueryValidator.IsValidTokenSequence(context.InstanceToValidate);
        if (!validResult.IsSuccess) context.AddFailure(context.PropertyPath, validResult.Error);
        return base.Validate(context);
    }
}