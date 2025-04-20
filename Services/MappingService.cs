using Data_Organizer_Server.DTOs;
using Data_Organizer_Server.Entities;
using Data_Organizer_Server.Interfaces;
using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Services
{
    public class MappingService(ICollectionFactory collectionFactory) : IMappingService
    {
        private readonly CollectionReference _usersMetadataCollection = collectionFactory.GetUsersMetadataCollection();
        private readonly CollectionReference _devicesCollection = collectionFactory.GetDevicesCollection();
        private readonly CollectionReference _noteHeadersCollection = collectionFactory.GetNoteHeadersCollection();

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

        public async Task<ChangePassword> MapToChangePasswordAsync(ChangePasswordDTO dto)
        {
            DocumentReference? metadataRef = null;
            DocumentReference? deviceRef = null;

            if (!string.IsNullOrEmpty(dto.UsersMetadataId))
            {
                var docRef = _usersMetadataCollection.Document(dto.UsersMetadataId);
                var snapshot = await docRef.GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    metadataRef = docRef;
                }
            }

            if (!string.IsNullOrEmpty(dto.DeviceId))
            {
                var docRef = _devicesCollection.Document(dto.DeviceId);
                var snapshot = await docRef.GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    deviceRef = docRef;
                }
            }

            return new ChangePassword
            {
                UsersMetadata = metadataRef,
                OldPassword = dto.OldPassword,
                Device = deviceRef,
                Location = dto.Location,
                Date = dto.Date
            };
        }

        public async Task<AccountLogin> MapToAccountLoginAsync(AccountLoginDTO dto)
        {
            DocumentReference? metadataRef = null;
            DocumentReference? deviceRef = null;

            if (!string.IsNullOrEmpty(dto.UsersMetadataId))
            {
                var docRef = _usersMetadataCollection.Document(dto.UsersMetadataId);
                var snapshot = await docRef.GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    metadataRef = docRef;
                }
            }

            if (!string.IsNullOrEmpty(dto.DeviceId))
            {
                var docRef = _devicesCollection.Document(dto.DeviceId);
                var snapshot = await docRef.GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    deviceRef = docRef;
                }
            }

            return new AccountLogin
            {
                UsersMetadata = metadataRef!,
                Device = deviceRef!,
                Location = dto.Location,
                Date = dto.Date
            };
        }

        public async Task<AccountLogout> MapToAccountLogoutAsync(AccountLogoutDTO dto)
        {
            DocumentReference? metadataRef = null;
            DocumentReference? deviceRef = null;

            if (!string.IsNullOrEmpty(dto.UsersMetadataId))
            {
                var docRef = _usersMetadataCollection.Document(dto.UsersMetadataId);
                var snapshot = await docRef.GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    metadataRef = docRef;
                }
            }

            if (!string.IsNullOrEmpty(dto.DeviceId))
            {
                var docRef = _devicesCollection.Document(dto.DeviceId);
                var snapshot = await docRef.GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    deviceRef = docRef;
                }
            }

            return new AccountLogout
            {
                UsersMetadata = metadataRef!,
                Device = deviceRef!,
                Location = dto.Location,
                Date = dto.Date
            };
        }

        public async Task<(NoteHeader Header, NoteBody Body)> MapToNoteAsync(NoteDTO dto)
        {
            DocumentReference? noteBodyRef = null;

            if (!string.IsNullOrEmpty(dto.NoteBodyId))
            {
                var docRef = _noteHeadersCollection.Document(dto.NoteBodyId);
                var snapshot = await docRef.GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    noteBodyRef = docRef;
                }
            }

            var header = new NoteHeader
            {
                UserId = dto.UserId,
                Title = dto.Title,
                PreviewText = dto.PreviewText,
                CreationTime = dto.CreationTime,
                IsDeleted = dto.IsDeleted,
                NoteBodyReference = noteBodyRef!
            };

            var body = new NoteBody
            {
                Content = dto.Content
            };

            return (header, body);
        }

        public NoteDTO MapNoteHeaderToDTO(NoteHeader header) => new NoteDTO
        {
            UserId = header.UserId,
            Title = header.Title,
            PreviewText = header.PreviewText,
            CreationTime = header.CreationTime,
            IsDeleted = header.IsDeleted,
            NoteBodyId = header.NoteBodyReference?.Id
        };
    }
}
