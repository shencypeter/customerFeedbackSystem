namespace CustomerFeedbackSystem.Models;

public class FeedbackDetailsViewModel
{
    public Feedback Feedback { get; set; } = null!;
    public List<FeedbackResponse> Replies { get; set; } = new();
    public List<FeedbackAttachment> QuestionAttachments { get; set; } = new();
    public ILookup<int, FeedbackAttachment> ReplyAttachments { get; set; }
        = Enumerable.Empty<FeedbackAttachment>().ToLookup(_ => 0);
}
