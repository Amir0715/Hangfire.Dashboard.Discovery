using System;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Hangfire.Dashboard.Blazor.Core.Abstractions.Tokens;

namespace Hangfire.Dashboard.Blazor.Core.Validators;

public class FieldAccessValidator : AbstractValidator<FieldAccessToken>
{
    private static Regex _fieldRegex;
    
    static FieldAccessValidator()
    {
        var jobContextProps = typeof(JobContext).GetProperties();
        var regexBuilder = new StringBuilder();
        foreach (var property in jobContextProps)
        {
            if (property.PropertyType == typeof(string))
            {
                regexBuilder.Append($"^{property.Name}$|");
            } else if (property.PropertyType.IsAssignableTo(typeof(INumber<>)))
            {
                regexBuilder.Append($"^{property.Name}$|");
            } else if (property.PropertyType == typeof(JsonDocument))
            {
                regexBuilder.AppendFormat(@"^{0}\.\S+$|", property.Name);
            }
        }

        _fieldRegex = new Regex(regexBuilder.ToString(), RegexOptions.Compiled | RegexOptions.CultureInvariant, TimeSpan.FromMinutes(1));
    }
    
    public override ValidationResult Validate(ValidationContext<FieldAccessToken> context)
    {
        if (!_fieldRegex.IsMatch(context.InstanceToValidate.FieldPath))
        {
            context.AddFailure(context.InstanceToValidate.FieldPath, $"Неправильное обращение к полю, Обращение к полю должно удовлетворять регулярному выражению: {_fieldRegex}");
        }

        return base.Validate(context);
    }
}