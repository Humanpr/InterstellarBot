using interstarbot.Interfaces;
using interstarbot.MediaContexts;
using MediatR;

namespace interstarbot.MediaCommands;

public class MediaProcessCommand : IRequest, IProcessMediaCommand
{
    public IMediaContext MediaContext { get;}
    public MediaProcessCommand(IMediaContext mediaContext)
    {
        MediaContext = mediaContext;
    }
}