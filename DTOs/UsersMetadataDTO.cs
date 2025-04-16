using Data_Organizer_Server.Entities;
using System.Text.Json.Serialization;

namespace Data_Organizer.DTOs
{
    public class UsersMetadataDTO
    {
        [JsonPropertyName("creationDate")]
        public DateTime? CreationDate { get; set; }

        [JsonPropertyName("creationDeviceId")]
        public string? CreationDeviceId { get; set; }

        [JsonPropertyName("creationLocation")]
        public Location? CreationLocation { get; set; }

        [JsonPropertyName("deletionDate")]
        public DateTime? DeletionDate { get; set; }

        [JsonPropertyName("deletionDeviceId")]
        public string? DeletionDeviceId { get; set; }

        [JsonPropertyName("deletionLocation")]
        public Location? DeletionLocation { get; set; }
    }
}
