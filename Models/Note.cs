using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Models
{
    [FirestoreData]
    public class Note
    {
        [FirestoreProperty]
        public string UserId { get; set; }

        [FirestoreProperty]
        public string Title { get; set; }

        [FirestoreProperty]
        public string PreviewText { get; set; }

        [FirestoreProperty]
        public string Content { get; set; }

        [FirestoreProperty]
        public DateTime CreationTime { get; set; }

        [FirestoreProperty]
        public bool IsDeleted { get; set; }
    }
}
