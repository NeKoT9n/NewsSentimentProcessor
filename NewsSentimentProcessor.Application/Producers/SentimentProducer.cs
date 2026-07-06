using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Models.Messages;

namespace NewsSentimentProcessor.Application.Producers;

public class SentimentProducer(IPublishEndpoint publishEndpoint, ILogger<SentimentProducer> logger)
{
    public async Task PublishSentimentResultAsync(SentimentProcessed result, CancellationToken ct = default)
    {
        try
        {
            await publishEndpoint.Publish(result, ct);
            
            logger.LogInformation("Message for NewsId {Id} successfully published", result.NewsId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not publish sentiment result for article {Id}", result.NewsId);
            throw;
        }
    }
}