using Application.Common.Models;
using FluentValidation;
using MediatR;

namespace Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : class
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if(!validators.Any())
        {
            return await next(cancellationToken).ConfigureAwait(false);
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)))
            .ConfigureAwait(false);

        var failures = validationResults.Where(r => r.Errors.Count > 0).SelectMany(r => r.Errors).ToList();

        if(failures.Count > 0)
        {
            var errors = failures.Select(
                f => Error.Validation(f.ErrorMessage, f.PropertyName, f.CustomState?.ToString()))
                .ToList();

            var responseType = typeof(TResponse);

            if(responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var failureMethod = responseType.GetMethod(nameof(Result<>.Failure), [ typeof(List<Error>) ]);
                return (TResponse)failureMethod!.Invoke(null, [ errors ])!;
            } else if(responseType == typeof(Result))
            {
                return (TResponse)(object)Result.Failure(errors);
            }
        }

        return await next(cancellationToken).ConfigureAwait(false);
    }
}