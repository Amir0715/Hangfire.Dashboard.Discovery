using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Hangfire.Dashboard.Blazor.Core.Abstractions;
using Hangfire.Dashboard.Blazor.Core.Abstractions.Tokens;
using Hangfire.Dashboard.Blazor.Core.Dtos;
using Hangfire.Dashboard.Blazor.Core.Validators;

namespace Hangfire.Dashboard.Blazor.Core.Services;

public class JobProvider : IJobProvider
{
    private readonly IValidator<IEnumerable<Token>> _tokensValidator;
    private readonly IValidator<FieldAccessToken> _fieldAccessValidator;
    private readonly ITokenizer _tokenizer;
    private readonly IExpressionGenerator _expressionGenerator;
    private readonly IJobRepository _jobRepository;

    public JobProvider(
        IValidator<IEnumerable<Token>> tokensValidator, 
        IValidator<FieldAccessToken> validator,
        ITokenizer tokenizer,
        IExpressionGenerator expressionGenerator,
        IJobRepository jobRepository
        )
    {
        _tokensValidator = tokensValidator ?? throw new ArgumentNullException(nameof(tokensValidator));
        _fieldAccessValidator = validator ?? throw new ArgumentNullException(nameof(validator));
        _tokenizer = tokenizer ?? throw new ArgumentNullException(nameof(tokenizer));
        _expressionGenerator = expressionGenerator ?? throw new ArgumentNullException(nameof(expressionGenerator));
        _jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
    }
    
    public async ValueTask<Result<List<JobContext>>> SearchJobs(QueryDto query)
    {
        var tokens = _tokenizer.Tokenize(query.QueryString).ToList();
        // ReSharper disable once MethodHasAsyncOverload
        var validationResult = _tokensValidator.Validate(tokens);
        if (!validationResult.IsValid)
        {
            return Result<List<JobContext>>.Failed(validationResult.ToString());
        }

        foreach (var fieldAccessToken in tokens.OfType<FieldAccessToken>())
        {
            // ReSharper disable once MethodHasAsyncOverload
            validationResult = _fieldAccessValidator.Validate(fieldAccessToken);
            if (!validationResult.IsValid)
            {
                return Result<List<JobContext>>.Failed(validationResult.ToString());
            }
        }

        var expression = _expressionGenerator.GenerateExpression(tokens);
        var jobContexts = await _jobRepository.SearchAsync(new SearchQuery()
        {
            QueryExpression = expression,
            StartDateTimeOffset = query.StartDateTimeOffset,
            EndDateTimeOffset = query.EndDateTimeOffset
        });
        return Result<List<JobContext>>.Success(jobContexts);
    }
}