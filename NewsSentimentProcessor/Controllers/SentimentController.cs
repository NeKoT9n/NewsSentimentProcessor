using Microsoft.AspNetCore.Mvc;
using NewsSentimentProcessor.Application;

namespace NewsSentimentProcessor.Controllers;

[Route("api/[controller]")]
public class SentimentController(SentimentService sentimentService) : ControllerBase
{
    [HttpPost("process")]
    public async Task<ActionResult<OllamaResponse>> ProcessTest([FromBody] string text)
    {
        var result = await sentimentService.ProcessText(text);
        
        if(result.IsFailure)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }
}