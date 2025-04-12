using Data_Organizer_Server.Models;
using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Interfaces
{
    public interface IChangePasswordRepository
    {
        Task<DocumentReference> CreateChangePasswordAsync(ChangePassword changePassword);
    }
}
