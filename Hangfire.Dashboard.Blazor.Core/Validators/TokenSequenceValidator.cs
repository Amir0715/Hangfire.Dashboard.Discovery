using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using Hangfire.Dashboard.Blazor.Core.Helpers;
using Hangfire.Dashboard.Blazor.Core.Tokenization.Tokens;

namespace Hangfire.Dashboard.Blazor.Core.Validators;

/// <summary>
/// Валидатор на правильную последовательность токенов. 
/// </summary>
public class TokenSequenceValidator : AbstractValidator<IEnumerable<Token>> 
{
    public override ValidationResult Validate(ValidationContext<IEnumerable<Token>> context)
    {
        var validationResult = new ValidationResult();
        var isValid = QueryValidator.IsValidTokenSequence(context.InstanceToValidate);
        if (!isValid) validationResult.Errors.Add(new ValidationFailure());
        return validationResult;
    }
}