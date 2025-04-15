using Data_Organizer_Server.Entities;
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
