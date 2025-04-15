using Data_Organizer_Server.Entities;
using Data_Organizer_Server.Interfaces;
using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Repositories
{
    public class UserRepository(ICollectionFactory collectionFactory) : IUserRepository
    {
        private readonly CollectionReference _usersCollection = collectionFactory.GetUsersCollection();

        public async Task<string> CreateUserAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException("Argument \"user\" is null while creating the user.");

            await _usersCollection.AddAsync(user);

            return user.Uid;
        }

        public async Task<User> GetUserByUidAsync(string uid)
        {
            if (string.IsNullOrWhiteSpace(uid))
                throw new ArgumentNullException("Argument \"uid\" is null or empty while getting user by uid.");

            var query = _usersCollection.WhereEqualTo("Uid", uid).WhereEqualTo("IsDeleted", false);
            var snapshot = await query.GetSnapshotAsync();
            var docs = snapshot.Documents;

            if (docs.Count == 0)
                throw new KeyNotFoundException($"Not deleted User Document with ID '{uid}' was not found.");

            var user = docs[0].ConvertTo<User>();

            return user;
        }

        public async Task RemoveUserAsync(string uid)
        {
            if (string.IsNullOrWhiteSpace(uid))
                throw new ArgumentNullException("Argument \"uid\" is null or empty while removing the user.");

            var user = await GetUserByUidAsync(uid);

            user.IsDeleted = true;

            await UpdateUserAsync(user);
        }

        public async Task UpdateUserAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException("Argument \"user\" is null while updating the user.");

            var query = _usersCollection.WhereEqualTo("Uid", user.Uid);
            var snapshot = await query.GetSnapshotAsync();
            var docs = snapshot.Documents;

            if (docs.Count == 0)
                throw new KeyNotFoundException($"User Document with ID '{user.Uid}' was not found.");

            var docRef = docs[0].Reference;
            await docRef.SetAsync(user);
        }
    }
}
