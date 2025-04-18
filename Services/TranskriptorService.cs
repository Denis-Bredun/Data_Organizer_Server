using Data_Organizer_Server.Interfaces;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace Data_Organizer_Server.Services
{
    public class TranskriptorService : ITranskriptorService
    {
        private readonly string _subscriptionKey;
        private readonly string _region;

        public TranskriptorService()
        {
            _subscriptionKey = Environment.GetEnvironmentVariable("AZURE_SPEECH_KEY");
            _region = Environment.GetEnvironmentVariable("AZURE_SPEECH_REGION");

            if (string.IsNullOrWhiteSpace(_subscriptionKey) || string.IsNullOrWhiteSpace(_region))
                throw new InvalidOperationException("Azure Speech subscription key or region is not set in environment variables.");
        }

        public async Task<string> TranscribeFileAsync(string audioFilePath, string languageCode)
        {
            string convertedFilePath = null;
            try
            {
                string inputFile = ConvertToWavIfNeeded(audioFilePath, out convertedFilePath);

                var config = SpeechConfig.FromSubscription(_subscriptionKey, _region);
                config.SpeechRecognitionLanguage = languageCode;
                using var audioInput = AudioConfig.FromWavFileInput(inputFile);
                using var recognizer = new SpeechRecognizer(config, audioInput);
                var result = await recognizer.RecognizeOnceAsync();

                return result.Reason switch
                {
                    ResultReason.RecognizedSpeech => result.Text,
                    ResultReason.NoMatch => throw new Exception("Speech could not be recognized."),
                    _ => throw new Exception($"Recognition error: {result.Reason}")
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred during audio transcription in AzureAudioTranscriptionService.", ex);
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
