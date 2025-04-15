using Data_Organizer_Server.Entities;
using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Interfaces
{
    public interface IChangePasswordRepository
    {
        Task<DocumentReference> CreateChangePasswordAsync(ChangePassword changePassword);
    }
}
