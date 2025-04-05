using System;
using System.Text.Json.Serialization;

namespace Data_Organizer_Server.Models
{
    public class PayPalSubscription
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("start_time")]
        public DateTime StartTime { get; set; }

        [JsonPropertyName("quantity")]
        public string Quantity { get; set; }

        [JsonPropertyName("plan_id")]
        public string PlanId { get; set; }

        [JsonPropertyName("subscriber")]
        public PayPalSubscriber Subscriber { get; set; }

        [JsonPropertyName("create_time")]
        public DateTime CreateTime { get; set; }

        [JsonPropertyName("update_time")]
        public DateTime UpdateTime { get; set; }
    }

    public class PayPalSubscriber
    {
        [JsonPropertyName("email_address")]
        public string EmailAddress { get; set; }

        [JsonPropertyName("name")]
        public PayPalName Name { get; set; }
    }

    public class PayPalName
    {
        [JsonPropertyName("given_name")]
        public string GivenName { get; set; }

        [JsonPropertyName("surname")]
        public string Surname { get; set; }
    }
} 