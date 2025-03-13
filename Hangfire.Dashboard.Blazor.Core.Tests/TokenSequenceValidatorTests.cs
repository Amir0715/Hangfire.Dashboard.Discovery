using Hangfire.Dashboard.Blazor.Core.Tests.Helpers;
using Hangfire.Dashboard.Blazor.Core.Validators;
using Xunit.Abstractions;

namespace Hangfire.Dashboard.Blazor.Core.Tests;

public class TokenSequenceValidatorTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public static IEnumerable<object[]> TestData = new List<object[]>
    {
        new object[] { new TokenListBuilder().Open(), false },
        new object[] { new TokenListBuilder().Close(), false },
        new object[] { new TokenListBuilder().Close().Open(), false },
        new object[] { new TokenListBuilder().Open().Close(), false },

        new object[] { new TokenListBuilder().Less(), false },
        new object[] { new TokenListBuilder().Close().Less().Open(), false },
        new object[] { new TokenListBuilder().Open().Less().Close(), false },

        new object[] { new TokenListBuilder().LessOrEqual(), false },
        new object[] { new TokenListBuilder().Close().LessOrEqual().Open(), false },
        new object[] { new TokenListBuilder().Open().LessOrEqual().Close(), false },

        new object[] { new TokenListBuilder().Equal(), false },
        new object[] { new TokenListBuilder().Close().Equal().Open(), false },
        new object[] { new TokenListBuilder().Open().Equal().Close(), false },

        new object[] { new TokenListBuilder().NotEqual(), false },
        new object[] { new TokenListBuilder().Close().NotEqual().Open(), false },
        new object[] { new TokenListBuilder().Open().NotEqual().Close(), false },

        new object[] { new TokenListBuilder().Like(), false },
        new object[] { new TokenListBuilder().Close().Like().Open(), false },
        new object[] { new TokenListBuilder().Open().Like().Close(), false },

        new object[] { new TokenListBuilder().Greater(), false },
        new object[] { new TokenListBuilder().Close().Greater().Open(), false },
        new object[] { new TokenListBuilder().Open().Greater().Close(), false },

        new object[] { new TokenListBuilder().GreaterOrEqual(), false },
        new object[] { new TokenListBuilder().Close().GreaterOrEqual().Open(), false },
        new object[] { new TokenListBuilder().Open().GreaterOrEqual().Close(), false },

        new object[] { new TokenListBuilder().Constant("test"), false },
        new object[] { new TokenListBuilder().Close().Constant("test").Open(), false },
        new object[] { new TokenListBuilder().Open().Constant("test").Close(), false },

        new object[] { new TokenListBuilder().Constant("job.type"), false },
        new object[] { new TokenListBuilder().Constant("job.type()"), false },
        new object[] { new TokenListBuilder().Constant("(job)"), false },

        new object[] { new TokenListBuilder().Constant("(job)"), false },

        new object[] { new TokenListBuilder().FieldAccess("job.name"), false },
        new object[] { new TokenListBuilder().Close().FieldAccess("job.name").Open(), false },
        new object[] { new TokenListBuilder().Open().FieldAccess("job.name").Close(), false },

        new object[] { new TokenListBuilder().FieldAccess("(job)"), false },
        new object[] { new TokenListBuilder().FieldAccess("(job.name)"), false },
        new object[] { new TokenListBuilder().FieldAccess("(job.name)"), false },

        // Положительные кейсы
        new object[] { new TokenListBuilder().FieldAccess("a").Equal().Constant("b"), true },
        new object[] { new TokenListBuilder().Open().FieldAccess("a").Equal().Constant("b").Close(), true },
        new object[]
        {
            new TokenListBuilder()
                .Open().FieldAccess("a").Equal().Constant("b").Close()
                .And()
                .Open().Constant("c").Like().Constant("d").Close(),
            true
        },
        new object[]
        {
            new TokenListBuilder()
                .FieldAccess("x").Greater().Constant("10")
                .Or()
                .FieldAccess("y").LessOrEqual().Constant("20"),
            true
        },
        new object[]
        {
            new TokenListBuilder()
                .Open().Open().FieldAccess("a").NotEqual().Constant("b").Close().Close()
                .Or()
                .FieldAccess("c").Like().Constant("d"),
            true
        },

        // Отрицательные кейсы
        // Неправильные скобочные структуры
        new object[] { new TokenListBuilder().Open().FieldAccess("a").Equal().Constant("b"), false },
        new object[] { new TokenListBuilder().FieldAccess("a").Equal().Constant("b").Close(), false },

        // Неправильное расположение операторов
        new object[] { new TokenListBuilder().Equal().FieldAccess("a").Constant("b"), false },
        new object[] { new TokenListBuilder().FieldAccess("a").Constant("b").Equal(), false },
        new object[] { new TokenListBuilder().And().FieldAccess("a").Equal().Constant("b"), false },

        // Дублированные операнды
        new object[] { new TokenListBuilder().FieldAccess("a").FieldAccess("b"), false },
        new object[] { new TokenListBuilder().Constant("a").Constant("b"), false },

        new object[]
        {
            new TokenListBuilder()
                .FieldAccess("a").Equal().Constant("b")
                .And()
                .FieldAccess("c").Equal().Constant("d"),
            true
        },
        new object[]
        {
            new TokenListBuilder()
                .Open().FieldAccess("a").Equal().Constant("b").Close()
                .Open().FieldAccess("c").Equal().Constant("d").Close(),
            false // Нет оператора между скобками
        },

        // Пустые выражения
        new object[] { new TokenListBuilder().Open().Close(), false },
        new object[] { new TokenListBuilder().Open().And().Close(), false },

        // Сложные комбинации
        new object[]
        {
            new TokenListBuilder()
                .FieldAccess("a").Equal().Constant("b")
                .Or().FieldAccess("c").Equal().Constant("d")
                .And().FieldAccess("e").Equal().Constant("f"),
            true
        },
        new object[]
        {
            new TokenListBuilder()
                .FieldAccess("a").Equal().Constant("b")
                .Or().Open().FieldAccess("c").Equal().Constant("d")
                .And().FieldAccess("e").Equal().Constant("f").Close(),
            true 
        },

        // Граничные случаи
        new object[] { new TokenListBuilder().FieldAccess("a"), false }, // Только операнд
        new object[]
        {
            new TokenListBuilder().FieldAccess("a").And().FieldAccess("b"), false
        },
        new object[]
        {
            new TokenListBuilder().Constant("a").And().Constant("b"), false
        },
        new object[]
        {
            new TokenListBuilder().FieldAccess("a").And().Constant("b"), false
        },
        new object[]
        {
            new TokenListBuilder().FieldAccess("a").Equal().Constant("d").And().Constant("b"), false
        },
        new object[]
        {
            new TokenListBuilder().FieldAccess("a").Equal().Constant("d").And().FieldAccess("b"), false
        },
        new object[]
        {
            new TokenListBuilder()
                .Open().FieldAccess("a").Equal().Constant("b")
                .Or().FieldAccess("c").Equal().Constant("d").Close(),
            true
        }
    };

    public TokenSequenceValidatorTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    
    [Theory]
    [MemberData(nameof(TestData))]
    public void TokenSequenceValidator_Should_Success(TokenListBuilder tokens, bool expectedIsValid)
    {
        _testOutputHelper.WriteLine(tokens.ToString());
        var actualIsValid = QueryValidator.IsValidTokenSequence(tokens);
        Assert.Equal(expectedIsValid, actualIsValid.IsSuccess);
    }
}