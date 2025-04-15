using Data_Organizer_Server.Entities;
using Data_Organizer_Server.Interfaces;
using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Repositories
{
    public class UsersMetadataRepository(ICollectionFactory collectionFactory) : IUsersMetadataRepository
    {
        private readonly CollectionReference _metadataCollection = collectionFactory.GetUsersMetadataCollection();
        private readonly CollectionReference _usersCollection = collectionFactory.GetUsersCollection();

        public async Task<DocumentReference> CreateMetadataAsync(UsersMetadata metadata)
        {
            if (metadata == null)
                throw new ArgumentNullException("Argument \"metadata\" is null while creating metadata.");

            var docRef = await _metadataCollection.AddAsync(metadata);

            return docRef;
        }

        public async Task<DocumentReference> GetUsersMetadataReferenceByUidAsync(string uid)
        {
            if (string.IsNullOrWhiteSpace(uid))
                throw new ArgumentNullException("Argument \"uid\" is null or empty while getting user's metadata reference.");

            var query = _usersCollection.WhereEqualTo("Uid", uid);
            var snapshot = await query.GetSnapshotAsync();
            var docs = snapshot.Documents;

            if (docs.Count == 0)
                throw new KeyNotFoundException($"User Document with UID '{uid}' was not found.");

            var user = docs[0].ConvertTo<User>();

            user.UsersMetadata = await CreateMetadataIfNecessary(user.UsersMetadata);

            return user.UsersMetadata;
        }

        private async Task<DocumentReference> CreateMetadataIfNecessary(DocumentReference metadataDocRef)
        {
            if (metadataDocRef == null)
            {
                UsersMetadata usersMetadata = new UsersMetadata();
                metadataDocRef = await CreateMetadataAsync(usersMetadata);
            }
            return metadataDocRef;
        }

        public async Task UpdateMetadataAsync(string uid, UsersMetadata metadata)
        {
            if (string.IsNullOrWhiteSpace(uid))
                throw new ArgumentNullException("Argument \"uid\" is null or empty while updating metadata.");

            if (metadata == null)
                throw new ArgumentNullException("Argument \"metadata\" is null while updating metadata.");

            var metadataRef = await GetUsersMetadataReferenceByUidAsync(uid);
            await metadataRef.SetAsync(metadata);
        }
    }
}
