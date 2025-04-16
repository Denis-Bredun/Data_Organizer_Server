using Data_Organizer_Server.Interfaces;
using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Services
{
    public class CollectionFactory(FirestoreDb firestoreDb) : ICollectionFactory
    {
        private readonly FirestoreDb _firestoreDb = firestoreDb;

        public CollectionReference GetAccountLoginCollection() => _firestoreDb.Collection("AccountLogin");

        public CollectionReference GetAccountLogoutCollection() => _firestoreDb.Collection("AccountLogout");

        public CollectionReference GetChangePasswordCollection() => _firestoreDb.Collection("ChangePassword");

        public CollectionReference GetDevicesCollection() => _firestoreDb.Collection("Devices");

        public CollectionReference GetNoteHeadersCollection() => _firestoreDb.Collection("NoteHeaders");

        public CollectionReference GetNoteBodiesCollection() => _firestoreDb.Collection("NoteBodies");

        public CollectionReference GetUsersCollection() => _firestoreDb.Collection("Users");

        public CollectionReference GetUsersMetadataCollection() => _firestoreDb.Collection("UsersMetadata");
    }
}
