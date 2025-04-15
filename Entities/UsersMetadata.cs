using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Entities
{
    [FirestoreData]
    public class UsersMetadata
    {
        [FirestoreProperty]
        public DateTime? CreationDate { get; set; }

        [FirestoreProperty]
        public DocumentReference? CreationDevice { get; set; }

        [FirestoreProperty]
        public Location? CreationLocation { get; set; }

        [FirestoreProperty]
        public DateTime? DeletionDate { get; set; }

        [FirestoreProperty]
        public DocumentReference? DeletionDevice { get; set; }

        [FirestoreProperty]
        public Location? DeletionLocation { get; set; }
    }
}
