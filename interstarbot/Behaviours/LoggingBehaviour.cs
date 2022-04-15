using interstarbot.Interfaces;
using MediatR;

namespace interstarbot.Behaviours;

public class LoggingBehaviour<TRequest,TResponse> : IPipelineBehavior<TRequest,TResponse> where TRequest : IRequest<TResponse>,ICommand
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest,TResponse>> logger)
    {
        _logger = logger;
    }
    
    public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        _logger.LogInformation("New request came in {type} with id {medianame}",typeof(TRequest),request.MediaContext.MediaName);
        return next();
    }
}