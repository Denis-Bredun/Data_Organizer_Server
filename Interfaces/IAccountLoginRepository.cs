using Data_Organizer_Server.Entities;
using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Interfaces
{
    public interface IAccountLoginRepository
    {
        Task<DocumentReference> CreateAccountLoginAsync(AccountLogin accountLogin);
    }
}
