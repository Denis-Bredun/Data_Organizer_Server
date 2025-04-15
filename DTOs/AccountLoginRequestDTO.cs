﻿using Data_Organizer_Server.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Data_Organizer_Server.DTOs
{
    public class AccountLoginRequestDTO
    {
        [Required]
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = default!;

        [Required]
        [JsonPropertyName("accountLogin")]
        public AccountLogin AccountLogin { get; set; } = default!;

        [Required]
        [JsonPropertyName("deviceInfo")]
        public DeviceInfoModel DeviceInfo { get; set; } = default!;
    }
}
