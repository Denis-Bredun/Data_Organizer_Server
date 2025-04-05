using System.Text.Json.Serialization;

namespace Data_Organizer_Server.Models
{
    public class PayPalClientTokenResponse
    {
        [JsonPropertyName("client_token")]
        public string ClientToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
    }

    public class PayPalClientTokenRequest
    {
        [JsonPropertyName("plan_id")]
        public string PlanId { get; set; }
    }
} 