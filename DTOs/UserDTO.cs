﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Data_Organizer_Server.DTOs
{
    public class UserDTO
    {
        [Required]
        [JsonPropertyName("uid")]
        public string Uid { get; set; }

        [JsonPropertyName("usersMetadataId")]
        public string? UsersMetadataId { get; set; }

        [Required]
        [JsonPropertyName("isDeleted")]
        public bool IsDeleted { get; set; }

        [Required]
        [JsonPropertyName("isMetadataStored")]
        public bool IsMetadataStored { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }
}
