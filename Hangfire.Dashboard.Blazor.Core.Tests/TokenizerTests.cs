using Hangfire.Dashboard.Blazor.Core.Abstractions.Tokens;
using Hangfire.Dashboard.Blazor.Core.Tests.Helpers;
using Hangfire.Dashboard.Blazor.Core.Tokenization;

namespace Hangfire.Dashboard.Blazor.Core.Tests;

public class TokenizerTests
{
    public static IEnumerable<object[]> Tokenize_TestData = new List<object[]>
    {
        new object[] { string.Empty, new TokenListBuilder() },
        new object[] { "&", new TokenListBuilder() },
        new object[] { "|", new TokenListBuilder() },
        new object[] { "\"", new TokenListBuilder() },
        
        new object[] { "(", new TokenListBuilder().Open() },
        new object[] { ")", new TokenListBuilder().Close() },
        
        new object[] { ">", new TokenListBuilder().Greater() },
        new object[] { "<", new TokenListBuilder().Less() },
        new object[] { ">=", new TokenListBuilder().GreaterOrEqual() },
        new object[] { "<=", new TokenListBuilder().LessOrEqual() },
        new object[] { "!=", new TokenListBuilder().NotEqual() },
        new object[] { "~=", new TokenListBuilder().Like() },
        
        new object[] { "&&", new TokenListBuilder().And() },
        new object[] { "||", new TokenListBuilder().Or() },
        
        new object[] { "job", new TokenListBuilder().FieldAccess("job") },
        new object[] { "job.field", new TokenListBuilder().FieldAccess("job.field")},
        new object[] { "job.field.field2.field3", new TokenListBuilder().FieldAccess("job.field.field2.field3") },
        
        new object[] { "job.type==\"jobtype\"", new TokenListBuilder().FieldAccess("job.type").Equal().Constant("jobtype") },
        new object[] { "(job.type==\"jobtype\"", new TokenListBuilder().Open().FieldAccess("job.type").Equal().Constant("jobtype") },
        new object[] { "(job.type==\"jobtype\")", new TokenListBuilder().Open().FieldAccess("job.type").Equal().Constant("jobtype").Close() },
        new object[] { "( job.type==\"jobtype\")", new TokenListBuilder().Open().FieldAccess("job.type").Equal().Constant("jobtype").Close() },
        new object[] { "( job.type==\"jobtype\" )", new TokenListBuilder().Open().FieldAccess("job.type").Equal().Constant("jobtype").Close() },
        new object[] { "( job.type ==\"jobtype\" )", new TokenListBuilder().Open().FieldAccess("job.type").Equal().Constant("jobtype").Close() },
        new object[] { "( job.type == \"jobtype\" )", new TokenListBuilder().Open().FieldAccess("job.type").Equal().Constant("jobtype").Close() },
        
        new object[] { "job.type ==\"jobtype\"", new TokenListBuilder().FieldAccess("job.type").Equal().Constant("jobtype") },
        new object[] { "job.type == \"jobtype\"", new TokenListBuilder().FieldAccess("job.type").Equal().Constant("jobtype") },
        new object[] { "job.type == \"jobtype\" ", new TokenListBuilder().FieldAccess("job.type").Equal().Constant("jobtype") },
        new object[] { " job.type == \"jobtype\" ", new TokenListBuilder().FieldAccess("job.type").Equal().Constant("jobtype") },
        
        new object[] { " job.type>\"jobtype\" ", new TokenListBuilder().FieldAccess("job.type").Greater().Constant("jobtype") },
        new object[] { " job.type>=\"jobtype\" ", new TokenListBuilder().FieldAccess("job.type").GreaterOrEqual().Constant("jobtype") },
        new object[] { " job.type<\"jobtype\" ", new TokenListBuilder().FieldAccess("job.type").Less().Constant("jobtype") },
        new object[] { " job.type<=\"jobtype\" ", new TokenListBuilder().FieldAccess("job.type").LessOrEqual().Constant("jobtype") },
        new object[] { " job.type > \"jobtype\" ", new TokenListBuilder().FieldAccess("job.type").Greater().Constant("jobtype") },
        new object[] { " job.type >= \"jobtype\" ", new TokenListBuilder().FieldAccess("job.type").GreaterOrEqual().Constant("jobtype") },
        new object[] { " job.type < \"jobtype\" ", new TokenListBuilder().FieldAccess("job.type").Less().Constant("jobtype") },
        new object[] { " job.type <= \"jobtype\" ", new TokenListBuilder().FieldAccess("job.type").LessOrEqual().Constant("jobtype") },
        new object[] { " job.type <= \"jobtype\" ", new TokenListBuilder().FieldAccess("job.type").LessOrEqual().Constant("jobtype") },
        
        new object[] { "( job.type == \"jobtype\" ) || (job.name==\"Execute sync\")", new TokenListBuilder().Open().FieldAccess("job.type").Equal().Constant("jobtype").Close().Or().Open().FieldAccess("job.name").Equal().Constant("Execute sync").Close() },

        new object[] { " \"jobtype\" ", new TokenListBuilder().Constant("jobtype") },
        new object[] { " \"jobtype<T>\" ", new TokenListBuilder().Constant("jobtype<T>") },
        new object[] { " \"jobtype<=T>=\" ", new TokenListBuilder().Constant("jobtype<=T>=") },
        new object[] { " \"jobtype==T\" ", new TokenListBuilder().Constant("jobtype==T") },
    };
    
    [Theory]
    [MemberData(nameof(Tokenize_TestData))]
    public void Tokenizer_Should_Valid(string query, IEnumerable<Token> expectedTokens)
    {
        var actualTokens = new Tokenizer().Tokenize(query).ToList();
        Assert.Equal(expectedTokens, actualTokens);
    }
}