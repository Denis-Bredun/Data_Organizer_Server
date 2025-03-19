namespace Data_Organizer_Server.Models
{
    public class TranscriptionFromFileRequest
    {
        public IFormFile AudioFile { get; set; }
        public string LanguageCode { get; set; } = "uk-UA";
    }
}
