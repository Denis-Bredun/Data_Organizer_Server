using Data_Organizer_Server.DTOs;
using Data_Organizer_Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Data_Organizer_Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("transkriptor")]
    public class TranskriptorController(
    ITranskriptorService transcriptionService,
    ILogger<TranskriptorController> logger) : ControllerBase
    {
        private const long MaxFileSize = 100 * 1024 * 1024;
        private readonly ITranskriptorService _transcriptionService = transcriptionService;
        private readonly ILogger<TranskriptorController> _logger = logger;

        [HttpPost("transcribe-file")]
        [RequestSizeLimit(MaxFileSize)]
        public async Task<IActionResult> TranscribeFromFileAsync([FromForm] AudiofileDTO request)
        {
            if (request == null || request.AudioFile == null || request.AudioFile.Length == 0)
            {
                string error = "Please upload an audio file";
                _logger.LogError(error);
                return BadRequest(error);
            }

            if (request.AudioFile.Length > MaxFileSize)
            {
                string error = $"File size exceeds {MaxFileSize / 1024 / 1024}MB limit";
                _logger.LogError(error);
                return BadRequest(error);
            }

            string tempFilePath = Path.GetTempFileName();
            try
            {
                await using (var stream = System.IO.File.Create(tempFilePath))
                {
                    await request.AudioFile.CopyToAsync(stream);
                }

                var transcriptionResult = await _transcriptionService.TranscribeFileAsync(tempFilePath, request.LanguageCode);
                _logger.LogInformation("Audio transcription successful. Result: {Transcription}", transcriptionResult);

                return Ok(new { Transcription = transcriptionResult });
            }
            catch (HttpRequestException ex)
            {
                string error = "Error connecting to Azure Speech API. Please try again later.";
                _logger.LogError(ex, error);
                return StatusCode(502, new { Error = error });
            }
            catch (InvalidOperationException ex)
            {
                string error = ex.Message;
                _logger.LogError(ex, "Configuration error in AzureAudioController.");
                return StatusCode(500, new { Error = error });
            }
            catch (ArgumentException ex)
            {
                string error = ex.Message;
                _logger.LogError(ex, "Invalid input data for transcription.");
                return BadRequest(new { Error = error });
            }
            catch (Exception ex)
            {
                string error = "An internal server error occurred. Please try again later.";
                _logger.LogError(ex, "Unexpected error in AzureAudioController.");
                return StatusCode(500, new { Error = error });
            }
            finally
            {
                if (System.IO.File.Exists(tempFilePath))
                    System.IO.File.Delete(tempFilePath);
            }
        }
    }
}
