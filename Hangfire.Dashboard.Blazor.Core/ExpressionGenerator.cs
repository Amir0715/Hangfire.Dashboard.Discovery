using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using Hangfire.Dashboard.Blazor.Core.Abstractions;
using Hangfire.Dashboard.Blazor.Core.Abstractions.Tokens;

namespace Hangfire.Dashboard.Blazor.Core;

public class ExpressionGenerator : IExpressionGenerator
{
    public Expression<Func<JobContext, bool>> GenerateExpression(IEnumerable<Token> tokens)
    {
        using var tokenEnumerator = tokens.GetEnumerator();
        var jobParameter = Expression.Parameter(typeof(JobContext), "jobCtx");
        var expression = GenerateExpression(tokenEnumerator, jobParameter);
        return Expression.Lambda<Func<JobContext, bool>>(expression, jobParameter);
    }

    private Expression GenerateExpression(IEnumerator<Token> tokenEnumerator, ParameterExpression jobParameter)
    {
        Expression? currentExpression = null;
        OperatorToken? lastOperatorToken = null;
        while (tokenEnumerator.MoveNext())
        {
            var token = tokenEnumerator.Current;
            switch (lastOperatorType: lastOperatorToken, token)
            {
                case (null, FieldAccessToken fat):
                    currentExpression = Access(jobParameter, fat);
                    break;
                case (null, StringToken constantToken):
                    currentExpression = Expression.Constant(constantToken.Value);
                    break;
                case (null, NumberToken constantToken):
                    currentExpression = Expression.Constant(constantToken.Value);
                    break;
                case (null, OperatorToken { Operator: OperatorType.And or OperatorType.Or } op):
                    var nextBooleanExpression = GetNextBoolExpression(tokenEnumerator, jobParameter);
                    currentExpression = BinaryExpression(currentExpression!, op, nextBooleanExpression);
                    break;
                case (null, OperatorToken op):
                    lastOperatorToken = op;
                    break;
                case ({ } operatorToken, ParenToken { Paren: ParenType.Open }):
                    var parenExpression = GenerateExpression(tokenEnumerator, jobParameter);
                    currentExpression = BinaryExpression(currentExpression!, operatorToken, parenExpression);
                    break;
                case (null, ParenToken { Paren: ParenType.Open }):
                    currentExpression = GenerateExpression(tokenEnumerator, jobParameter);
                    break;
                case ({ } operatorToken, StringToken constantToken):
                    currentExpression = BinaryExpression(currentExpression!, operatorToken, constantToken);
                    lastOperatorToken = null;
                    break;
                case ({ } operatorToken, NumberToken constantToken):
                    currentExpression = BinaryExpression(currentExpression!, operatorToken, constantToken);
                    lastOperatorToken = null;
                    break;
                case ({ } operatorToken, FieldAccessToken fat):
                    currentExpression = BinaryExpression(currentExpression!, operatorToken, Access(jobParameter, fat));
                    lastOperatorToken = null;
                    break;
                case ({ } operatorType, ParenToken { Paren: ParenType.Close }):
                    return currentExpression;
            }
        }

        return currentExpression;
    }

    private Expression GetNextBoolExpression(IEnumerator<Token> tokenEnumerator, ParameterExpression jobParameter)
    {
        Expression? prevExpression = null;
        OperatorToken? lastOperatorToken = null;
        while (tokenEnumerator.MoveNext())
        {
            switch (lastOperatorToken, tokenEnumerator.Current)
            {
                case (_, ParenToken):
                    return GenerateExpression(tokenEnumerator, jobParameter);

                case (null, FieldAccessToken fat):
                    prevExpression = Access(jobParameter, fat);
                    break;

                case (null, StringToken constantToken):
                    prevExpression = Expression.Constant(constantToken.Value);
                    break;

                case (null, NumberToken constantToken):
                    prevExpression = Expression.Constant(constantToken.Value);
                    break;

                case (null, OperatorToken operatorToken):
                    lastOperatorToken = operatorToken;
                    break;

                case ({ }, StringToken constantToken):
                    return BinaryExpression(prevExpression, lastOperatorToken, constantToken);

                case ({ }, NumberToken constantToken):
                    return BinaryExpression(prevExpression, lastOperatorToken, constantToken);

                case ({ }, FieldAccessToken fat):
                    return BinaryExpression(prevExpression, lastOperatorToken, Access(jobParameter, fat));
            }
        }

        throw new ArgumentException("invalid token sequance", nameof(tokenEnumerator));
    }

    private static Expression Access(Expression expression, FieldAccessToken fieldAccessToken)
    {
        Span<Range> sectionsSpanRanges = stackalloc Range[fieldAccessToken.Depth];
        fieldAccessToken.FieldPath.AsSpan().Split(sectionsSpanRanges, '.');

        Expression propOrField = Expression.PropertyOrField(expression,
            fieldAccessToken.FieldPath.AsSpan(sectionsSpanRanges[0]).ToString()
        );
        var jsonDocumentAccess = false;
        foreach (var sectionRange in sectionsSpanRanges[1..])
        {
            var fieldName = fieldAccessToken.FieldPath.AsSpan(sectionRange).ToString();

            // Если мы встретили JsonDocument, то и все под вызовы начиная с него надо делать через GetProperty
            // Args.Name.list.queue
            if (propOrField.Type == typeof(JsonDocument))
            {
                var rootElement = Expression.PropertyOrField(propOrField, nameof(JsonDocument.RootElement));
                propOrField = Expression.Call(rootElement, nameof(JsonElement.GetProperty), [],
                    Expression.Constant(fieldName));
            }
            else if (propOrField.Type == typeof(JsonElement))
            {
                propOrField = Expression.Call(propOrField, nameof(JsonElement.GetProperty), [],
                    Expression.Constant(fieldName));
            }
            else
            {
                propOrField = Expression.PropertyOrField(propOrField, fieldName);
            }
        }

        return propOrField;
    }

    private static Expression BinaryExpression(
        Expression left,
        OperatorToken operatorToken,
        StringToken stringToken
    )
    {
        return BinaryExpression(left, operatorToken, Expression.Constant(stringToken.Value));
    }

    private static Expression BinaryExpression(
        Expression left,
        OperatorToken operatorToken,
        NumberToken numberToken
    )
    {
        return BinaryExpression(left, operatorToken, Expression.Constant(numberToken.Value));
    }

    private static Expression BinaryExpression(
        Expression left,
        OperatorToken operatorToken,
        Expression right
    )
    {
        if (left.Type == typeof(float) && right.Type == typeof(JsonElement))
        {
            right = Expression.Call(right, nameof(JsonElement.GetSingle), []);
        } else if (right.Type == typeof(float) && left.Type == typeof(JsonElement))
        {
            left = Expression.Call(left, nameof(JsonElement.GetSingle), []);
        } else if (left.Type == typeof(string) && right.Type == typeof(JsonElement))
        {
            right = Expression.Call(right, nameof(JsonElement.GetString), []);
        } else if (right.Type == typeof(string) && left.Type == typeof(JsonElement))
        {
            left = Expression.Call(left, nameof(JsonElement.GetString), []);
        }
        
        return operatorToken.Operator switch
        {
            OperatorType.And => Expression.AndAlso(left, right),
            OperatorType.Or => Expression.OrElse(left, right),
            OperatorType.Equal => Expression.Equal(left, right),
            OperatorType.NotEqual => Expression.NotEqual(left, right),
            OperatorType.GreaterThan => Expression.GreaterThan(left, right),
            OperatorType.GreaterThanOrEqual => Expression.GreaterThanOrEqual(left,
                right),
            OperatorType.LessThan => Expression.LessThan(left, right),
            OperatorType.LessThanOrEqual => Expression.LessThanOrEqual(left,
                right),
            OperatorType.Like => Expression.Call(left, nameof(string.Contains), [], right),
            _ => throw new ArgumentOutOfRangeException(nameof(operatorToken), operatorToken.Type, "Out of range")
        };
    }
}