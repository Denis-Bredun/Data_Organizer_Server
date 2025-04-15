using Data_Organizer_Server.Entities;
using Data_Organizer_Server.Interfaces;
using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Repositories
{
    public class AccountLoginRepository(ICollectionFactory collectionFactory) : IAccountLoginRepository
    {
        private readonly CollectionReference _accountLoginCollection = collectionFactory.GetAccountLoginCollection();

        public async Task<DocumentReference> CreateAccountLoginAsync(AccountLogin accountLogin)
        {
            if (accountLogin == null)
                throw new ArgumentNullException("Argument \"accountLogin\" is null while creating the account login.");

            return await _accountLoginCollection.AddAsync(accountLogin);
        }
    }
}
