using Data_Organizer_Server.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Data_Organizer_Server.DTOs
{
    public class ChangePasswordRequestDTO
    {
        [Required]
        [JsonPropertyName("uid")]
        public string Uid { get; set; } = default!;

        [Required]
        [JsonPropertyName("changePassword")]
        public ChangePassword ChangePassword { get; set; } = default!;

        [Required]
        [JsonPropertyName("deviceInfo")]
        public DeviceInfoModel DeviceInfo { get; set; } = default!;
    }
}
