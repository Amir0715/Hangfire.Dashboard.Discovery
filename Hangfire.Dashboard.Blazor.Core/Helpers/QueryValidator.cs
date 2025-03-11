using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Hangfire.Dashboard.Blazor.Core.Tokenization.Tokens;

namespace Hangfire.Dashboard.Blazor.Core.Helpers;

public static class QueryValidator
{
    /// <summary>
    /// Валидирует строку на правильную скобочную последовательность.  
    /// </summary>
    /// <remarks>Игнорирует невалидные скобочные выражения внутри <c>"</c>.</remarks>
    public static bool IsValidParenthesisSequence(string query)
    {
        // TODO: переделать на Result паттерн
        // TODO: избавиться и реализовать валидатор на основе токенов
        if (string.IsNullOrWhiteSpace(query))
            return false;

        var stack = new Stack<char>();
        var insideQuotes = false;

        foreach (var c in query)
        {
            if (c == '"')
            {
                insideQuotes = !insideQuotes;
                continue;
            }

            if (insideQuotes)
                continue;

            if (c == '(')
            {
                stack.Push(c);
            }
            else if (c == ')')
            {
                if (stack.Count == 0 || stack.Pop() != '(')
                {
                    return false;
                }
            }
        }

        return stack.Count == 0 && !insideQuotes;
    }

    public static bool IsValidTokenSequence(IEnumerable<Token> tokens)
    {
        var tokensList = tokens.ToList();

        // Проверка на пустой список
        if (tokensList.Count == 0)
            return false;

        // Проверка наличия хотя бы одного оператора сравнения
        if (!tokensList.Any(t => t is OperatorToken { Operator: not OperatorType.And and not OperatorType.Or }))
            return false;

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
                    return false;
                var openIndex = parenthesisStack.Pop();
                parenthesisPairs.Add((openIndex, i));
            }
        }

        // Незакрытые скобки
        if (parenthesisStack.Count != 0)
            return false;

        // Проверка на пустые скобки
        foreach (var pair in parenthesisPairs)
        {
            if (pair.Close - pair.Open < 2)
                return false;
        }

        // Проверка операторов
        for (var i = 0; i < tokensList.Count; i++)
        {
            var current = tokensList[i];
            if (current is not OperatorToken op) continue;

            if (i == 0 || i == tokensList.Count - 1)
                return false;

            var prev = tokensList[i - 1];
            var next = tokensList[i + 1];

            if (IsComparisonOperator(op.Operator))
            {
                // Оператор сравнения: между двумя операндами
                if (!IsOperand(prev) || !IsOperand(next))
                    return false;
            }
            else if (IsLogicalOperator(op.Operator))
            {
                // Логический оператор: между выражениями (скобками или сравнениями)
                if (i - 3 < 0 || i + 1 + 3 > tokensList.Count)
                    return false;
                
                var prevSlice = CollectionsMarshal.AsSpan(tokensList).Slice(i - 3, 3);
                var nextSlice = CollectionsMarshal.AsSpan(tokensList).Slice(i + 1, 3);
                bool validPrev = IsExpressionEnd(prevSlice);
                bool validNext = IsExpressionStart(nextSlice);

                if (!validPrev || !validNext)
                    return false;
            }
        }

        // Проверка на два операнда подряд
        for (var i = 0; i < tokensList.Count - 1; i++)
        {
            var current = tokensList[i];
            var next = tokensList[i + 1];
            if (IsOperand(current) && IsOperand(next))
                return false;
        }

        // Проверка на закрывающую скобку перед операндом или открывающей скобкой без оператора
        for (var i = 0; i < tokensList.Count - 1; i++)
        {
            var current = tokensList[i];
            var next = tokensList[i + 1];

            if (current is ParenToken { Paren: ParenType.Close } &&
                (IsExpressionStart(next) || next is ParenToken { Paren: ParenType.Open }))
            {
                return false;
            }
        }

        return true;
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