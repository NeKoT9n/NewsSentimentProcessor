using MassTransit;
using Microsoft.Extensions.Options;
using NewsSentimentProcessor.Application;
using NewsSentimentProcessor.Application.Configurations;
using NewsSentimentProcessor.Application.Producers;
using NewsSentimentProcessor.Consumers;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

builder.Services.Configure<OllamaOptions>(
    builder.Configuration.GetSection(OllamaOptions.SectionName));

builder.Services.AddScoped<SentimentService>();
builder.Services.AddScoped<SentimentProducer>();
builder.Services.AddHttpClient<OllamaProvider>();

services.AddMassTransit(x =>
{
    x.AddConsumer<NewsAddedConsumer>();
    
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitConfig = builder.Configuration.GetSection("MessageBroker");
        
        cfg.Host(rabbitConfig["Host"], "/", h => {
            h.Username(rabbitConfig["UserName"]!);
            h.Password(rabbitConfig["Password"]!);
        });
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Engine = "Qwen 2.5" }));

app.Run();