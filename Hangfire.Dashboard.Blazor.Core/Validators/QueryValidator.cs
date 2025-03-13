using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Hangfire.Dashboard.Blazor.Core.Abstractions.Tokens;

namespace Hangfire.Dashboard.Blazor.Core.Validators;

public static class QueryValidator
{
    public static Result IsValidTokenSequence(IEnumerable<Token> tokens)
    {
        var tokensList = tokens.ToList();

        // Проверка на пустой список
        if (tokensList.Count == 0)
            return Result.Failed("Последовательность токенов не может быть пустым");

        // Проверка наличия хотя бы одного оператора сравнения
        if (!tokensList.Any(t => t is OperatorToken { Operator: not OperatorType.And and not OperatorType.Or }))
            return Result.Failed("Выражение должно содержать хотя-бы один оператор сравнения (>, >=, <, <=, ==, !=, ~=)");

        var firstOperatorIndex = tokensList.FindIndex(t => t.Type == TokenType.Operator);
        var lastOperatorIndex = tokensList.FindLastIndex(t => t.Type == TokenType.Operator);
        
        if (firstOperatorIndex == 0 || lastOperatorIndex == tokensList.Count - 1)
            return Result.Failed("Выражение не может начинаться или заканчиваться на оператор");
        
        // Проверка скобочной структуры и сбора пар
        var parenthesisStack = new Stack<int>();
        var parenthesisPairs = new List<(int Open, int Close)>();
        for (var i = 0; i < tokensList.Count; i++)
        {
            var token = tokensList[i];
            if (token is ParenToken { Paren: ParenType.Open })
            {
                parenthesisStack.Push(i);
            }
            else if (token is ParenToken { Paren: ParenType.Close })
            {
                if (parenthesisStack.Count == 0)
                    return Result.Failed("Все закрывающие скобки должны иметь пары");
                
                var openIndex = parenthesisStack.Pop();
                parenthesisPairs.Add((openIndex, i));
            }
        }

        // Незакрытые скобки
        if (parenthesisStack.Count != 0)
            return Result.Failed("Все открывающие скобки должны иметь пары");

        // Проверка на пустые скобки
        foreach (var pair in parenthesisPairs)
        {
            if (pair.Close - pair.Open < 2)
                return Result.Failed("Выражение не может содержать пустую пару скобок");
        }

        // Проверка операторов
        for (var i = 0; i < tokensList.Count; i++)
        {
            var current = tokensList[i];
            if (current is not OperatorToken op) continue;

            var prev = tokensList[i - 1];
            var next = tokensList[i + 1];

            if (IsComparisonOperator(op.Operator))
            {
                // Оператор сравнения: между двумя операндами
                if (!IsOperand(prev) || !IsOperand(next))
                    return Result.Failed("Оператор сравнения должен быть между двумя операндами");
            }
            else if (IsLogicalOperator(op.Operator))
            {
                // Логический оператор: между выражениями (скобками или сравнениями)
                if (i - 3 < 0 || i + 1 + 3 > tokensList.Count)
                    return Result.Failed("Неправильное расположение логического оператора");
                
                var prevSlice = CollectionsMarshal.AsSpan(tokensList).Slice(i - 3, 3);
                var nextSlice = CollectionsMarshal.AsSpan(tokensList).Slice(i + 1, 3);
                bool validPrev = IsExpressionEnd(prevSlice);
                bool validNext = IsExpressionStart(nextSlice);

                if (!validPrev || !validNext)
                    return Result.Failed("Неправильное расположение логического оператора");
            }
        }

        // Проверка на два операнда подряд
        for (var i = 0; i < tokensList.Count - 1; i++)
        {
            var current = tokensList[i];
            var next = tokensList[i + 1];
            if (IsOperand(current) && IsOperand(next))
                return Result.Failed("После операнда ожидается оператор сравнения");
        }

        // Проверка на закрывающую скобку перед операндом или открывающей скобкой без оператора
        for (var i = 0; i < tokensList.Count - 1; i++)
        {
            var current = tokensList[i];
            var next = tokensList[i + 1];

            if (current is ParenToken { Paren: ParenType.Close } &&
                (IsExpressionStart(next) || next is ParenToken { Paren: ParenType.Open }))
            {
                return Result.Failed("Неправильно составление выражение");
            }
        }

        return Result.Success();
    }

    // Вспомогательные методы
    private static bool IsComparisonOperator(OperatorType op)
    {
        return op != OperatorType.And && op != OperatorType.Or;
    }

    private static bool IsLogicalOperator(OperatorType op)
    {
        return op is OperatorType.And or OperatorType.Or;
    }

    private static bool IsOperand(Token token)
    {
        return token is FieldAccessToken or ConstantToken;
    }

    private static bool IsExpressionEnd(ReadOnlySpan<Token> tokens)
    {
        return tokens[^1] is ParenToken { Paren: ParenType.Close } || IsExpression(tokens);
    }

    private static bool IsExpressionStart(ReadOnlySpan<Token> tokens)
    {
        return tokens[0] is ParenToken { Paren: ParenType.Open } || IsExpression(tokens);
    }
    
    private static bool IsExpressionStart(Token token)
    {
        return token is ParenToken { Paren: ParenType.Open } || IsOperand(token);
    }

    private static bool IsExpression(ReadOnlySpan<Token> tokens)
    {
        return
            (tokens[0] is { Type: TokenType.FieldAccess } &&
             tokens[1] is OperatorToken { Operator: not OperatorType.And and not OperatorType.Or } &&
             tokens[2] is { Type: TokenType.Constant }) || 
            
            (tokens[0] is { Type: TokenType.Constant } &&
             tokens[1] is OperatorToken { Operator: not OperatorType.And and not OperatorType.Or } &&
             tokens[2] is { Type: TokenType.FieldAccess });
    }
}