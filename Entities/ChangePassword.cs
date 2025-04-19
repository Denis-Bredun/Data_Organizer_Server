using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Entities
{
    [FirestoreData]
    public class ChangePassword
    {
        [FirestoreProperty]
        public DocumentReference? UsersMetadata { get; set; }

        [FirestoreProperty]
        public string OldPassword { get; set; }

        [FirestoreProperty]
        public DocumentReference? Device { get; set; }

        [FirestoreProperty]
        public Location Location { get; set; }

        [FirestoreProperty]
        public DateTime Date { get; set; }
    }
}
