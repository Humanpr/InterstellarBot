using System.Threading.Channels;
using interstarbot.Interfaces;
using interstarbot.Options;
using interstarbot.MediaContexts;
using MediatR;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace interstarbot.MediaCommands;

public class MediaPublishCommand :  IRequest , IPublishMediaCommand
{
    public IMediaContext MediaContext { get; }
    public MediaPublishCommand(IMediaContext mediaContext)
    {
        MediaContext = mediaContext;
    }
}