using Data_Organizer_Server.Entities;
using Data_Organizer_Server.Interfaces;
using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Repositories
{
    public class AccountLogoutRepository(ICollectionFactory collectionFactory) : IAccountLogoutRepository
    {
        private readonly CollectionReference _accountLogoutCollection = collectionFactory.GetAccountLogoutCollection();

        public async Task<DocumentReference> CreateAccountLogoutAsync(AccountLogout accountLogout)
        {
            if (accountLogout == null)
                throw new ArgumentNullException("Argument \"accountLogout\" is null while creating the account logout.");

            return await _accountLogoutCollection.AddAsync(accountLogout);
        }
    }
}
