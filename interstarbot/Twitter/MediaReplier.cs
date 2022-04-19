using interstarbot.Interfaces;
using interstarbot.MediaContexts;
using interstarbot.Options;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace interstarbot.Twitter;

public class MediaReplier : IPublishMedia
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<MediaReplier> _logger;

    public MediaReplier(IServiceScopeFactory serviceScopeFactory, ILogger<MediaReplier> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task Publish(IMediaContext mediaContext)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var _configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var credentials = scope.ServiceProvider.GetRequiredService<TwitterCredentials>();
        // todo singleton twitter client?
        var userClient = new TwitterClient(credentials);
        
        var processedUri = _configuration.GetSection("MediaIO").GetValue<string>("ProcessedMediaLocation");
        var outpath = @$"{processedUri}{Path.DirectorySeparatorChar}{mediaContext.MediaName}.mp4";

        var videoBinary = File.ReadAllBytes(outpath);
        var uploadedVideo = await userClient.Upload.UploadTweetVideoAsync(videoBinary);
        await  userClient.Upload.WaitForMediaProcessingToGetAllMetadataAsync(uploadedVideo);
        
        _logger.LogInformation($" Tweet with {mediaContext.ReplyTweetID} id created by {mediaContext.ReplyTweetUserHandle} user.. ");
        
        var reply = await userClient.Tweets.PublishTweetAsync(new PublishTweetParameters("@" + mediaContext.ReplyTweetUserHandle + " here is edited")
        {
            InReplyToTweetId = mediaContext.ReplyTweetID,
            Medias = {uploadedVideo}
        });
        
    }
}