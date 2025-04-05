using System;
using System.Text.Json.Serialization;

namespace Data_Organizer_Server.Models
{
    public class PayPalTransaction
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("amount_with_breakdown")]
        public AmountWithBreakdown AmountWithBreakdown { get; set; }

        [JsonPropertyName("time")]
        public DateTime Time { get; set; }
    }

    public class AmountWithBreakdown
    {
        [JsonPropertyName("gross_amount")]
        public Amount GrossAmount { get; set; }

        [JsonPropertyName("fee_amount")]
        public Amount FeeAmount { get; set; }

        [JsonPropertyName("net_amount")]
        public Amount NetAmount { get; set; }
    }

    public class Amount
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("currency_code")]
        public string CurrencyCode { get; set; }
    }
} 