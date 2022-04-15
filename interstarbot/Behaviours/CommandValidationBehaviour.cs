using FluentValidation;
using interstarbot.Interfaces;
using interstarbot.MediaContexts;
using MediatR;

namespace interstarbot.Behaviours;

public class CommandValidationBehaviour<TRequest,TResponse> : IPipelineBehavior<TRequest,TResponse> where TRequest : IRequest<TResponse>,ICommand
{
    private readonly ILogger<CommandValidationBehaviour<TRequest, TResponse>> _logger;
    private readonly IValidator<MediaContext> _mediaContextValidator;
    
    public CommandValidationBehaviour(ILogger<CommandValidationBehaviour<TRequest,TResponse>> logger,IValidator<MediaContext> mediaContextValidator)
    {
        _logger = logger;
        _mediaContextValidator = mediaContextValidator;
    }
    
    public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        _mediaContextValidator.ValidateAndThrow((MediaContext) request.MediaContext);
        _logger.LogInformation($"Validation succeeded {request.MediaContext.MediaName}. Command type {typeof(TRequest)}");
        return next();
    }
}