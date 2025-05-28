using System;
using System.Collections.Generic;
using System.Globalization;
using Hangfire.Dashboard.Blazor.Core.Abstractions;
using Hangfire.Dashboard.Blazor.Core.Abstractions.Tokens;

namespace Hangfire.Dashboard.Blazor.Core.Tokenization;

public class Tokenizer : ITokenizer
{
    public IEnumerable<Token> Tokenize(string query)
    {
        query ??= string.Empty;
        query = query.Trim();

        var tokens = new List<Token>();
        char? lastChar = null;
        var word = new List<char>(10);
        var quotationOpenIsMet = false;

        foreach (var @char in query)
        { 
start:
            switch (lastChar, @char)
            {
                case (_, '(') when !quotationOpenIsMet:
                    tokens.Add(new ParenToken(ParenType.Open));
                    AddWordAndClear(tokens, word);
                    break;
                case (_, ')') when !quotationOpenIsMet:
                    AddWordAndClear(tokens, word);
                    tokens.Add(new ParenToken(ParenType.Close));
                    break;
                case ('=', '=') when !quotationOpenIsMet:
                    tokens.Add(new OperatorToken(OperatorType.Equal));
                    lastChar = null;
                    break;
                case ('!', '=') when !quotationOpenIsMet:
                    tokens.Add(new OperatorToken(OperatorType.NotEqual));
                    lastChar = null;
                    break;
                case ('~', '=') when !quotationOpenIsMet:
                    tokens.Add(new OperatorToken(OperatorType.Like));
                    lastChar = null;
                    break;
                case ('>', '=') when !quotationOpenIsMet:
                    tokens.Add(new OperatorToken(OperatorType.GreaterThanOrEqual));
                    lastChar = null;
                    break;
                case ('<', '=') when !quotationOpenIsMet:
                    tokens.Add(new OperatorToken(OperatorType.LessThanOrEqual));
                    lastChar = null;
                    break;
                case ('&', '&') when !quotationOpenIsMet:
                    tokens.Add(new OperatorToken(OperatorType.And));
                    lastChar = null;
                    break;
                case ('|', '|') when !quotationOpenIsMet:
                    tokens.Add(new OperatorToken(OperatorType.Or));
                    lastChar = null;
                    break;
                case ('<', _) when !quotationOpenIsMet:
                    tokens.Add(new OperatorToken(OperatorType.LessThan));
                    lastChar = null;
                    goto start;
                    break;
                case ('>', _) when !quotationOpenIsMet:
                    tokens.Add(new OperatorToken(OperatorType.GreaterThan));
                    lastChar = null;
                    goto start;
                    break;
                case (_, '!' or '=' or '~' or '>' or '<' or '&' or '|') when !quotationOpenIsMet:
                    lastChar = @char;
                    AddWordAndClear(tokens, word);
                    break;
                case (_, '"'):
                    if (quotationOpenIsMet)
                    {
                        AddConstantAndClear(tokens, word);
                        quotationOpenIsMet = false;
                    }
                    else
                    {
                        quotationOpenIsMet = true;
                        word.Clear();
                    }

                    lastChar = null;
                    break;
                case (null, _):
                    word.Add(@char);
                    break;
            }
        }

        switch (lastChar)
        {
            case '>': 
                tokens.Add(new OperatorToken(OperatorType.GreaterThan));
                break;
            case '<':
                tokens.Add(new OperatorToken(OperatorType.LessThan));
                break;
        }

        if (word.Count > 0)
        {
            AddWordAndClear(tokens, word);
        }
        
        return tokens;
    }

    private static void AddWordAndClear(List<Token> tokens, List<char>? word)
    {
        if (word is not { Count: > 0 }) return;
        var wordString = string.Join(null, word).Trim();
        if (string.IsNullOrWhiteSpace(wordString)) return;
        
        // Проверяем, является ли токен числом или датой
        if (float.TryParse(wordString, CultureInfo.InvariantCulture, out var floatNumber))
        {
            tokens.Add(new NumberToken(floatNumber));
        }
        else
        {
            tokens.Add(new FieldAccessToken(wordString));
        }
        
        word.Clear();
    }
    
    private static void AddConstantAndClear(List<Token> tokens, List<char>? word)
    {
        if (word is not { Count: > 0 }) return;
        var wordString = string.Join(null, word);
        
        if (string.IsNullOrWhiteSpace(wordString))
        {
            tokens.Add(new StringToken(wordString));
        } else if (DateTimeOffset.TryParse(wordString, out var dateTimeOffset))
        {
            tokens.Add(new DateTimeOffsetToken(dateTimeOffset));
        }
        else
        {
            tokens.Add(new StringToken(wordString));
        }
        
        word.Clear();
    }
}