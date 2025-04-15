using Data_Organizer_Server.Interfaces;
using Data_Organizer_Server.Models;
using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Services
{
    public class FirestoreDbService : IFirestoreDbService
    {
        private readonly IAccountLoginRepository _accountLoginRepository;
        private readonly IAccountLogoutRepository _accountLogoutRepository;
        private readonly IChangePasswordRepository _changePasswordRepository;
        private readonly IDeviceInfoRepository _deviceInfoRepository;
        private readonly INoteRepository _noteRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUsersMetadataRepository _usersMetadataRepository;
        private readonly ILogger<FirestoreDbService> _logger;

        public FirestoreDbService(
            IAccountLoginRepository accountLoginRepository,
            IAccountLogoutRepository accountLogoutRepository,
            IChangePasswordRepository changePasswordRepository,
            IDeviceInfoRepository deviceInfoRepository,
            INoteRepository noteRepository,
            IUserRepository userRepository,
            IUsersMetadataRepository usersMetadataRepository,
            ILogger<FirestoreDbService> logger)
        {
            _accountLoginRepository = accountLoginRepository;
            _accountLogoutRepository = accountLogoutRepository;
            _changePasswordRepository = changePasswordRepository;
            _deviceInfoRepository = deviceInfoRepository;
            _noteRepository = noteRepository;
            _userRepository = userRepository;
            _usersMetadataRepository = usersMetadataRepository;
            _logger = logger;
        }

        public async Task<UserCreationRequest> CreateUserAsync(UserCreationRequest userCreationRequest)
        {
            var user = userCreationRequest.User;
            var userMetadata = userCreationRequest.UsersMetadata;

            if (userMetadata != null)
            {
                var metadataRef = await _usersMetadataRepository.CreateMetadataAsync(userMetadata);
                user.UsersMetadata = metadataRef;
                _logger.LogInformation("Metadata document created successfully for user UID: {Uid}", user.Uid);
            }

            await _userRepository.CreateUserAsync(user);
            return userCreationRequest;
        }

        public async Task<User> GetUserByUidAsync(string uid)
        {
            return await _userRepository.GetUserByUidAsync(uid);
        }

        public async Task UpdateUserAsync(User user)
        {
            await _userRepository.UpdateUserAsync(user);
        }

        public async Task<bool> RemoveUserAsync(UserCreationRequest request)
        {
            var user = request.User;

            if (user.IsMetadataStored)
            {
                if (request.UsersMetadata == null)
                    return false;

                await _usersMetadataRepository.UpdateMetadataAsync(user.Uid, request.UsersMetadata);
                _logger.LogInformation("Metadata for user with UID '{Uid}' was updated before soft-delete.", user.Uid);
            }

            await _userRepository.RemoveUserAsync(user.Uid);
            return true;
        }

        public async Task<ChangePassword?> CreateChangePasswordAsync(ChangePasswordCreationRequest request)
        {
            var (deviceDocRef, usersMetadataDocRef) = await GetDeviceAndMetadataAsync(request.Uid, request.DeviceInfo);

            var changePassword = request.ChangePassword;
            changePassword.Device = deviceDocRef;
            changePassword.UsersMetadata = usersMetadataDocRef;

            await _changePasswordRepository.CreateChangePasswordAsync(changePassword);
            return changePassword;
        }

        public async Task<AccountLogin?> CreateAccountLoginAsync(AccountLoginCreationRequest request)
        {
            var (deviceDocRef, usersMetadataDocRef) = await GetDeviceAndMetadataAsync(request.UserId, request.DeviceInfo);

            var accountLogin = request.AccountLogin;
            accountLogin.Device = deviceDocRef;
            accountLogin.UsersMetadata = usersMetadataDocRef;

            await _accountLoginRepository.CreateAccountLoginAsync(accountLogin);
            return accountLogin;
        }

        public async Task<AccountLogout?> CreateAccountLogoutAsync(AccountLogoutCreationRequest request)
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
