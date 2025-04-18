using Data_Organizer_Server.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Data_Organizer_Server.Services
{
    public class TranskriptorService : ITranskriptorService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;

        public const string UPLOAD_URL_ENDPOINT = "https://api.tor.app/developer/transcription/local_file/get_upload_url";
        public const string INITIATE_TRANSCRIPTION_ENDPOINT = "https://api.tor.app/developer/transcription/local_file/initiate_transcription";

        public TranskriptorService(HttpClient httpClient, IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClient;
            _httpClientFactory = httpClientFactory;
            _apiKey = Environment.GetEnvironmentVariable("TRANSKRIPTOR_API_KEY");

            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("Transkriptor API key is not set in environment variables.");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<string> TranscribeFileAsync(string audioFilePath, string languageCode)
        {
            if (string.IsNullOrWhiteSpace(audioFilePath) || !File.Exists(audioFilePath))
                throw new FileNotFoundException("Audio file not found.", audioFilePath);

            string convertedFilePath = null;

            try
            {
                string filePathToUse = ConvertToWavIfNeeded(audioFilePath, out convertedFilePath);
                string fileName = Path.GetFileName(filePathToUse);

                var uploadRequest = new { file_name = fileName };
                var uploadContent = new StringContent(JsonSerializer.Serialize(uploadRequest), Encoding.UTF8, "application/json");

                var uploadResponse = await _httpClient.PostAsync(UPLOAD_URL_ENDPOINT, uploadContent);
                uploadResponse.EnsureSuccessStatusCode();

                var uploadJson = await uploadResponse.Content.ReadAsStringAsync();
                var uploadDoc = JsonDocument.Parse(uploadJson);
                string uploadUrl = uploadDoc.RootElement.GetProperty("upload_url").GetString();
                string publicUrl = uploadDoc.RootElement.GetProperty("public_url").GetString();

                var uploadClient = _httpClientFactory.CreateClient();
                await using (var fileStream = File.OpenRead(filePathToUse))
                using (var fileContent = new StreamContent(fileStream))
                {
                    var fileUploadResponse = await uploadClient.PutAsync(uploadUrl, fileContent);
                    fileUploadResponse.EnsureSuccessStatusCode();
                }

                var transcriptionRequest = new
                {
                    url = publicUrl,
                    language = languageCode,
                    service = "Standard"
                };

                var transcriptionContent = new StringContent(JsonSerializer.Serialize(transcriptionRequest), Encoding.UTF8, "application/json");
                var transcriptionResponse = await _httpClient.PostAsync(INITIATE_TRANSCRIPTION_ENDPOINT, transcriptionContent);
                transcriptionResponse.EnsureSuccessStatusCode();

                var transcriptionJson = await transcriptionResponse.Content.ReadAsStringAsync();
                var transcriptionDoc = JsonDocument.Parse(transcriptionJson);

                return transcriptionDoc.RootElement.GetProperty("message").GetString();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("HTTP request to Transkriptor failed!", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Error parsing response from Transkriptor!", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unknown error in TranskriptorService!", ex);
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(convertedFilePath) && File.Exists(convertedFilePath))
                    File.Delete(convertedFilePath);
            }
        }

        private string ConvertToWavIfNeeded(string filePath, out string convertedFilePath)
        {
            convertedFilePath = null;
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            if (extension == ".wav")
                return filePath;

            convertedFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".wav");

            var ffmpegPath = "ffmpeg";

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-i \"{filePath}\" -acodec pcm_s16le -ar 16000 -ac 1 \"{convertedFilePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new System.Diagnostics.Process { StartInfo = startInfo };
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                var error = process.StandardError.ReadToEnd();
                throw new Exception($"FFmpeg conversion failed: {error}");
            }

            return convertedFilePath;
        }
    }
}
