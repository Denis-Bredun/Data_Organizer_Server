using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Entities
{
    [FirestoreData]
    public class Location
    {
        [FirestoreProperty]
        public double Latitude { get; set; }

        [FirestoreProperty]
        public double Longitude { get; set; }
    }
}
