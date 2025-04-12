using Data_Organizer_Server.Models;
using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Interfaces
{
    public interface IUsersMetadataRepository
    {
        Task<DocumentReference> CreateMetadataAsync(UsersMetadata metadata);
        Task<DocumentReference> GetUsersMetadataReferenceByUidAsync(string uid);
        Task UpdateMetadataAsync(string uid, UsersMetadata metadata);
    }
}
