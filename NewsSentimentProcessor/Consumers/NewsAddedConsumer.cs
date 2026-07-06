using MassTransit;
using NewsSentimentProcessor.Application;
using NewsSentimentProcessor.Application.Dto;
using Shared.Models.Messages;

namespace NewsSentimentProcessor.Consumers;

public class NewsAddedConsumer(ILogger<NewsAddedConsumer> logger, SentimentService sentimentService): IConsumer<ArticleAdded>
{
    public async Task Consume(ConsumeContext<ArticleAdded> context)
    {
        var message = context.Message;
        
        logger.LogInformation($"News received: {message.Title}");

        var articleToProcess = new ArticleToProcess(
            message.NewsId,
            message.Title,
            message.Content);

        await sentimentService.ProcessArticle(articleToProcess);
        
    }
}