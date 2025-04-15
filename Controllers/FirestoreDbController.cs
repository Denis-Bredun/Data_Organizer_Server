using Data_Organizer_Server.DTOs;
using Data_Organizer_Server.Entities;
using Data_Organizer_Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Data_Organizer_Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("firestoredb")]
    public class FirestoreDbController(
    IFirestoreDbService firestoreDbService,
    ILogger<FirestoreDbController> logger) : ControllerBase
    {
        private readonly IFirestoreDbService _firestoreDbService = firestoreDbService;
        private readonly ILogger<FirestoreDbController> _logger = logger;


        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUserAsync([FromBody] UserRequestDTO userCreationRequest)
        {
            if (userCreationRequest == null || userCreationRequest.User == null)
            {
                var error = "Empty request or missing user data!";
                _logger.LogError("Received invalid user creation request: {Error}", error);
                return BadRequest(new UserRequestDTO
                {
                    Error = error
                });
            }

            try
            {
                var result = await _firestoreDbService.CreateUserAsync(userCreationRequest);
                _logger.LogInformation("User created successfully with UID: {Uid}", result.User.Uid);
                return Ok(result);
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
                var user = await _firestoreDbService.GetUserByUidAsync(uid);
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
        public async Task<IActionResult> UpdateUserAsync([FromBody] Entities.User user)
        {
            if (user == null)
            {
                var error = "User object is required for update.";
                _logger.LogError("Received null user in update request: {Error}", error);
                return BadRequest(new { Error = error });
            }

            try
            {
                await _firestoreDbService.UpdateUserAsync(user);
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
        public async Task<IActionResult> RemoveUserAsync([FromBody] UserRequestDTO request)
        {
            if (request == null || request.User == null)
            {
                var error = "Invalid request. User information is missing.";
                _logger.LogError("Invalid RemoveUser request: {Error}", error);
                return BadRequest(new { Error = error });
            }

            try
            {
                var result = await _firestoreDbService.RemoveUserAsync(request);

                if (!result)
                {
                    var metadataError = $"User '{request.User.Uid}' has metadata stored, but UsersMetadata object is null in request.";
                    _logger.LogWarning(metadataError);
                    return BadRequest(new { Error = metadataError });
                }

                _logger.LogInformation("User with UID '{Uid}' was successfully soft-deleted.", request.User.Uid);
                return Ok(request);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Argument null error during user removal.");
                return BadRequest(new { Error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found for UID '{Uid}'.", request.User.Uid);
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during removal of user with UID '{Uid}'.", request.User.Uid);
                return StatusCode(500, new { Error = "An internal server error occurred. Please try again later." });
            }
        }

        [HttpPost("create-change-password")]
        public async Task<IActionResult> CreateChangePasswordAsync([FromBody] ChangePasswordRequestDTO request)
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
                var changePassword = await _firestoreDbService.CreateChangePasswordAsync(request);
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
                _logger.LogWarning(ex, "User not found for UID '{Uid}'.", request.Uid);
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during password change request creation.");
                return StatusCode(500, new { Error = "An internal server error occurred. Please try again later." });
            }
        }

        [HttpPost("create-account-login")]
        public async Task<IActionResult> CreateAccountLoginAsync([FromBody] AccountLoginRequestDTO request)
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
                var accountLogin = await _firestoreDbService.CreateAccountLoginAsync(request);
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
                _logger.LogWarning(ex, "User not found for UserID '{UserId}'.", request.UserId);
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during account login request creation.");
                return StatusCode(500, new { Error = "An internal server error occurred. Please try again later." });
            }
        }

        [HttpPost("create-account-logout")]
        public async Task<IActionResult> CreateAccountLogoutAsync([FromBody] AccountLogoutRequestDTO request)
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
                var accountLogout = await _firestoreDbService.CreateAccountLogoutAsync(request);
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
                _logger.LogWarning(ex, "User not found for UserID '{UserId}'.", request.UserId);
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
                var createdNote = await _firestoreDbService.CreateNoteAsync(note);
                _logger.LogInformation("Note created successfully for UID: {Uid}", note.Header.UserId);
                return Ok(createdNote);
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
                var headers = await _firestoreDbService.GetNoteHeadersByUidAsync(uid);
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
                var body = await _firestoreDbService.GetNoteBodyByHeaderAsync(noteHeader);
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
                await _firestoreDbService.RemoveNoteAsync(noteHeader);
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
                await _firestoreDbService.UpdateNoteAsync(note);
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
