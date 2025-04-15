using Data_Organizer_Server.Entities;
using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Interfaces
{
    public interface IAccountLogoutRepository
    {
        Task<DocumentReference> CreateAccountLogoutAsync(AccountLogout accountLogout);
    }
}
