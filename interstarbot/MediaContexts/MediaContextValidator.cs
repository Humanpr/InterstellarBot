using System.Drawing;
using FluentValidation;

namespace interstarbot.MediaContexts;

public class MediaContextValidator : AbstractValidator<MediaContext>
{
    public MediaContextValidator()
    {
        RuleFor(mctx => mctx.MediaName).NotEmpty()
            .WithMessage((mctx) =>$"{nameof(mctx.MediaName)} can not be null!");

        RuleFor(mctx => mctx.MediaUrl).NotEmpty()
            .WithMessage((mctx) =>$"{nameof(mctx.MediaUrl)} can not be null!");
        
        RuleFor(mctx => mctx.VideoDuration).NotEqual(0)
            .WithMessage((mctx) =>$"{nameof(mctx.VideoDuration)} can not be equal to 0!");
        
        RuleFor(mctx => mctx.ReplyTweetID).NotEqual(0)
            .WithMessage((mctx) =>$"{nameof(mctx.ReplyTweetID)} can not be equal to 0!");
        
        
        RuleFor(mctx => mctx.ReplyTweetUserHandle).NotEmpty()
            .WithMessage((mctx) =>$"{nameof(mctx.ReplyTweetUserHandle)} can not be null!");
        
        RuleFor(mctx => mctx.ReplyTweetIDStr).NotEmpty()
            .WithMessage((mctx) =>$"{nameof(mctx.ReplyTweetIDStr)} can not be null!");
        
        // Video duration can be great or equal to (EndTime - StartTime)
        RuleFor(mctx => mctx.VideoDuration)
            .Must((mctx, videoduration) => videoduration >= mctx.EndTime - mctx.StartTime)
            .WithMessage("Wrong start and end time!");
    }
}