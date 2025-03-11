using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Validators;

namespace Hangfire.Dashboard.Blazor.Core.Validators;

public class IsValidParenthesisSequenceValidator<T> : PropertyValidator<T, string>
{
    public override string Name => "IsValidParenthesisSequenceValidator";
    
    public override bool IsValid(ValidationContext<T> context, string value)
    {
        return IsValidParenthesisSequence(value);
    }
    
    public static bool IsValidParenthesisSequence(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) 
            return false;
            
        var stack = new Stack<char>();
        foreach (var c in query)
        {
            if (c is '(' or '"')
            {
                stack.Push(c);
                continue;
            }

            if (c is ')')
            {
                if (!stack.Any())
                {
                    return false;
                }
                
                var lastChar = stack.Pop();
                if (lastChar != '(')
                {
                    return false;
                }
            } else if (c is '"')
            {
                if (!stack.Any())
                {
                    return false;
                }
                
                var lastChar = stack.Pop();
                if (lastChar != '"')
                {
                    return false;
                }
            }
        }

        return !stack.Any();
    }
}