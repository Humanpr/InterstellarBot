using interstarbot.Interfaces;
using interstarbot.MediaCommands;
using interstarbot.MediaContexts;
using MediatR;

namespace interstarbot.MediaHandlers;

public class MediaPublishCommandHandler : AsyncRequestHandler<MediaPublishCommand>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MediaPublishCommandHandler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override Task Handle(MediaPublishCommand request, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var mediaPublisher = scope.ServiceProvider.GetRequiredService<IPublishMedia>();
        
        return mediaPublisher.Publish(request.MediaContext);
    }
}