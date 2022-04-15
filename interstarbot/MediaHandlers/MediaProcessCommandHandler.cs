using interstarbot.Interfaces;
using interstarbot.MediaCommands;
using MediatR;

namespace interstarbot.MediaHandlers;

public class MediaProcessCommandHandler : AsyncRequestHandler<MediaProcessCommand>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MediaProcessCommandHandler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override Task Handle(MediaProcessCommand request, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var mediaProcessor = scope.ServiceProvider.GetRequiredService<IMediaProcessor>();
        return mediaProcessor.Process(cancellationToken,request.MediaContext);
    }
}