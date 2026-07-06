using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using NewsSentimentProcessor.Application.Configurations;
using Shared.Common.Validation;

namespace NewsSentimentProcessor.Application;

public class OllamaProvider
{

    private readonly OllamaOptions _options;
    private readonly HttpClient _httpClient;
    private static readonly SemaphoreSlim _semaphore = new(4, 4);

    public OllamaProvider(HttpClient httpClient, IOptions<OllamaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
    }
 
    public async Task<Result<OllamaResponse, Error>> GetSentiment(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Errors.General.Failure("Input text cannot be empty.");

        var systemPrompt = "You are an expert sentiment analysis AI. Analyze the news text and determine its sentiment: Positive, Negative, Neutral, or Mixed. " +
                   "Determine the intensity 'score' on a fixed scale from 0 to 10, where: " +
                   "0 means extremely weak or barely noticeable sentiment, " +
                   "5 means moderate/average intensity, " +
                   "10 means extremely intense, critical, or highly impactful emotional sentiment (e.g., a score of 10 for Negative means a catastrophic event, for Positive means a massive breakthrough). " +
                   "Respond ONLY with a JSON object containing 'sentiment' (string) and 'score' (integer) fields. Do not wrap the response in markdown code blocks.";

        var requestBody = new
        {
            model = _options.ModelName,
            prompt = $"System: {systemPrompt}\n\nUser text to analyze: \"{text}\"\n\nResponse:",
            stream = false,
            format = "json" 
        };

        await _semaphore.WaitAsync();

        try
        {
            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/generate", content);
            
            if (!response.IsSuccessStatusCode)
                return Errors.General.Failure($"Ollama returned error: {response.StatusCode}");


            var jsonResponse = await response.Content.ReadAsStringAsync();
            
            using var doc = JsonDocument.Parse(jsonResponse);
            
            if (!doc.RootElement.TryGetProperty("response", out var responseTextElement))
                return Errors.General.Failure("Invalid response structure from Ollama.");
            
            string? innerJson = responseTextElement.GetString();
            
            if (string.IsNullOrEmpty(innerJson))
                return Errors.General.Failure("Ollama returned an empty response text.");

            try
            {
                using var innerDoc = JsonDocument.Parse(innerJson);
                var root = innerDoc.RootElement;

                string? sentimentValue = FindPropertyInJson(root, "sentiment")?.GetString();
                int? scoreValue = FindPropertyInJson(root, "score")?.GetInt32();

                if (scoreValue == null)
                {
                    var scoreStr = FindPropertyInJson(root, "score")?.GetString();
                    if (int.TryParse(scoreStr, out var parsedScore))
                    {
                        scoreValue = parsedScore;
                    }
                }

                if (string.IsNullOrEmpty(sentimentValue))
                    return Errors.General.Failure($"Failed to extract 'sentiment' from JSON. Raw output: {innerJson}");

                return new OllamaResponse
                {
                    Sentiment = sentimentValue,
                    Score = scoreValue ?? 5
                };
            }
            catch (JsonException)
            {
                return Errors.General.Failure($"Ollama returned invalid JSON syntax. Raw output: {innerJson}");
            }
        }
        catch (Exception ex)
        {
            return Errors.General.Failure($"Exception occurred: {ex.Message}");
        }
        finally
        {
            _semaphore.Release();
        }

    }

    private static JsonElement? FindPropertyInJson(JsonElement element, string propertyName)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    return property.Value;
                }

                if (property.Value.ValueKind == JsonValueKind.Object)
                {
                    var found = FindPropertyInJson(property.Value, propertyName);
                    if (found != null) return found;
                }
            }
        }
        return null;
    }
}

public class OllamaResponse
{
    [JsonPropertyName("sentiment")]
    public string Sentiment { get; init; } = string.Empty;

    [JsonPropertyName("score")]
    public float Score { get; init; }
}