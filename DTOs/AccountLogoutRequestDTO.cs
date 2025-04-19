using Data_Organizer_Server.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Data_Organizer_Server.DTOs
{
    public class AccountLogoutRequestDTO
    {
        [Required]
        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [Required]
        [JsonPropertyName("accountLogout")]
        public AccountLogout AccountLogout { get; set; }

        [Required]
        [JsonPropertyName("deviceInfo")]
        public DeviceInfoModel DeviceInfo { get; set; }
    }
}
