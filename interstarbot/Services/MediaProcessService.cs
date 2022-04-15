using System.Globalization;
using System.Threading.Channels;
using interstarbot.Interfaces;
using interstarbot.MediaContexts;
using interstarbot.MediaCommands;
using MediatR;
using Tweetinvi.Parameters;

namespace interstarbot.Services;

public class MediaProcessService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private ChannelReader<IProcessMediaCommand> _processMediaCommandReader;
    private ChannelWriter<IPublishMediaCommand> _publishMediaCommandWriter;
    private ILogger<MediaProcessService> _logger;

    public MediaProcessService(Channel<IProcessMediaCommand> processMediaCommandChannel, Channel<IPublishMediaCommand> publishMediaCommandChannel, ILogger<MediaProcessService> logger, IServiceScopeFactory scopeFactory)
    {
        _processMediaCommandReader = processMediaCommandChannel.Reader;
        _publishMediaCommandWriter = publishMediaCommandChannel.Writer;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = new ParallelOptions { MaxDegreeOfParallelism = 2 };
        
        await Parallel.ForEachAsync(_processMediaCommandReader.ReadAllAsync(stoppingToken),options,async (command,token) =>
        {
            using var scope = _scopeFactory.CreateScope();
            var mediatR = scope.ServiceProvider.GetRequiredService<IMediator>();
            
            _logger.LogInformation("New process task received. {id}",command.MediaContext.MediaName);
            try
            {
             await mediatR.Send(command);
            }
            catch (Exception e)
            {
                _logger.LogError(e.StackTrace);
                _logger.LogError("Process command resulted with failure. MediaContext ID {id} ",command.MediaContext.MediaName);
                return;
            }
            _logger.LogInformation("Process task finished. {id}",command.MediaContext.MediaName);
            
            // Creating new publish command. Setting media context and passing to the publish channel.d
            var publishCommand = new MediaPublishCommand(command.MediaContext);
            
            await _publishMediaCommandWriter.WriteAsync(publishCommand);
        });
    }
}