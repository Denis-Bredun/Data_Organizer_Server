using Data_Organizer.DTOs;
using Data_Organizer_Server.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Data_Organizer_Server.DTOs
{
    public class ChangePasswordRequestDTO
    {
        [Required]
        [JsonPropertyName("uid")]
        public string Uid { get; set; }

        [Required]
        [JsonPropertyName("changePasswordDTO")]
        public ChangePasswordDTO ChangePasswordDTO { get; set; }

        [Required]
        [JsonPropertyName("deviceInfo")]
        public DeviceInfoModel DeviceInfo { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }
}
