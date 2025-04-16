using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Data_Organizer.DTOs
{
    public class UserIsMetadataStoredPropertyUpdateDTO
    {
        [Required]
        [JsonPropertyName("uid")]
        public string Uid { get; set; }

        [Required]
        [JsonPropertyName("isMetadataStored")]
        public bool IsMetadataStored { get; set; }

        [JsonPropertyName("error")]
        public string Error { get; set; }
    }
}
