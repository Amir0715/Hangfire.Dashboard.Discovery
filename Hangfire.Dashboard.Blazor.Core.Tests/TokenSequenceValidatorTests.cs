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

        new object[] { new TokenListBuilder().String("test"), false },
        new object[] { new TokenListBuilder().Close().String("test").Open(), false },
        new object[] { new TokenListBuilder().Open().String("test").Close(), false },

        new object[] { new TokenListBuilder().String("job.type"), false },
        new object[] { new TokenListBuilder().String("job.type()"), false },
        new object[] { new TokenListBuilder().String("(job)"), false },

        new object[] { new TokenListBuilder().String("(job)"), false },

        new object[] { new TokenListBuilder().FieldAccess("job.name"), false },
        new object[] { new TokenListBuilder().Close().FieldAccess("job.name").Open(), false },
        new object[] { new TokenListBuilder().Open().FieldAccess("job.name").Close(), false },

        new object[] { new TokenListBuilder().FieldAccess("(job)"), false },
        new object[] { new TokenListBuilder().FieldAccess("(job.name)"), false },
        new object[] { new TokenListBuilder().FieldAccess("(job.name)"), false },

        // Положительные кейсы
        new object[] { new TokenListBuilder().FieldAccess("a").Equal().String("b"), true },
        new object[] { new TokenListBuilder().Open().FieldAccess("a").Equal().String("b").Close(), true },
        new object[]
        {
            new TokenListBuilder()
                .Open().FieldAccess("a").Equal().String("b").Close()
                .And()
                .Open().String("c").Like().String("d").Close(),
            true
        },
        new object[]
        {
            new TokenListBuilder()
                .FieldAccess("x").Greater().String("10")
                .Or()
                .FieldAccess("y").LessOrEqual().String("20"),
            true
        },
        new object[]
        {
            new TokenListBuilder()
                .Open().Open().FieldAccess("a").NotEqual().String("b").Close().Close()
                .Or()
                .FieldAccess("c").Like().String("d"),
            true
        },

        // Отрицательные кейсы
        // Неправильные скобочные структуры
        new object[] { new TokenListBuilder().Open().FieldAccess("a").Equal().String("b"), false },
        new object[] { new TokenListBuilder().FieldAccess("a").Equal().String("b").Close(), false },

        // Неправильное расположение операторов
        new object[] { new TokenListBuilder().Equal().FieldAccess("a").String("b"), false },
        new object[] { new TokenListBuilder().FieldAccess("a").String("b").Equal(), false },
        new object[] { new TokenListBuilder().And().FieldAccess("a").Equal().String("b"), false },

        // Дублированные операнды
        new object[] { new TokenListBuilder().FieldAccess("a").FieldAccess("b"), false },
        new object[] { new TokenListBuilder().String("a").String("b"), false },

        new object[]
        {
            new TokenListBuilder()
                .FieldAccess("a").Equal().String("b")
                .And()
                .FieldAccess("c").Equal().String("d"),
            true
        },
        new object[]
        {
            new TokenListBuilder()
                .Open().FieldAccess("a").Equal().String("b").Close()
                .Open().FieldAccess("c").Equal().String("d").Close(),
            false // Нет оператора между скобками
        },

        // Пустые выражения
        new object[] { new TokenListBuilder().Open().Close(), false },
        new object[] { new TokenListBuilder().Open().And().Close(), false },

        // Сложные комбинации
        new object[]
        {
            new TokenListBuilder()
                .FieldAccess("a").Equal().String("b")
                .Or().FieldAccess("c").Equal().String("d")
                .And().FieldAccess("e").Equal().String("f"),
            true
        },
        new object[]
        {
            new TokenListBuilder()
                .FieldAccess("a").Equal().String("b")
                .Or().Open().FieldAccess("c").Equal().String("d")
                .And().FieldAccess("e").Equal().String("f").Close(),
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
            new TokenListBuilder().String("a").And().String("b"), false
        },
        new object[]
        {
            new TokenListBuilder().FieldAccess("a").And().String("b"), false
        },
        new object[]
        {
            new TokenListBuilder().FieldAccess("a").Equal().String("d").And().String("b"), false
        },
        new object[]
        {
            new TokenListBuilder().FieldAccess("a").Equal().String("d").And().FieldAccess("b"), false
        },
        new object[]
        {
            new TokenListBuilder()
                .Open().FieldAccess("a").Equal().String("b")
                .Or().FieldAccess("c").Equal().String("d").Close(),
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