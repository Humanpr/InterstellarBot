namespace interstarbot.Interfaces;

public interface IMediaProcessor
{
    public Task Process(CancellationToken cancellationToken,IMediaContext mediaContext);
}