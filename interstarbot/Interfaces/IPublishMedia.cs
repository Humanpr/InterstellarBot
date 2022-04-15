namespace interstarbot.Interfaces;

public interface IPublishMedia
{
    public Task Publish(IMediaContext mediaContext);
}