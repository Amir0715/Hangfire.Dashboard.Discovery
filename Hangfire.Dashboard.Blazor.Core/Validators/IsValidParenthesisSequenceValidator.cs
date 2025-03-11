using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Validators;

namespace Hangfire.Dashboard.Blazor.Core.Validators;

/// <summary>
/// Валидатор для проверки скобочных выражений. Игнорирует невалидные скобочные выражения внутри ". 
/// </summary>
/// <typeparam name="T"></typeparam>
public class IsValidParenthesisSequenceValidator<T> : PropertyValidator<T, string>
{
    public override string Name => "IsValidParenthesisSequenceValidator";
    
    public override bool IsValid(ValidationContext<T> context, string value)
    {
        return IsValidParenthesisSequence(value);
    }
    
    /// <summary>
    /// Валидирует строку на правильную скобочную последовательность.  
    /// </summary>
    /// <remarks>Игнорирует невалидные скобочные выражения внутри <c>"</c>.</remarks>
    public static bool IsValidParenthesisSequence(string query)
    {
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
}