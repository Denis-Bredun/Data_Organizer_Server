using Data_Organizer_Server.Interfaces;
using Data_Organizer_Server.Models;
using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Repositories
{
    public class AccountLogoutRepository : IAccountLogoutRepository
    {
        private readonly CollectionReference _accountLogoutCollection;

        public AccountLogoutRepository(ICollectionFactory collectionFactory)
        {
            _accountLogoutCollection = collectionFactory.GetAccountLogoutCollection();
        }

        public async Task<DocumentReference> CreateAccountLogoutAsync(AccountLogout accountLogout)
        {
            if (accountLogout == null)
                throw new ArgumentNullException("Argument \"accountLogout\" is null while creating the account logout.");

            return await _accountLogoutCollection.AddAsync(accountLogout);
        }
    }
}
