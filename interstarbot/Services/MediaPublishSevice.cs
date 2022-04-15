using System.Threading.Channels;
using interstarbot.Interfaces;
using MediatR;

namespace interstarbot.Services;

public class MediaPublishSevice : BackgroundService
{
    private ChannelReader<IPublishMediaCommand> _publishCommandReader;
    private ILogger<MediaPublishSevice> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public MediaPublishSevice(Channel<IPublishMediaCommand> publishCommandReader, ILogger<MediaPublishSevice> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _publishCommandReader = publishCommandReader.Reader;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = new ParallelOptions { MaxDegreeOfParallelism = 2 };
        
        await Parallel.ForEachAsync(_publishCommandReader.ReadAllAsync(), options,async (command,cancelationToken) =>
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var mediatR = scope.ServiceProvider.GetRequiredService<IMediator>();
            
            _logger.LogInformation($"New publish task received {command.MediaContext.MediaName}");

            try
            {
                await mediatR.Send(command);
            }
            catch (Exception e)
            {
                _logger.LogError(e.StackTrace);
                _logger.LogError("Publish command resulted with failure. MediaContext ID {id} ",command.MediaContext.MediaName);
                return;
            }
            
            _logger.LogInformation($"Published Media ID {command.MediaContext.MediaName} to ReplyTweetID {command.MediaContext.ReplyTweetID}");
        });
    }
}