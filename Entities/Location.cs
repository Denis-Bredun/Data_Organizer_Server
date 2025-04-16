using Google.Cloud.Firestore;
using System.Text.Json.Serialization;

namespace Data_Organizer_Server.Entities
{
    [FirestoreData]
    public class Location
    {
        [JsonPropertyName("latitude")]
        [FirestoreProperty]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        [FirestoreProperty]
        public double Longitude { get; set; }
    }
}
