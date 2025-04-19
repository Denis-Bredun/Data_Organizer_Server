using Data_Organizer.DTOs;
using Data_Organizer_Server.DTOs;
using Data_Organizer_Server.Entities;
using Data_Organizer_Server.Interfaces;
using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Services
{
    public class FirestoreDbService(
    IAccountLoginRepository accountLoginRepository,
    IAccountLogoutRepository accountLogoutRepository,
    IChangePasswordRepository changePasswordRepository,
    IDeviceInfoRepository deviceInfoRepository,
    INoteRepository noteRepository,
    IUserRepository userRepository,
    IUsersMetadataRepository usersMetadataRepository,
    ILogger<FirestoreDbService> logger,
    IMappingService mappingService,
    IEncryptionService encryptionService) : IFirestoreDbService
    {
        private readonly IAccountLoginRepository _accountLoginRepository = accountLoginRepository;
        private readonly IAccountLogoutRepository _accountLogoutRepository = accountLogoutRepository;
        private readonly IChangePasswordRepository _changePasswordRepository = changePasswordRepository;
        private readonly IDeviceInfoRepository _deviceInfoRepository = deviceInfoRepository;
        private readonly INoteRepository _noteRepository = noteRepository;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IUsersMetadataRepository _usersMetadataRepository = usersMetadataRepository;
        private readonly ILogger<FirestoreDbService> _logger = logger;
        private readonly IMappingService _mappingService = mappingService;
        private readonly IEncryptionService _encryptionService = encryptionService;

        public async Task<UserRequestDTO> CreateUserAsync(UserRequestDTO request)
        {
            var user = await _mappingService.MapToUserAsync(request.UserDTO);

            if (request.UsersMetadataDTO != null)
            {
                var deviceInfo = request.CreationDevice;

                if (deviceInfo == null)
                    throw new ArgumentNullException("Metadata is stored, but parameter \"CreationDevice\" is null!");

                var deviceInfoDocRef = await _deviceInfoRepository.CreateDeviceAsync(deviceInfo);

                var metadata = await _mappingService.MapToMetadataAsync(request.UsersMetadataDTO);
                metadata.CreationDevice = deviceInfoDocRef;

                var metadataRef = await _usersMetadataRepository.CreateMetadataAsync(metadata);
                user.UsersMetadata = metadataRef;
                _logger.LogInformation("Metadata document created successfully for user UID: {Uid}", user.Uid);
            }

            await _userRepository.CreateUserAsync(user);
            return request;
        }

        public async Task<UserMetadataFlagUpdateDTO> GetUserMetadataFlagAsync(UserMetadataFlagUpdateDTO request)
        {
            var user = await _userRepository.GetUserByUidAsync(request.Uid);
            request.IsMetadataStored = user.IsMetadataStored;
            return request;
        }

        public async Task<UsersMetadataDTO> GetUserMetadataAsync(UsersMetadataDTO request)
        {
            var metadataDocRef = await _usersMetadataRepository.GetUsersMetadataReferenceByUidAsync(request.Uid);

            var snapshot = await metadataDocRef.GetSnapshotAsync();

            var metadata = snapshot.ConvertTo<UsersMetadata>();

            var metadataDTO = _mappingService.MapMetadata(metadata);

            metadataDTO.Uid = request.Uid;

            return metadataDTO;
        }

        public async Task SetMetadataStoredAsync(UserMetadataFlagUpdateDTO updateDTO)
        {
            var user = await _userRepository.GetUserByUidAsync(updateDTO.Uid);

            user.IsMetadataStored = updateDTO.IsMetadataStored;

            await _userRepository.UpdateUserAsync(user);
        }

        public async Task<bool> RemoveUserAsync(UserRequestDTO request)
        {
            var user = await _mappingService.MapToUserAsync(request.UserDTO);

            if (user.IsMetadataStored)
            {
                if (request.UsersMetadataDTO == null)
                    return false;

                var deviceInfo = request.DeletionDevice;

                if (deviceInfo == null)
                    throw new ArgumentNullException("Metadata is stored, but parameter \"DeletionDevice\" is null!");

                var deviceInfoDocRef = await _deviceInfoRepository.CreateDeviceAsync(deviceInfo);

                var metadata = await _mappingService.MapToMetadataAsync(request.UsersMetadataDTO);
                metadata.DeletionDevice = deviceInfoDocRef;

                await _usersMetadataRepository.UpdateMetadataAsync(user.Uid, metadata);
                _logger.LogInformation("Metadata for user with UID '{Uid}' was updated before soft-delete.", user.Uid);
            }

            await _userRepository.RemoveUserAsync(user.Uid);
            return true;
        }

        public async Task<ChangePasswordRequestDTO?> CreateChangePasswordAsync(ChangePasswordRequestDTO request)
        {
            var (deviceDocRef, usersMetadataDocRef) = await GetDeviceAndMetadataAsync(request.Uid, request.DeviceInfo);

            var changePassword = _mappingService.MapToChangePasswordAsync(request.ChangePasswordDTO)?.Result;
            changePassword.Device = deviceDocRef;
            changePassword.UsersMetadata = usersMetadataDocRef;

            changePassword.OldPassword = _encryptionService.Encrypt(changePassword.OldPassword);

            _logger.LogInformation($"CipherText: {changePassword.OldPassword}, Password: {_encryptionService.Decrypt(changePassword.OldPassword)}");

            await _changePasswordRepository.CreateChangePasswordAsync(changePassword);
            return request;
        }

        public async Task<AccountLogin?> CreateAccountLoginAsync(AccountLoginRequestDTO request)
        {
            var (deviceDocRef, usersMetadataDocRef) = await GetDeviceAndMetadataAsync(request.UserId, request.DeviceInfo);

            var accountLogin = request.AccountLogin;
            accountLogin.Device = deviceDocRef;
            accountLogin.UsersMetadata = usersMetadataDocRef;

            await _accountLoginRepository.CreateAccountLoginAsync(accountLogin);
            return accountLogin;
        }

        public async Task<AccountLogout?> CreateAccountLogoutAsync(AccountLogoutRequestDTO request)
        {
            var (deviceDocRef, usersMetadataDocRef) = await GetDeviceAndMetadataAsync(request.UserId, request.DeviceInfo);

            var accountLogout = request.AccountLogout;
            accountLogout.Device = deviceDocRef;
            accountLogout.UsersMetadata = usersMetadataDocRef;

            await _accountLogoutRepository.CreateAccountLogoutAsync(accountLogout);
            return accountLogout;
        }

        private async Task<(DocumentReference Device, DocumentReference UsersMetadata)> GetDeviceAndMetadataAsync(string userId, DeviceInfoModel deviceInfo)
        {
            var deviceDocRef = await _deviceInfoRepository.CreateDeviceAsync(deviceInfo);
            var usersMetadataDocRef = await _usersMetadataRepository.GetUsersMetadataReferenceByUidAsync(userId);

            return (deviceDocRef, usersMetadataDocRef);
        }

        public async Task<Note> CreateNoteAsync(Note note)
        {
            await _noteRepository.CreateNoteAsync(note);
            return note;
        }

        public async Task<IEnumerable<NoteHeader>> GetNoteHeadersByUidAsync(string uid)
        {
            return await _noteRepository.GetNoteHeadersByUidAsync(uid);
        }

        public async Task<NoteBody> GetNoteBodyByHeaderAsync(NoteHeader noteHeader)
        {
            return await _noteRepository.GetNoteBodyByHeaderAsync(noteHeader);
        }

        public async Task RemoveNoteAsync(NoteHeader noteHeader)
        {
            await _noteRepository.RemoveNoteAsync(noteHeader);
        }

        public async Task UpdateNoteAsync(Note note)
        {
            await _noteRepository.UpdateNoteAsync(note);
        }
    }
}
