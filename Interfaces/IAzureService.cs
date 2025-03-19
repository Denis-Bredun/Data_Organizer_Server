namespace Data_Organizer_Server.Interfaces
{
    public interface IAzureService
    {
        Task<string> TranscribeFileAsync(string audioFilePath, string languageCode);
    }
}
