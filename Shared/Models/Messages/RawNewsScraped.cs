namespace Shared.Models.Messages;

public class RawNewsScraped
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public DateTime ScrapedAt { get; init; }
}