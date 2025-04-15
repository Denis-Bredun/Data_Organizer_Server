using Data_Organizer_Server.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Data_Organizer_Server.DTOs
{
    public class UserRequestDTO
    {
        [Required]
        [JsonPropertyName("user")]
        public User User { get; set; } = default!;

        [JsonPropertyName("usersMetadata")]
        public UsersMetadata? UsersMetadata { get; set; }

        [JsonPropertyName("creationDevice")]
        public DeviceInfoModel? CreationDevice { get; set; }

        [JsonPropertyName("deletionDevice")]
        public DeviceInfoModel? DeletionDevice { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }
}
