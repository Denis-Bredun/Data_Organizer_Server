using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Entities
{
    [FirestoreData]
    public class Note
    {
        [FirestoreProperty]
        public NoteHeader Header { get; set; }

        [FirestoreProperty]
        public NoteBody Body { get; set; }
    }
}
