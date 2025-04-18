namespace Data_Organizer_Server.Interfaces
{
    public interface ITranskriptorService
    {
        Task<string> TranscribeFileAsync(string audioFilePath, string languageCode);
    }
}
