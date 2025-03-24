using Hangfire.Dashboard.Blazor.Core.Abstractions.Tokens;
using Hangfire.Dashboard.Blazor.Core.Validators;

namespace Hangfire.Dashboard.Blazor.Core.Tests;

public class FieldAccessValidatorTests
{
    public static IEnumerable<object[]> TestData = new List<object[]>
    {
        new object[] { new FieldAccessToken("Method"), true },
        new object[] { new FieldAccessToken("State"), true },
        new object[] { new FieldAccessToken("Id"), true },
        new object[] { new FieldAccessToken("Queue"), true },
        new object[] { new FieldAccessToken("Type"), true },
        new object[] { new FieldAccessToken("Args.name"), true },
        new object[] { new FieldAccessToken("Args.offset"), true },
        new object[] { new FieldAccessToken("Args.customer.id"), true },
        new object[] { new FieldAccessToken("Args.invoice.id"), true },
        new object[] { new FieldAccessToken("CreatedAt"), true },
        new object[] { new FieldAccessToken("ExpireAt"), true },
        
        
        new object[] { new FieldAccessToken("Name"), false },
        new object[] { new FieldAccessToken("job"), false },
        new object[] { new FieldAccessToken("method"), false },
        new object[] { new FieldAccessToken("state"), false },
    };
    
    [Theory]
    [MemberData(nameof(TestData))]
    public void FieldAccessValidator_Success(FieldAccessToken fieldAccessToken, bool expectedValid)
    {
        var actualValid = new FieldAccessValidator().Validate(fieldAccessToken).IsValid;
        Assert.Equal(expectedValid, actualValid);
    }
}