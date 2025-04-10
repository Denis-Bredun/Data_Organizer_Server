using Data_Organizer_Server.Interfaces;
using Data_Organizer_Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Data_Organizer_Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/openai")]
    public class OpenAIController : ControllerBase
    {
        private readonly IOpenAIService _openAIService;
        private readonly ILogger<OpenAIController> _logger;

        public OpenAIController(IOpenAIService openAIService, ILogger<OpenAIController> logger)
        {
            _openAIService = openAIService;
            _logger = logger;
        }

        [HttpPost("summary")]
        public async Task<IActionResult> GetSummary([FromBody] SummaryRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Content))
            {
                if (request != null)
                    request.Error = "Empty request or missing content!";

                _logger.LogWarning($"Received invalid request: {request?.Error}");
                return request != null ?
                    BadRequest(request) :
                    BadRequest(new SummaryRequest() { Error = "Empty request or missing content!" });
            }

            try
            {
                request.Result = await _openAIService.GetSummary(request.Content);
                _logger.LogInformation("Summary was successfully created. Result: {Summary}", request.Result);
                return Ok(request);
            }
            catch (HttpRequestException ex)
            {
                request.Error = "Error connecting to OpenAI API. Please try again later.";
                _logger.LogError(ex, request.Error);
                return StatusCode(502, request);
            }
            catch (NullReferenceException ex)
            {
                request.Error = "OpenAI API did not return the expected result.";
                _logger.LogError(ex, request.Error);
                return StatusCode(500, request);
            }
            catch (ArgumentException ex)
            {
                request.Error = ex.Message;
                _logger.LogWarning(ex, "Invalid input data for OpenAIService.");
                return BadRequest(request);
            }
            catch (Exception ex)
            {
                request.Error = "An internal server error occurred. Please try again later.";
                _logger.LogError(ex, "Unexpected error in OpenAIController.");
                return StatusCode(500, request);
            }
        }
    }
}
