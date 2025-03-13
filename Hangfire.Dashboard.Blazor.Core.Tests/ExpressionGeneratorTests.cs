using System.Linq.Expressions;
using System.Text.Json;
using Hangfire.Dashboard.Blazor.Core.Tests.Helpers;
using Xunit.Abstractions;

namespace Hangfire.Dashboard.Blazor.Core.Tests;

public class ExpressionGeneratorTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ExpressionGeneratorTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void GenerateExpression_Valid()
    {
        // job.Invocation.Type == "ScheduleEventHandleJob"
        var tokens = new TokenListBuilder()
            .FieldAccess("Type")
            .Equal()
            .Constant("ScheduleEventHandleJob");
        var expressionGenerator = new ExpressionGenerator();

        List<JobContext> jobs =
        [
            new()
            {
                Type = "ScheduleEventHandleJob"
            },
            new()
            {
                Type = "ScheduleEventHandleJob2"
            }
        ];

        var expression = expressionGenerator.GenerateExpression(tokens);
        _testOutputHelper.WriteLine(expression.ToString());

        var actual = jobs.AsQueryable().Where(expression).ToList();
        Assert.Equal(1, actual.Count);
        Assert.True(actual.All(x => x.Type == "ScheduleEventHandleJob"));
    }

    [Fact]
    public void GenerateExpression_Valid2()
    {
        // job.Invocation.Type == "ScheduleEventHandleJob"
        var tokens = new TokenListBuilder()
            .Open()
            .FieldAccess("Type")
            .Equal()
            .Constant("ScheduleEventHandleJob")
            .Close();
        var expressionGenerator = new ExpressionGenerator();

        List<JobContext> jobs =
        [
            new()
            {
                Type = "ScheduleEventHandleJob"
            },
            new()
            {
                Type = "ScheduleEventHandleJob2"
            }
        ];

        var expression = expressionGenerator.GenerateExpression(tokens);
        _testOutputHelper.WriteLine(expression.ToString());

        var actual = jobs.AsQueryable().Where(expression).ToList();
        Assert.Equal(1, actual.Count);
        Assert.True(actual.All(x => x.Type == "ScheduleEventHandleJob"));
    }

    [Fact]
    public void GenerateExpression_Valid3()
    {
        // (job.Type => ())
        var tokens = new TokenListBuilder()
            .Open()
            .FieldAccess("Type")
            .Equal()
            .Constant("ScheduleEventHandleJob")
            .Close()
            .Or()
            .FieldAccess("Type")
            .Equal()
            .Constant("ScheduleEventHandleJob2");
        var expressionGenerator = new ExpressionGenerator();

        List<JobContext> jobs =
        [
            new()
            {
                Type = "ScheduleEventHandleJob"
            },
            new()
            {
                Type = "ScheduleEventHandleJob2"
            },
            new()
            {
                Type = "ScheduleEventHandleJob3"
            }
        ];

        var actualExpression = expressionGenerator.GenerateExpression(tokens);
        _testOutputHelper.WriteLine(actualExpression.ToString());
        Expression<Func<JobContext, bool>> expectedExpression = job =>
            (job.Type == "ScheduleEventHandleJob") || job.Type == "ScheduleEventHandleJob2";
        var actual = jobs.AsQueryable().Where(actualExpression).ToList();
        Assert.Equal(2, actual.Count);
        Assert.True(actual.All(expectedExpression.Compile()));

        // Assert.Equal(expectedExpression, actualExpression);
    }

    [Fact]
    public void GenerateExpression_Valid4()
    {
        // (job.Type => ())
        var tokens = new TokenListBuilder()
            .Constant("ScheduleEventHandleJob")
            .Equal()
            .FieldAccess("Type");
        var expressionGenerator = new ExpressionGenerator();

        List<JobContext> jobs =
        [
            new()
            {
                Type = "ScheduleEventHandleJob"
            },
            new()
            {
                Type = "ScheduleEventHandleJob2"
            },
            new()
            {
                Type = "ScheduleEventHandleJob3"
            }
        ];

        var actualExpression = expressionGenerator.GenerateExpression(tokens);
        _testOutputHelper.WriteLine(actualExpression.ToString());
        Expression<Func<JobContext, bool>> expectedExpression = job => job.Type == "ScheduleEventHandleJob";
        var actual = jobs.AsQueryable().Where(actualExpression).ToList();
        Assert.Equal(jobs.Count(expectedExpression.Compile()), actual.Count);
        Assert.True(actual.All(expectedExpression.Compile()));

        // Assert.Equal(expectedExpression, actualExpression);
    }

    [Fact]
    public void GenerateExpression_Valid5()
    {
        // (job.Type => ())
        var tokens = new TokenListBuilder()
            .FieldAccess("Type")
            .Like()
            .Constant("2");
        var expressionGenerator = new ExpressionGenerator();

        List<JobContext> jobs =
        [
            new()
            {
                Type = "ScheduleEventHandleJob"
            },
            new()
            {
                Type = "ScheduleEventHandleJob2"
            },
            new()
            {
                Type = "ScheduleEventHandleJob3"
            }
        ];

        var actualExpression = expressionGenerator.GenerateExpression(tokens);
        _testOutputHelper.WriteLine(actualExpression.ToString());
        Expression<Func<JobContext, bool>> expectedExpression = job => job.Type.Contains("2");
        var actual = jobs.AsQueryable().Where(actualExpression).ToList();
        Assert.Equal(jobs.Count(expectedExpression.Compile()), actual.Count);
        Assert.True(actual.All(expectedExpression.Compile()));

        // Assert.Equal(expectedExpression, actualExpression);
    }

    [Fact]
    public void GenerateExpression_Valid6()
    {
        // (job.Type => ())
        var tokens = new TokenListBuilder()
            .Constant("2")
            .Like()
            .FieldAccess("Type");
        var expressionGenerator = new ExpressionGenerator();

        List<JobContext> jobs =
        [
            new()
            {
                Type = "ScheduleEventHandleJob"
            },
            new()
            {
                Type = "ScheduleEventHandleJob2"
            },
            new()
            {
                Type = "ScheduleEventHandleJob3"
            }
        ];

        var actualExpression = expressionGenerator.GenerateExpression(tokens);
        _testOutputHelper.WriteLine(actualExpression.ToString());
        Expression<Func<JobContext, bool>> expectedExpression = job => "2".Contains(job.Type);
        var actual = jobs.AsQueryable().Where(actualExpression).ToList();
        Assert.Equal(jobs.Count(expectedExpression.Compile()), actual.Count);
        Assert.True(actual.All(expectedExpression.Compile()));

        // Assert.Equal(expectedExpression, actualExpression);
    }

    [Fact]
    public void GenerateExpression_JsonDocument_Valid7()
    {
        var tokens = new TokenListBuilder()
            .FieldAccess("Arguments.name")
            .Equal()
            .Constant("ScheduleEventHandleJob");
        var expressionGenerator = new ExpressionGenerator();
        List<JobContext> jobs =
        [
            new()
            {
                Type = "ScheduleEventHandleJob",
                Args = JsonDocument.Parse(
                        """
                        {
                           "name": "ScheduleEventHandleJob"
                        }
                        """)
            },
            new()
            {
                Type = "ScheduleEventHandleJob2",
                Args = JsonDocument.Parse(
                        """
                        {
                           "name": "ScheduleEventHandleJob2"
                        }
                        """)
            },
            new()
            {
                Type = "ScheduleEventHandleJob3",
                Args = JsonDocument.Parse(
                        """
                        {
                           "name": "ScheduleEventHandleJob3"
                        }
                        """)
            }
        ];

        var actualExpression = expressionGenerator.GenerateExpression(tokens);
        _testOutputHelper.WriteLine(actualExpression.ToString());
        Expression<Func<JobContext, bool>> expectedExpression = job => job.Args.RootElement.GetProperty("name").GetString() == "ScheduleEventHandleJob";
        var actual = jobs.AsQueryable().Where(actualExpression).ToList();
        Assert.Equal(jobs.Count(expectedExpression.Compile()), actual.Count);
        Assert.True(actual.All(expectedExpression.Compile()));

        // Assert.Equal(expectedExpression, actualExpression);
    }
}