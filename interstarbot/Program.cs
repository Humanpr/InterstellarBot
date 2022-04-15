using System.Text.RegularExpressions;
using System.Threading.Channels;
using FluentValidation;
using interstarbot.Behaviours;
using interstarbot.Services;
using interstarbot.Interfaces;
using interstarbot.MediaCommands;
using interstarbot.MediaContexts;
using interstarbot.MediaProcessing;
using interstarbot.Options;
using interstarbot.Twitter;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Tweetinvi;
using Tweetinvi.AspNet;
using Tweetinvi.Core.DTO;
using Tweetinvi.Models;
using HttpMethod = Tweetinvi.Models.HttpMethod;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<IMediaProcessor, MediaProcessor>();
builder.Services.AddSingleton<IPublishMedia,MediaReplier>();

builder.Services.AddSingleton<TwitterCredentials>((services) =>
{
    var configuration = services.GetRequiredService<IConfiguration>();
    TwitterCreds twitterCreds = new();
    // Setting twitter credentials
    configuration.GetSection(TwitterCreds.LogSection).Bind(twitterCreds);
    var credentials = new TwitterCredentials(twitterCreds.API_KEY, twitterCreds.API_KEY_SECRET, twitterCreds.ACCESS_TOKEN,
        twitterCreds.ACCESS_TOKEN_SECRET)
    {
        BearerToken = twitterCreds.BEARER
    };
    return credentials;
});

builder.Services.AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Singleton);

builder.Services.AddSingleton(Channel.CreateUnbounded<IProcessMediaCommand>());
builder.Services.AddSingleton(Channel.CreateUnbounded<IPublishMediaCommand>());

builder.Services.AddHostedService<WebhookRegisterer>();
builder.Services.AddHostedService<MediaProcessService>();
builder.Services.AddHostedService<MediaPublishSevice>();

builder.Services.AddMediatR(typeof(Program));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CommandValidationBehaviour<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));

builder.Services.AddEndpointsApiExplorer();
Plugins.Add<AspNetPlugin>(); 

var app = builder.Build();
using var serviceScope = app.Services.CreateScope();
var services = serviceScope.ServiceProvider;
var credentials = services.GetRequiredService<TwitterCredentials>();

ILogger<Program> _logger = services.GetRequiredService<ILogger<Program>>();

var twitterClient = new TwitterClient(credentials);
var requestHandler = twitterClient.AccountActivity.CreateRequestHandler();
var config = new WebhookMiddlewareConfiguration(requestHandler);

# region account_activity

var accountActivityStream = requestHandler.GetAccountActivityStream(1030093491559378944, "dev"); // todo user_id
ChannelWriter<IProcessMediaCommand> processMediaChannelWriter = services.GetRequiredService<Channel<IProcessMediaCommand>>().Writer;
accountActivityStream.TweetCreated += async (sender, tweetCreatedEvent) =>
{
    try
    {
        // Checking if received tweet contains any bot mention
        if (tweetCreatedEvent.Tweet.Entities.UserMentions.Any(user => user.Id == 1030093491559378944))
        {
            _logger.LogInformation($"A tweet was created by USER_ID or a tweet mentioning USER_ID {tweetCreatedEvent.GetType()} {tweetCreatedEvent.InResultOf.ToString()}");
            ITweet replyTweet = tweetCreatedEvent.Tweet;
            var tweetId = replyTweet.InReplyToStatusIdStr ?? replyTweet.IdStr;
            // getting media tweet
            var result = await twitterClient.Execute.RequestAsync<TweetDTO>(request =>
            {
                request.Url = $"https://api.twitter.com/1.1/statuses/show.json?id={tweetId}&tweet_mode=extended";
                request.HttpMethod = HttpMethod.GET;
            });

            ITweet mediaTweet = twitterClient.Factories.CreateTweet(result.Model);
            
            if (mediaTweet.Entities.Medias.Count is 0)
            {
                _logger.LogInformation("MEDIA NOT FOUND");
                return;
            }
            if (mediaTweet.Entities.Medias.Where(m => m.MediaType is "video").Count() is 0)
            {
                _logger.LogInformation("VIDEO NOT FOUND");
                return;
            }

            
            var mediaProcessCommand = new MediaProcessCommand(new MediaContext(mediaTweet,replyTweet));
            
            await processMediaChannelWriter.WriteAsync(mediaProcessCommand);
        }
    }
    catch (Exception e)
    {
        _logger.LogError(e.StackTrace);
    }
};
#endregion

app.UseTweetinviWebhooks(config);
app.UseAuthorization();
app.MapControllers();
app.Run();