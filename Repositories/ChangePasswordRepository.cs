using Data_Organizer_Server.Entities;
using Data_Organizer_Server.Interfaces;
using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Repositories
{
    public class ChangePasswordRepository(ICollectionFactory collectionFactory) : IChangePasswordRepository
    {
        private readonly CollectionReference _changePasswordCollection = collectionFactory.GetChangePasswordCollection();

        public async Task<DocumentReference> CreateChangePasswordAsync(ChangePassword changePassword)
        {
            if (changePassword == null)
                throw new ArgumentNullException("Argument \"changePassword\" is null while creating the password change.");

            return await _changePasswordCollection.AddAsync(changePassword);
        }
    }
}
