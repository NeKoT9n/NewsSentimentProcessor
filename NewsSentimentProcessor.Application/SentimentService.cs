using Microsoft.Extensions.Logging;
using NewsSentimentProcessor.Application.Dto;
using NewsSentimentProcessor.Application.Producers;
using Shared.Common.Validation;
using Shared.Models;
using Shared.Models.Messages;
using static MassTransit.ValidationResultExtensions;

namespace NewsSentimentProcessor.Application;

public class SentimentService(ILogger<SentimentService> logger, SentimentProducer producer, OllamaProvider ollamaProvider)
{
    public async Task ProcessArticle(ArticleToProcess article)
    {
        try
        {
            logger.LogInformation("Analyzing sentiment for article: {Id}", article.Id);

            var safeTitle = article.Title.Truncate(150);
            var safeDescription = article.Description.Truncate(1000);

            var result = await ollamaProvider.GetSentiment($"Название: {safeTitle}, Новость: {safeDescription}");

            if (result.IsFailure)
                throw new Exception(result.Error.Message);

            var sentimentType = SentimentType.None;

            if (!Enum.TryParse(result.Value.Sentiment, ignoreCase: true, out sentimentType))
                sentimentType = SentimentType.None;

            var message = new SentimentProcessed()
            {
                NewsId = article.Id,
                SentimentType = sentimentType,
                Score = result.Value.Score
            };

            await producer.PublishSentimentResultAsync(message);
        }
        catch (Exception ex)
        {
            logger.LogError("Error while analyzing sentiment {error}", ex.Message);
        }
    }
    
    public async Task<Result<OllamaResponse, Error>> ProcessText(string text)
    {
        logger.LogInformation("Analyzing sentiment for text: {text}", text);
        
        return await ollamaProvider.GetSentiment(text);
    }

}

public static class StringExtensions
{
    public static string Truncate(this string? value, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        return value.Length <= maxLength
            ? value
            : string.Concat(value.AsSpan(0, maxLength - suffix.Length), suffix);
    }
}