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
                request.Error = "Порожній запит або відсутній контент!";
                _logger.LogWarning($"Отримано некоректний запит: {request.Error}");
                return BadRequest(request);
            }

            try
            {
                request.Result = await _openAIService.GetSummary(request.Content);
                return Ok(request);
            }
            catch (HttpRequestException ex)
            {
                request.Error = "Помилка підключення до OpenAI API. Спробуйте пізніше.";
                _logger.LogError(ex, request.Error);
                return StatusCode(502, request);
            }
            catch (NullReferenceException ex)
            {
                request.Error = "OpenAI API не повернув очікуваний результат.";
                _logger.LogError(ex, request.Error);
                return StatusCode(500, request);
            }
            catch (ArgumentException ex)
            {
                request.Error = ex.Message;
                _logger.LogWarning(ex, "Некоректні вхідні дані для OpenAIService.");
                return BadRequest(request);
            }
            catch (Exception ex)
            {
                request.Error = "Сталася внутрішня помилка сервера. Будь ласка, спробуйте пізніше.";
                _logger.LogError(ex, "Неочікувана помилка в OpenAIController.");
                return StatusCode(500, request);
            }
        }
    }
}
