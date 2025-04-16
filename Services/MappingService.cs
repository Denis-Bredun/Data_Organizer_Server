using Data_Organizer.DTOs;
using Data_Organizer_Server.Entities;
using Data_Organizer_Server.Interfaces;
using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Services
{
    public class MappingService(ICollectionFactory collectionFactory) : IMappingService
    {
        private readonly CollectionReference _usersMetadataCollection = collectionFactory.GetUsersMetadataCollection();
        private readonly CollectionReference _devicesCollection = collectionFactory.GetDevicesCollection();

        public UserDTO MapUser(User user) => new UserDTO
        {
            Uid = user.Uid,
            UsersMetadataId = user.UsersMetadata?.Id,
            IsDeleted = user.IsDeleted,
            IsMetadataStored = user.IsMetadataStored
        };

        public UsersMetadataDTO MapMetadata(UsersMetadata metadata) => new UsersMetadataDTO
        {
            CreationDate = metadata.CreationDate,
            CreationDeviceId = metadata.CreationDevice?.Id,
            CreationLocation = metadata.CreationLocation,
            DeletionDate = metadata.DeletionDate,
            DeletionDeviceId = metadata.DeletionDevice?.Id,
            DeletionLocation = metadata.DeletionLocation
        };

        public async Task<User> MapToUserAsync(UserDTO dto)
        {
            DocumentReference? metadataRef = null;

            if (!string.IsNullOrEmpty(dto.UsersMetadataId))
            {
                var docRef = _usersMetadataCollection.Document(dto.UsersMetadataId);
                var snapshot = await docRef.GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    metadataRef = docRef;
                }
            }

            return new User
            {
                Uid = dto.Uid,
                UsersMetadata = metadataRef,
                IsDeleted = dto.IsDeleted,
                IsMetadataStored = dto.IsMetadataStored
            };
        }

        public async Task<UsersMetadata> MapToMetadataAsync(UsersMetadataDTO dto)
        {
            DocumentReference? creationDevice = null;
            DocumentReference? deletionDevice = null;

            if (!string.IsNullOrEmpty(dto.CreationDeviceId))
            {
                var deviceRef = _devicesCollection.Document(dto.CreationDeviceId);
                var snapshot = await deviceRef.GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    creationDevice = deviceRef;
                }
            }

            if (!string.IsNullOrEmpty(dto.DeletionDeviceId))
            {
                var deviceRef = _devicesCollection.Document(dto.DeletionDeviceId);
                var snapshot = await deviceRef.GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    deletionDevice = deviceRef;
                }
            }

            return new UsersMetadata
            {
                CreationDate = dto.CreationDate,
                CreationDevice = creationDevice,
                CreationLocation = dto.CreationLocation,
                DeletionDate = dto.DeletionDate,
                DeletionDevice = deletionDevice,
                DeletionLocation = dto.DeletionLocation
            };
        }
    }
}
