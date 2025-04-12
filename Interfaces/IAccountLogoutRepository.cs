using Data_Organizer_Server.Models;
using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Interfaces
{
    public interface IAccountLogoutRepository
    {
        Task<DocumentReference> CreateAccountLogoutAsync(AccountLogout accountLogout);
    }
}
