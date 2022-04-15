using System.Security.Principal;

namespace interstarbot.Interfaces;

public interface IMediaContext
{
    public string MediaUrl { get;}
    public string MediaName { get;}
    public string ReplyTweetIDStr { get;}
    public string ReplyTweetUserHandle { get; }
    public long? ReplyTweetID { get;}
    public decimal? StartTime { get; }
    public decimal? EndTime { get; }
    public int? VideoDuration { get; }
}