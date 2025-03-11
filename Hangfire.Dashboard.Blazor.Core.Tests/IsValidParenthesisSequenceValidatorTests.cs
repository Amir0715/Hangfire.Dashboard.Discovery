using FluentValidation;
using Hangfire.Dashboard.Blazor.Core.Validators;

namespace Hangfire.Dashboard.Blazor.Core.Tests;

public class IsValidParenthesisSequenceValidatorTests
{
    public static IEnumerable<object[]> TestData = new List<object[]>
    {
        new object[] { "()", true },
        new object[] { ")", false },
        new object[] { "(", false },
        new object[] { "", false },
        new object[] { "(a", false },
        new object[] { "a)", false },
        new object[] { "(a)", true },
        new object[] { "\"\"", true },
        new object[] { "\"", false },
        new object[] { "\"", false },
        new object[] { "\"asda(asdad\"", true },
        new object[] { "\"asda(asdad", false },
    };

    [Theory]
    [MemberData(nameof(TestData))]
    public void ValidParenthesisSequence_Should_Success(string query, bool expectedIsValid)
    {
        var validationContext = new ValidationContext<string>(query);
        var validator = new IsValidParenthesisSequenceValidator<string>();

        var actualIsValid = validator.IsValid(validationContext, query);

        Assert.Equal(expectedIsValid, actualIsValid);
    }
}