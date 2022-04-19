using System.Diagnostics;
using interstarbot.Interfaces;

namespace interstarbot.MediaProcessing;

public class MediaProcessor : IMediaProcessor
{
    private readonly ILogger<MediaProcessor> _logger;
    private readonly IConfiguration _configuration;

    public MediaProcessor(ILogger<MediaProcessor> logger,IConfiguration _configuration)
    {
        _logger = logger;
        this._configuration = _configuration;
    }

    public Task Process(CancellationToken cancellationToken,IMediaContext mediaContext)
    {
        var videoUrl = mediaContext.MediaUrl;
        var audiopath = _configuration.GetSection("MediaIO").GetValue<string>("SampleAudio");
        var processedUri = _configuration.GetSection("MediaIO").GetValue<string>("ProcessedMediaLocation");
        var ffmpeguri = _configuration.GetSection("MediaIO").GetValue<string>("FFMPEG");
        var outpath = @$"{processedUri}{Path.DirectorySeparatorChar}{mediaContext.MediaName}.mp4";
        
        _logger.LogInformation($"audiolpoc {audiopath} processedloc {processedUri} ffmpeg {ffmpeguri} out {outpath}");
        
        var processinfo = new ProcessStartInfo
        {
            FileName = ffmpeguri,
            WorkingDirectory = processedUri,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true
        };
        
        if (mediaContext.StartTime == -1 || mediaContext.EndTime == -1 || mediaContext.VideoDuration < mediaContext.EndTime - mediaContext.StartTime)
        {
            processinfo.Arguments =
                $"-y -i {videoUrl} -ss 134 -i {audiopath} -map 0:v -map 1:a -c:v copy -shortest {outpath}";
        }
        else
        {
            processinfo.Arguments = $"-y -i {videoUrl}  -i {audiopath} -filter_complex \"[0:a]volume=0:enable=between(t\\,{mediaContext.StartTime}\\,{mediaContext.EndTime})[a0];[1:a]atrim=0:{mediaContext.EndTime - mediaContext.StartTime},adelay={mediaContext.StartTime}s:all=1[a1];[a0][a1]amix=normalize=0:duration=first[aout]\" -map 0:v  -map [aout] -c:v copy {outpath}";
        }
        _logger.LogInformation($"Command : {processinfo.Arguments} ");
        using var process = new Process{ StartInfo = processinfo };
        try
        {

            process.Start();
            
            return process.StandardOutput.ReadToEndAsync(); // waitforexitasync not working
            
        }
        catch (Exception e)
        {
            _logger.LogError($"ERROR PROCESS START.. ");
            throw;
        }
    }
}