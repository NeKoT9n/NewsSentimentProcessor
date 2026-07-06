namespace Shared.Models.Messages;

public class NewsAdded
{
    public long NewsId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    
}

public class SentimentProcessed
{
    public long NewsId { get; set; }
    public SentimentType Type { get; set; } = SentimentType.None;
    public float Score { get; } = 0;
}