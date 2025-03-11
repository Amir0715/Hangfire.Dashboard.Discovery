namespace Hangfire.Dashboard.Blazor.Core.Tests;

public class QueryParserTests
{
    [Theory]
    [InlineData("", false)]
    [InlineData("(", false)]
    [InlineData(")", false)]
    [InlineData("()", true)]
    [InlineData("(()", false)]
    [InlineData("(())", true)]
    [InlineData("(())()", true)]
    [InlineData("((asd))(asdasda)", true)]
    [InlineData("((asd))(asdasda)()", true)]
    [InlineData("((job.type != \"ScheduleEventHandleJob\" || job.method != \"ScheduleEventHandleJobAsync\") && job.state ~= \"Enqueued|Processing\")\n|| (job.params.topic = \"smx_comfort_client_app_internal_events\" || job.params.topic ~= \"\")", true)]
    public void Test_IsValidParenthesisSequence(string query, bool isValid)
    {
        // Assert.Equal(isValid, QueryParser.IsValidParenthesisSequence(query));
    }

    [Theory]
    [InlineData("Type == \"ScheduleEventHandleJob\"")]
    public void Test_QueryParser(string query)
    {
        var expression = QueryParser.Parse(query);
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
        var actual = jobs.AsQueryable().Where(expression).ToList();
        Assert.Equal(1, actual.Count);
        Assert.True(actual.All(x => x.Type == "ScheduleEventHandleJob"));
    }
}