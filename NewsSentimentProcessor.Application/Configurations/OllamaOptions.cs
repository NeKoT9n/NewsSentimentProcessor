namespace NewsSentimentProcessor.Application.Configurations
{
    public class OllamaOptions
    {
        public const string SectionName = "Ollama";

        public string BaseUrl { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
    }
}
