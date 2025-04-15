using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Entities
{
    [FirestoreData]
    public class NoteBody
    {
        [FirestoreProperty]
        public string Content { get; set; }
    }
}
