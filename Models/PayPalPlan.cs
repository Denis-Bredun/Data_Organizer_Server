using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Data_Organizer_Server.Models
{
    public class PayPalPlan
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("product_id")]
        public string ProductId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("billing_cycles")]
        public List<BillingCycle> BillingCycles { get; set; }

        [JsonPropertyName("payment_preferences")]
        public PaymentPreferences PaymentPreferences { get; set; }

        [JsonPropertyName("create_time")]
        public DateTime CreateTime { get; set; }

        [JsonPropertyName("update_time")]
        public DateTime UpdateTime { get; set; }
    }

    public class BillingCycle
    {
        [JsonPropertyName("frequency")]
        public Frequency Frequency { get; set; }

        [JsonPropertyName("tenure_type")]
        public string TenureType { get; set; }

        [JsonPropertyName("sequence")]
        public int Sequence { get; set; }

        [JsonPropertyName("total_cycles")]
        public int TotalCycles { get; set; }

        [JsonPropertyName("pricing_scheme")]
        public PricingScheme PricingScheme { get; set; }
    }

    public class Frequency
    {
        [JsonPropertyName("interval_unit")]
        public string IntervalUnit { get; set; }

        [JsonPropertyName("interval_count")]
        public int IntervalCount { get; set; }
    }

    public class PricingScheme
    {
        [JsonPropertyName("fixed_price")]
        public FixedPrice FixedPrice { get; set; }
    }

    public class FixedPrice
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("currency_code")]
        public string CurrencyCode { get; set; }
    }

    public class PaymentPreferences
    {
        [JsonPropertyName("auto_bill_outstanding")]
        public bool AutoBillOutstanding { get; set; }

        [JsonPropertyName("setup_fee")]
        public SetupFee SetupFee { get; set; }

        [JsonPropertyName("setup_fee_failure_action")]
        public string SetupFeeFailureAction { get; set; }

        [JsonPropertyName("payment_failure_threshold")]
        public int PaymentFailureThreshold { get; set; }
    }

    public class SetupFee
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("currency_code")]
        public string CurrencyCode { get; set; }
    }
} 