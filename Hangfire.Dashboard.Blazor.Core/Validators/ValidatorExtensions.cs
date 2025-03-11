using System;
using FluentValidation;

namespace Hangfire.Dashboard.Blazor.Core.Validators;

public static class ValidatorExtensions
{
    public static IRuleBuilderOptions<T, string> IsValidParenthesisSequence<T>(
        this IRuleBuilder<T, string> ruleBuilder
    )
    {
        return ruleBuilder.SetValidator(new IsValidParenthesisSequenceValidator<T>());
    }
}