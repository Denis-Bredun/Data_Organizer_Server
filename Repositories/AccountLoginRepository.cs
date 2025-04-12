using Data_Organizer_Server.Interfaces;
using Data_Organizer_Server.Models;
using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Repositories
{
    public class AccountLoginRepository : IAccountLoginRepository
    {
        private readonly CollectionReference _accountLoginCollection;

        public AccountLoginRepository(ICollectionFactory collectionFactory)
        {
            _accountLoginCollection = collectionFactory.GetAccountLoginCollection();
        }

        public async Task<DocumentReference> CreateAccountLoginAsync(AccountLogin accountLogin)
        {
            if (accountLogin == null)
                throw new ArgumentNullException("Argument \"accountLogin\" is null while creating the account login.");

            return await _accountLoginCollection.AddAsync(accountLogin);
        }
    }
}
