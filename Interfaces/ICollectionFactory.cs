using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Interfaces
{
    public interface ICollectionFactory
    {
        CollectionReference GetUsersCollection();
        CollectionReference GetNoteBodiesCollection();
        CollectionReference GetNoteHeadersCollection();
        CollectionReference GetUsersMetadataCollection();
        CollectionReference GetDevicesCollection();
        CollectionReference GetChangePasswordCollection();
        CollectionReference GetAccountLoginCollection();
        CollectionReference GetAccountLogoutCollection();
    }
}
