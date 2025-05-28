namespace Hangfire.Dashboard.Blazor.Core.Tests.Helpers;

public static class TokenListBuilderExtensions
{
    public static TokenListBuilder DateTime(this TokenListBuilder builder, DateTime value) =>
        builder.DateTime(new DateTimeOffset(value));
    public static TokenListBuilder DateTime(this TokenListBuilder builder, string value) =>
        builder.DateTime(DateTimeOffset.Parse(value));

    public static TokenListBuilder Equal(this TokenListBuilder builder, string value) => 
        builder.Equal().String(value);
    
    public static TokenListBuilder Equal(this TokenListBuilder builder, float value) => 
        builder.Equal().Number(value);
    
    public static TokenListBuilder Equal(this TokenListBuilder builder, DateTimeOffset value) => 
        builder.Equal().DateTime(value);
    
    public static TokenListBuilder NotEqual(this TokenListBuilder builder, string value) => 
        builder.NotEqual().String(value);
    
    public static TokenListBuilder NotEqual(this TokenListBuilder builder, float value) => 
        builder.NotEqual().Number(value);
    
    public static TokenListBuilder NotEqual(this TokenListBuilder builder, DateTimeOffset value) => 
        builder.NotEqual().DateTime(value);
    
    public static TokenListBuilder Like(this TokenListBuilder builder, string value) => 
        builder.Like().String(value);
    
    public static TokenListBuilder Greater(this TokenListBuilder builder, float value) => 
        builder.Greater().Number(value);
    
    public static TokenListBuilder Greater(this TokenListBuilder builder, DateTimeOffset value) => 
        builder.Greater().DateTime(value);
    
    public static TokenListBuilder GreaterOrEqual(this TokenListBuilder builder, float value) => 
        builder.GreaterOrEqual().Number(value);
    
    public static TokenListBuilder GreaterOrEqual(this TokenListBuilder builder, DateTimeOffset value) => 
        builder.GreaterOrEqual().DateTime(value);
    
    public static TokenListBuilder Less(this TokenListBuilder builder, float value) => 
        builder.Less().Number(value);
    
    public static TokenListBuilder Less(this TokenListBuilder builder, DateTimeOffset value) => 
        builder.Less().DateTime(value);
    
    public static TokenListBuilder LessOrEqual(this TokenListBuilder builder, float value) => 
        builder.Less().Number(value);
    
    public static TokenListBuilder LessOrEqual(this TokenListBuilder builder, DateTimeOffset value) => 
        builder.Less().DateTime(value);
}