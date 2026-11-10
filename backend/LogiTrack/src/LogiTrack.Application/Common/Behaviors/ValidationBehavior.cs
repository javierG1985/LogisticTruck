using FluentValidation;
using Mediator;

namespace LogiTrack.Application.Common.Behaviors;

public sealed class ValidationBehavior<TMessage, TResponse>(IEnumerable<IValidator<TMessage>> validators)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var validatorList = validators.ToList();

        if (validatorList.Count > 0)
        {
            var context = new ValidationContext<TMessage>(message);
            var results = await Task.WhenAll(
                validatorList.Select(validator => validator.ValidateAsync(context, cancellationToken)));

            var failures = results
                .SelectMany(result => result.Errors)
                .ToList();

            if (failures.Count > 0)
            {
                throw new ValidationException(failures);
            }
        }

        return await next(message, cancellationToken);
    }
}
