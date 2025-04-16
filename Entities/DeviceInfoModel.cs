using Google.Cloud.Firestore;
using System.Text.Json.Serialization;

namespace Data_Organizer_Server.Entities
{
    [FirestoreData]
    public class DeviceInfoModel
    {
        [JsonPropertyName("name")]
        [FirestoreProperty]
        public string Name { get; set; }

        [JsonPropertyName("model")]
        [FirestoreProperty]
        public string Model { get; set; }

        [JsonPropertyName("manufacturer")]
        [FirestoreProperty]
        public string Manufacturer { get; set; }

        [JsonPropertyName("platform")]
        [FirestoreProperty]
        public string Platform { get; set; }

        [JsonPropertyName("idiom")]
        [FirestoreProperty]
        public string Idiom { get; set; }

        [JsonPropertyName("deviceType")]
        [FirestoreProperty]
        public string DeviceType { get; set; }

        [JsonPropertyName("version")]
        [FirestoreProperty]
        public string Version { get; set; }

        [JsonPropertyName("deviceInfoCombined")]
        [FirestoreProperty]
        public string DeviceInfoCombined => $"{Name}_{Model}_{Manufacturer}_{Platform}_{Idiom}_{DeviceType}";
    }
}
