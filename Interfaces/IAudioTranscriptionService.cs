namespace Data_Organizer_Server.Interfaces
{
    public interface IAudioTranscriptionService
    {
        Task<string> TranscribeAsync(string audioFilePath, string languageCode);
    }
}
