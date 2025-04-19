using Data_Organizer_Server.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Data_Organizer_Server.DTOs
{
    public class AccountLogoutDTO
    {
        [JsonPropertyName("usersMetadataId")]
        public string? UsersMetadataId { get; set; }

        [JsonPropertyName("deviceId")]
        public string? DeviceId { get; set; }

        [Required]
        [JsonPropertyName("location")]
        public Location Location { get; set; }

        [Required]
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
    }
}
