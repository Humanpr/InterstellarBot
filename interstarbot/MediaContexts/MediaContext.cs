using System.Text.RegularExpressions;
using interstarbot.Interfaces;
using Tweetinvi.Models;

namespace interstarbot.MediaContexts;

public class MediaContext : IMediaContext
{
    public string MediaName { get; }
    public string ReplyTweetIDStr { get;  }
    public string ReplyTweetUserHandle { get;  }
    public long? ReplyTweetID { get; }
    public string MediaUrl { get; }
    public decimal? StartTime { get; } = -1;
    public decimal? EndTime { get; } = -1;
    public int? VideoDuration { get; }

    private const string TimeIntervalPattern = @"start ([0-9][0-9]?[0-9]?) end ([0-9][0-9]?[0-9]?)$";
    public MediaContext(ITweet mediaTweet,ITweet replyTweet)
    {
        MediaName = mediaTweet.IdStr;
        
        var matches = Regex.Match(replyTweet.Text, TimeIntervalPattern).Groups;
        if (matches.Count == 3)
        {
            StartTime = Convert.ToDecimal(matches[1].Value);
            EndTime = Convert.ToDecimal(matches[2].Value);
        }
        
        MediaUrl = mediaTweet.Entities?.Medias.First(m => m.MediaType is "video").VideoDetails.Variants.First().URL;
        VideoDuration = mediaTweet.Entities?.Medias.First(m => m.MediaType is "video").VideoDetails.DurationInMilliseconds ?? -1;
        ReplyTweetUserHandle = replyTweet.CreatedBy.ToString() ?? throw new ArgumentNullException(nameof(ReplyTweetUserHandle),"Reply user handle can't be null!");
        ReplyTweetID = replyTweet.Id;
        ReplyTweetIDStr = replyTweet.IdStr;
    }
}
