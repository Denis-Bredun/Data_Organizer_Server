using Data_Organizer_Server.Interfaces;
using Data_Organizer_Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Data_Organizer_Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("firestoredb")]
    public class FirestoreDbController : ControllerBase
    {
        private readonly IAccountLoginRepository _accountLoginRepository;
        private readonly IAccountLogoutRepository _accountLogoutRepository;
        private readonly IChangePasswordRepository _changePasswordRepository;
        private readonly IDeviceInfoRepository _deviceInfoRepository;
        private readonly INoteRepository _noteRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUsersMetadataRepository _usersMetadataRepository;
        private readonly ILogger<FirestoreDbController> _logger;

        public FirestoreDbController(
            IAccountLoginRepository accountLoginRepository,
            IAccountLogoutRepository accountLogoutRepository,
            IChangePasswordRepository changePasswordRepository,
            IDeviceInfoRepository deviceInfoRepository,
            INoteRepository noteRepository,
            IUserRepository userRepository,
            IUsersMetadataRepository usersMetadataRepository,
            ILogger<FirestoreDbController> logger)
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

        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUserAsync([FromBody] UserCreationRequest userCreationRequest)
        {
            if (userCreationRequest == null || userCreationRequest.User == null)
            {
                var error = "Empty request or missing user data!";
                _logger.LogError("Received invalid user creation request: {Error}", error);
                return BadRequest(new UserCreationRequest
                {
                    Error = error
                });
            }

            try
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

                _logger.LogInformation("User created successfully with UID: {Uid}", user.Uid);
                return Ok(userCreationRequest);
            }
            catch (ArgumentNullException ex)
            {
                userCreationRequest.Error = ex.Message;
                _logger.LogError(ex, "Invalid input data during user creation.");
                return BadRequest(userCreationRequest);
            }
            catch (Exception ex)
            {
                userCreationRequest.Error = "An internal server error occurred. Please try again later.";
                _logger.LogError(ex, "Unexpected error during user creation.");
                return StatusCode(500, userCreationRequest);
            }
        }

        [HttpGet("user/{uid}")]
        public async Task<IActionResult> GetUserByUidAsync(string uid)
        {
            if (string.IsNullOrWhiteSpace(uid))
            {
                var error = "UID is required to fetch user.";
                _logger.LogError("Invalid UID parameter: {Error}", error);
                return BadRequest(new { Error = error });
            }

            try
            {
                var user = await _userRepository.GetUserByUidAsync(uid);
                _logger.LogInformation("User with UID '{Uid}' retrieved successfully.", uid);
                return Ok(user);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "UID was null or empty while retrieving user.");
                return BadRequest(new { Error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User with UID '{Uid}' not found.", uid);
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while retrieving user with UID '{Uid}'.", uid);
                return StatusCode(500, new { Error = "An internal server error occurred. Please try again later." });
            }
        }

        [HttpPut("update-user")]
        public async Task<IActionResult> UpdateUserAsync([FromBody] Models.User user)
        {
            if (user == null)
            {
                var error = "User object is required for update.";
                _logger.LogError("Received null user in update request: {Error}", error);
                return BadRequest(new { Error = error });
            }

            try
            {
                await _userRepository.UpdateUserAsync(user);
                _logger.LogInformation("User with UID '{Uid}' was successfully updated.", user.Uid);
                return Ok(user);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "User was null during update.");
                return BadRequest(new { Error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User with UID '{Uid}' was not found during update.", user?.Uid);
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while updating user with UID '{Uid}'.", user?.Uid);
                return StatusCode(500, new { Error = "An internal server error occurred. Please try again later." });
            }
        }

        [HttpDelete("remove-user")]
        public async Task<IActionResult> RemoveUserAsync([FromBody] UserCreationRequest request)
        {
            if (request == null || request.User == null)
            {
                var error = "Invalid request. User information is missing.";
                _logger.LogError("Invalid RemoveUser request: {Error}", error);
                return BadRequest(new { Error = error });
            }

            var user = request.User;

            try
            {
                if (user.IsMetadataStored)
                {
                    if (request.UsersMetadata == null)
                    {
                        var metadataError = $"User '{user.Uid}' has metadata stored, but UsersMetadata object is null in request.";
                        _logger.LogWarning(metadataError);
                        return BadRequest(new { Error = metadataError });
                    }

                    await _usersMetadataRepository.UpdateMetadataAsync(user.Uid, request.UsersMetadata);
                    _logger.LogInformation("Metadata for user with UID '{Uid}' was updated before soft-delete.", user.Uid);
                }

                await _userRepository.RemoveUserAsync(user.Uid);
                _logger.LogInformation("User with UID '{Uid}' was successfully soft-deleted.", user.Uid);

                return Ok(request);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Argument null error during user removal.");
                return BadRequest(new { Error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User or metadata not found for UID '{Uid}'.", user.Uid);
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during removal of user with UID '{Uid}'.", user.Uid);
                return StatusCode(500, new { Error = "An internal server error occurred. Please try again later." });
            }
        }

        [HttpPost("create-change-password")]
        public async Task<IActionResult> CreateChangePasswordAsync([FromBody] ChangePasswordCreationRequest request)
        {
            if (request == null ||
                request.ChangePassword == null ||
                request.DeviceInfo == null ||
                string.IsNullOrEmpty(request.Uid))
            {
                var error = "Empty request or missing data!";
                _logger.LogError("Received invalid ChangePassword request: {Error}", error);
                return BadRequest(new { Error = error });
            }

            try
            {
                var deviceDocRef = await _deviceInfoRepository.CreateDeviceAsync(request.DeviceInfo);
                var usersMetadataDocRef = await _usersMetadataRepository.GetUsersMetadataReferenceByUidAsync(request.Uid);

                var changePassword = request.ChangePassword;
                changePassword.Device = deviceDocRef;
                changePassword.UsersMetadata = usersMetadataDocRef;

                var changePasswordDocRef = await _changePasswordRepository.CreateChangePasswordAsync(changePassword);

                _logger.LogInformation("Password change request created successfully for UID: {Uid}", request.Uid);
                return Ok(changePassword);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Invalid input data during password change request creation.");
                return BadRequest(new { Error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Metadata not found for UID '{Uid}'.", request.Uid);
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during password change request creation.");
                return StatusCode(500, new { Error = "An internal server error occurred. Please try again later." });
            }
        }

        [HttpPost("create-account-login")]
        public async Task<IActionResult> CreateAccountLoginAsync([FromBody] AccountLoginCreationRequest request)
        {
            if (request == null ||
                request.AccountLogin == null ||
                request.DeviceInfo == null ||
                string.IsNullOrEmpty(request.UserId))
            {
                var error = "Empty request or missing data!";
                _logger.LogError("Received invalid AccountLogin request: {Error}", error);
                return BadRequest(new { Error = error });
            }

            try
            {
                var deviceDocRef = await _deviceInfoRepository.CreateDeviceAsync(request.DeviceInfo);
                var usersMetadataDocRef = await _usersMetadataRepository.GetUsersMetadataReferenceByUidAsync(request.UserId);

                var accountLogin = request.AccountLogin;
                accountLogin.Device = deviceDocRef;
                accountLogin.UsersMetadata = usersMetadataDocRef;

                var accountLoginDocRef = await _accountLoginRepository.CreateAccountLoginAsync(accountLogin);

                _logger.LogInformation("Account login request created successfully for UserID: {UserId}", request.UserId);
                return Ok(accountLogin);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Invalid input data during account login request creation.");
                return BadRequest(new { Error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Metadata not found for UserID '{UserId}'.", request.UserId);
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during account login request creation.");
                return StatusCode(500, new { Error = "An internal server error occurred. Please try again later." });
            }
        }

        [HttpPost("create-account-logout")]
        public async Task<IActionResult> CreateAccountLogoutAsync([FromBody] AccountLogoutCreationRequest request)
        {
            if (request == null ||
                request.AccountLogout == null ||
                request.DeviceInfo == null ||
                string.IsNullOrEmpty(request.UserId))
            {
                var error = "Empty request or missing data!";
                _logger.LogError("Received invalid AccountLogout request: {Error}", error);
                return BadRequest(new { Error = error });
            }

            try
            {
                var deviceDocRef = await _deviceInfoRepository.CreateDeviceAsync(request.DeviceInfo);
                var usersMetadataDocRef = await _usersMetadataRepository.GetUsersMetadataReferenceByUidAsync(request.UserId);

                var accountLogout = request.AccountLogout;
                accountLogout.Device = deviceDocRef;
                accountLogout.UsersMetadata = usersMetadataDocRef;

                var accountLogoutDocRef = await _accountLogoutRepository.CreateAccountLogoutAsync(accountLogout);

                _logger.LogInformation("Account logout request created successfully for UserID: {UserId}", request.UserId);
                return Ok(accountLogout);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Invalid input data during account logout request creation.");
                return BadRequest(new { Error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Metadata not found for UserID '{UserId}'.", request.UserId);
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during account logout request creation.");
                return StatusCode(500, new { Error = "An internal server error occurred. Please try again later." });
            }
        }

        [HttpPost("create-note")]
        public async Task<IActionResult> CreateNoteAsync([FromBody] Note note)
        {
            if (note == null || note.Header == null || note.Body == null)
            {
                var error = "Empty note or missing data!";
                _logger.LogError("Received invalid CreateNote request: {Error}", error);
                return BadRequest(new { Error = error });
            }

            try
            {
                await _noteRepository.CreateNoteAsync(note);

                _logger.LogInformation("Note created successfully for UID: {Uid}", note.Header.UserId);
                return Ok(note);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Invalid input data during note creation.");
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during note creation.");
                return StatusCode(500, new { Error = "An internal server error occurred. Please try again later." });
            }
        }

        [HttpGet("headers/{uid}")]
        public async Task<IActionResult> GetNoteHeadersByUidAsync(string uid)
        {
            if (string.IsNullOrWhiteSpace(uid))
            {
                var error = "UID is required to fetch note headers.";
                _logger.LogError("Invalid UID parameter: {Error}", error);
                return BadRequest(new { Error = error });
            }

            try
            {
                var headers = await _noteRepository.GetNoteHeadersByUidAsync(uid);
                _logger.LogInformation("Note headers for UID '{Uid}' retrieved successfully.", uid);
                return Ok(headers);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "UID was null or empty while retrieving note headers.");
                return BadRequest(new { Error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "No note headers found for UID '{Uid}'.", uid);
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while retrieving note headers for UID '{Uid}'.", uid);
                return StatusCode(500, new { Error = "An internal server error occurred. Please try again later." });
            }
        }

        [HttpPost("body-by-header")]
        public async Task<IActionResult> GetNoteBodyByHeaderAsync([FromBody] NoteHeader noteHeader)
        {
            if (noteHeader == null)
            {
                var error = "NoteHeader is required to fetch note body.";
                _logger.LogError("Invalid NoteHeader parameter: {Error}", error);
                return BadRequest(new { Error = error });
            }

            try
            {
                var body = await _noteRepository.GetNoteBodyByHeaderAsync(noteHeader);
                _logger.LogInformation("Note body for header with UID '{Uid}' retrieved successfully.", noteHeader.UserId);
                return Ok(body);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "NoteHeader was null while retrieving note body.");
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while retrieving note body for NoteHeader.");
                return StatusCode(500, new { Error = "An internal server error occurred. Please try again later." });
            }
        }

        [HttpDelete("remove-note")]
        public async Task<IActionResult> RemoveNoteAsync([FromBody] NoteHeader noteHeader)
        {
            if (noteHeader == null)
            {
                var error = "NoteHeader is required to remove note.";
                _logger.LogError("Invalid NoteHeader parameter: {Error}", error);
                return BadRequest(new { Error = error });
            }

            try
            {
                await _noteRepository.RemoveNoteAsync(noteHeader);
                _logger.LogInformation("Note marked as deleted for UID '{Uid}' and creation time '{CreationTime}'.",
                    noteHeader.UserId, noteHeader.CreationTime);
                return Ok(noteHeader);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Null argument encountered while removing note.");
                return BadRequest(new { Error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Note header not found during remove operation.");
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while removing note.");
                return StatusCode(500, new { Error = "An internal server error occurred. Please try again later." });
            }
        }

        [HttpPut("update-note")]
        public async Task<IActionResult> UpdateNoteAsync([FromBody] Note note)
        {
            if (note == null || note.Header == null)
            {
                var error = "Note or its header must not be null.";
                _logger.LogError("Invalid update note request: {Error}", error);
                return BadRequest(new { Error = error });
            }

            try
            {
                await _noteRepository.UpdateNoteAsync(note);
                _logger.LogInformation("Note updated successfully for UID: {Uid} at {Time}", note.Header.UserId, note.Header.CreationTime);
                return Ok(note);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Invalid input data during note update.");
                return BadRequest(new { Error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Note header not found during update. UID: {Uid}, Time: {Time}", note.Header?.UserId, note.Header?.CreationTime);
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred during note update.");
                return StatusCode(500, new { Error = "An internal server error occurred. Please try again later." });
            }
        }

    }
}
