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
            .String("ScheduleEventHandleJob");
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
            .String("ScheduleEventHandleJob")
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
            .String("ScheduleEventHandleJob")
            .Close()
            .Or()
            .FieldAccess("Type")
            .Equal()
            .String("ScheduleEventHandleJob2");
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
            .String("ScheduleEventHandleJob")
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
            .String("2");
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
        // (jobCtx => (string.Contains(jobCtx.Type, "2"))
        var tokens = new TokenListBuilder()
            .String("2")
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
            .String("ScheduleEventHandleJob");
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

    [Fact]
    public void GenerateExpression_Number_Valid()
    {
        // (jobCtx => jobCtx.Args.Number == 3)
        var tokens = new TokenListBuilder()
            .FieldAccess("Args.Number")
            .Equal()
            .Number(3);
        var expressionGenerator = new ExpressionGenerator();

        var correctJson = """{"Number":3}""";
        var incorrectJson = """{"Number":5}""";
        List<JobContext> jobs =
        [
            new()
            {
                Type = "ScheduleEventHandleJob",
                Args = JsonDocument.Parse(correctJson)
            },
            new()
            {
                Type = "ScheduleEventHandleJob2",
                Args = JsonDocument.Parse(incorrectJson)
            },
            new()
            {
                Type = "ScheduleEventHandleJob3",
                Args = JsonDocument.Parse(correctJson)
            }
        ];
        
        var actualExpression = expressionGenerator.GenerateExpression(tokens);
        _testOutputHelper.WriteLine(actualExpression.ToString());
        
        Expression<Func<JobContext, bool>> expectedExpression = job => job.Args.RootElement.GetProperty("Number").GetSingle() == 3f;
        
        var actual = jobs.AsQueryable().Where(actualExpression).ToList();
        Assert.Equal(jobs.Count(expectedExpression.Compile()), actual.Count);
        Assert.True(actual.All(expectedExpression.Compile()));
    }
    
    [Fact]
    public void GenerateExpression_Number_Valid2()
    {
        Expression<Func<JobContext, bool>> expectedExpression = 
            job => job.Args.RootElement.GetProperty("Number").GetSingle() > 3.5f;
        var tokens = new TokenListBuilder()
            .FieldAccess("Args.Number")
            .Greater()
            .Number(3.2f);
        var expressionGenerator = new ExpressionGenerator();

        var correctJson = """{"Number":3.2}""";
        var incorrectJson = """{"Number":5.1}""";
        List<JobContext> jobs =
        [
            new()
            {
                Type = "ScheduleEventHandleJob",
                Args = JsonDocument.Parse(correctJson)
            },
            new()
            {
                Type = "ScheduleEventHandleJob2",
                Args = JsonDocument.Parse(incorrectJson)
            },
            new()
            {
                Type = "ScheduleEventHandleJob3",
                Args = JsonDocument.Parse(correctJson)
            }
        ];
        
        var actualExpression = expressionGenerator.GenerateExpression(tokens);
        _testOutputHelper.WriteLine(actualExpression.ToString());
        
        var actual = jobs.AsQueryable().Where(actualExpression).ToList();
        Assert.Equal(jobs.Count(expectedExpression.Compile()), actual.Count);
        Assert.True(actual.All(expectedExpression.Compile()));
    }
    
    [Fact]
    public void GenerateExpression_Number_Valid3()
    {
        Expression<Func<JobContext, bool>> expectedExpression = 
            job => job.Args.RootElement.GetProperty("Number").GetSingle() >= 5.1f;
        var tokens = new TokenListBuilder()
            .FieldAccess("Args.Number")
            .GreaterOrEqual()
            .Number(5.1f);
        var expressionGenerator = new ExpressionGenerator();

        var correctJson = """{"Number":3.2}""";
        var incorrectJson = """{"Number":5.1}""";
        List<JobContext> jobs =
        [
            new()
            {
                Type = "ScheduleEventHandleJob",
                Args = JsonDocument.Parse(correctJson)
            },
            new()
            {
                Type = "ScheduleEventHandleJob2",
                Args = JsonDocument.Parse(incorrectJson)
            },
            new()
            {
                Type = "ScheduleEventHandleJob3",
                Args = JsonDocument.Parse(correctJson)
            }
        ];
        
        var actualExpression = expressionGenerator.GenerateExpression(tokens);
        _testOutputHelper.WriteLine(actualExpression.ToString());
        
        var actual = jobs.AsQueryable().Where(actualExpression).ToList();
        Assert.Equal(jobs.Count(expectedExpression.Compile()), actual.Count);
        Assert.True(actual.All(expectedExpression.Compile()));
    }
    
    [Fact]
    public void GenerateExpression_DateTimeOffset_Valid1()
    {
        var targetDateTime = DateTimeOffset.Now;
        Expression<Func<JobContext, bool>> expectedExpression = 
            job => job.CreatedAt >= targetDateTime;
        var tokens = new TokenListBuilder()
            .FieldAccess("CreatedAt")
            .GreaterOrEqual(targetDateTime);
        
        var expressionGenerator = new ExpressionGenerator();

        List<JobContext> jobs =
        [
            new()
            {
                CreatedAt = DateTimeOffset.Now.AddDays(-1)
            },
            new()
            {
                CreatedAt = DateTimeOffset.Now.AddDays(1)
            },
            new()
            {
                CreatedAt = DateTimeOffset.Now.AddDays(-1)
            }
        ];
        
        var actualExpression = expressionGenerator.GenerateExpression(tokens);
        _testOutputHelper.WriteLine(actualExpression.ToString());
        
        var actual = jobs.AsQueryable().Where(actualExpression).ToList();
        Assert.Equal(jobs.Count(expectedExpression.Compile()), actual.Count);
        Assert.True(actual.All(expectedExpression.Compile()));
    }
    
    [Fact]
    public void GenerateExpression_DateTimeOffset_Valid2()
    {
        var targetDateTime = DateTimeOffset.Parse("2025-05-29T12:00:00Z");
        Expression<Func<JobContext, bool>> expectedExpression = 
            job => job.CreatedAt == targetDateTime;
        var tokens = new TokenListBuilder()
            .FieldAccess("CreatedAt")
            .Equal(targetDateTime);
        
        var expressionGenerator = new ExpressionGenerator();

        List<JobContext> jobs =
        [
            new()
            {
                CreatedAt = DateTimeOffset.Parse("2025-05-29T12:00:00Z")
            },
            new()
            {
                CreatedAt = DateTimeOffset.Parse("2025-05-29T12:00:00+3:00")
            },
            new()
            {
                CreatedAt = DateTimeOffset.Parse("2025-05-29T00:00:00Z")
            }
        ];
        
        var actualExpression = expressionGenerator.GenerateExpression(tokens);
        _testOutputHelper.WriteLine(actualExpression.ToString());
        
        var actual = jobs.AsQueryable().Where(actualExpression).ToList();
        Assert.Equal(jobs.Count(expectedExpression.Compile()), actual.Count);
        Assert.True(actual.All(expectedExpression.Compile()));
    }
    
    
    [Fact]
    public void GenerateExpression_DateTimeOffset_Valid3()
    {
        var targetDateTime = DateTimeOffset.Parse("2025-05-29T13:00:00Z");
        Expression<Func<JobContext, bool>> expectedExpression = 
            job => job.CreatedAt < targetDateTime;
        var tokens = new TokenListBuilder()
            .FieldAccess("CreatedAt")
            .Less(targetDateTime);
        
        var expressionGenerator = new ExpressionGenerator();

        List<JobContext> jobs =
        [
            new()
            {
                CreatedAt = DateTimeOffset.Parse("2025-05-29T12:00:00Z")
            },
            new()
            {
                CreatedAt = DateTimeOffset.Parse("2025-05-29T12:00:00+3:00")
            },
            new()
            {
                CreatedAt = DateTimeOffset.Parse("2025-05-29T00:00:00Z")
            }
        ];
        
        var actualExpression = expressionGenerator.GenerateExpression(tokens);
        _testOutputHelper.WriteLine(actualExpression.ToString());
        
        var actual = jobs.AsQueryable().Where(actualExpression).ToList();
        Assert.Equal(jobs.Count(expectedExpression.Compile()), actual.Count);
        Assert.True(actual.All(expectedExpression.Compile()));
    }
    
    [Fact]
    public void GenerateExpression_DateTimeOffset_InJsonDocument_Valid()
    {
        var targetDateTime = DateTimeOffset.Parse("2025-05-28T13:00:00Z");
        Expression<Func<JobContext, bool>> expectedExpression = 
            job => job.Args.RootElement.GetProperty("CreatedAt").GetDateTimeOffset() > targetDateTime;
        var tokens = new TokenListBuilder()
            .FieldAccess("Args.CreatedAt")
            .Greater(targetDateTime);
        
        var expressionGenerator = new ExpressionGenerator();

        var correctJsonDocument = """{"CreatedAt":"2025-05-29T12:00:00Z"}""";
        var incorrectJsonDocument = """{"CreatedAt":"2025-05-23T12:00:00Z"}""";
        List<JobContext> jobs =
        [
            new()
            {
                Args = JsonDocument.Parse(correctJsonDocument)
            },
            new()
            {
                Args = JsonDocument.Parse(incorrectJsonDocument)
            },
            new()
            {
                Args = JsonDocument.Parse(correctJsonDocument)
            }
        ];
        
        var actualExpression = expressionGenerator.GenerateExpression(tokens);
        _testOutputHelper.WriteLine(actualExpression.ToString());
        
        var actual = jobs.AsQueryable().Where(actualExpression).ToList();
        Assert.Equal(jobs.Count(expectedExpression.Compile()), actual.Count);
        Assert.True(actual.All(expectedExpression.Compile()));
    }
    
    [Fact]
    public void GenerateExpression_DateTimeOffset_InJsonDocument_Valid2()
    {
        var targetDateTime = DateTimeOffset.Parse("2025-05-28T13:00:00Z");
        Expression<Func<JobContext, bool>> expectedExpression = 
            job => job.Args.RootElement.GetProperty("CreatedAt").GetDateTimeOffset() < targetDateTime;
        var tokens = new TokenListBuilder()
            .FieldAccess("Args.CreatedAt")
            .Less(targetDateTime);
        
        var expressionGenerator = new ExpressionGenerator();

        var correctJsonDocument = """{"CreatedAt":"2025-05-29T12:00:00Z"}""";
        var incorrectJsonDocument = """{"CreatedAt":"2025-05-23T12:00:00Z"}""";
        List<JobContext> jobs =
        [
            new()
            {
                Args = JsonDocument.Parse(correctJsonDocument)
            },
            new()
            {
                Args = JsonDocument.Parse(incorrectJsonDocument)
            },
            new()
            {
                Args = JsonDocument.Parse(correctJsonDocument)
            }
        ];
        
        var actualExpression = expressionGenerator.GenerateExpression(tokens);
        _testOutputHelper.WriteLine(actualExpression.ToString());
        
        var actual = jobs.AsQueryable().Where(actualExpression).ToList();
        Assert.Equal(jobs.Count(expectedExpression.Compile()), actual.Count);
        Assert.True(actual.All(expectedExpression.Compile()));
    }
}