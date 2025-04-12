using Data_Organizer_Server.Interfaces;
using Data_Organizer_Server.Models;
using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Repositories
{
    public class ChangePasswordRepository : IChangePasswordRepository
    {
        private readonly CollectionReference _changePasswordCollection;

        public ChangePasswordRepository(ICollectionFactory collectionFactory)
        {
            _changePasswordCollection = collectionFactory.GetChangePasswordCollection();
        }

        public async Task<DocumentReference> CreateChangePasswordAsync(ChangePassword changePassword)
        {
            if (changePassword == null)
                throw new ArgumentNullException("Argument \"changePassword\" is null while creating the password change.");

            return await _changePasswordCollection.AddAsync(changePassword);
        }
    }
}
