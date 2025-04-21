using Data_Organizer_Server.DTOs;
using Data_Organizer_Server.Entities;
using Data_Organizer_Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

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
            if (userCreationRequest == null || userCreationRequest.UserDTO == null)
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
                _logger.LogInformation("User created successfully with UID: {Uid}", result.UserDTO.Uid);
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

        [HttpPost("get-metadata")]
        public async Task<IActionResult> GetUserMetadataAsync([FromBody] UsersMetadataDTO request)
        {
            if (request == null)
            {
                var error = "Request body is required.";
                _logger.LogError("Null request received: {Error}", error);
                return BadRequest(new UsersMetadataDTO { Error = error });
            }

            if (string.IsNullOrWhiteSpace(request.Uid))
            {
                request.Error = "UID is required to retrieve metadata.";
                _logger.LogError("Invalid UID: {Error}", request.Error);
                return BadRequest(request);
            }

            try
            {
                var result = await _firestoreDbService.GetUserMetadataAsync(request);
                _logger.LogInformation("Retrieved metadata for user UID '{Uid}'", request.Uid);
                return Ok(result);
            }
            catch (ArgumentNullException ex)
            {
                request.Error = ex.Message;
                _logger.LogError(ex, "UID was null or empty.");
                return BadRequest(request);
            }
            catch (KeyNotFoundException ex)
            {
                request.Error = ex.Message;
                _logger.LogError(ex, "User metadata for UID '{Uid}' not found.", request.Uid);
                return NotFound(request);
            }
            catch (Exception ex)
            {
                request.Error = "An internal server error occurred. Please try again later.";
                _logger.LogError(ex, "Unexpected error retrieving metadata for UID '{Uid}'.", request.Uid);
                return StatusCode(500, request);
            }
        }

        [HttpPost("get-metadata-flag")]
        public async Task<IActionResult> GetUserMetadataFlagAsync([FromBody] UserMetadataFlagUpdateDTO request)
        {
            if (request == null)
            {
                var error = "Request body is required.";
                _logger.LogError("Null request received: {Error}", error);
                return BadRequest(new UserMetadataFlagUpdateDTO { Error = error });
            }

            if (string.IsNullOrWhiteSpace(request.Uid))
            {
                request.Error = "UID is required to retrieve metadata flag.";
                _logger.LogError("Invalid UID: {Error}", request.Error);
                return BadRequest(request);
            }

            try
            {
                var result = await _firestoreDbService.GetUserMetadataFlagAsync(request);
                _logger.LogInformation("Retrieved metadata flag for user UID '{Uid}' = {Flag}", request.Uid, result.IsMetadataStored);
                return Ok(result);
            }
            catch (ArgumentNullException ex)
            {
                request.Error = ex.Message;
                _logger.LogError(ex, "UID was null or empty.");
                return BadRequest(request);
            }
            catch (KeyNotFoundException ex)
            {
                request.Error = ex.Message;
                _logger.LogError(ex, "User with UID '{Uid}' not found.", request.Uid);
                return NotFound(request);
            }
            catch (Exception ex)
            {
                request.Error = "An internal server error occurred. Please try again later.";
                _logger.LogError(ex, "Unexpected error retrieving metadata flag for UID '{Uid}'.", request.Uid);
                return StatusCode(500, request);
            }
        }

        [HttpPut("set-metadata-flag")]
        public async Task<IActionResult> SetUserMetadataFlagAsync([FromBody] UserMetadataFlagUpdateDTO request)
        {
            if (request == null)
            {
                var error = "Request body is required.";
                _logger.LogError("Null updateDTO received: {Error}", error);
                return BadRequest(new UserMetadataFlagUpdateDTO { Error = error });
            }

            if (string.IsNullOrWhiteSpace(request.Uid))
            {
                request.Error = "UID is required to update metadata flag.";
                _logger.LogError("Invalid UID: {Error}", request.Error);
                return BadRequest(request);
            }

            try
            {
                await _firestoreDbService.SetMetadataStoredAsync(request);
                _logger.LogInformation("User's isMetadataStored flag updated successfully for UID '{Uid}'.", request.Uid);
                return Ok(request);
            }
            catch (ArgumentNullException ex)
            {
                request.Error = ex.Message;
                _logger.LogError(ex, "User was null during update.");
                return BadRequest(request);
            }
            catch (KeyNotFoundException ex)
            {
                request.Error = ex.Message;
                _logger.LogError(ex, "User with UID '{Uid}' not found during update.", request.Uid);
                return NotFound(request);
            }
            catch (Exception ex)
            {
                request.Error = "An internal server error occurred. Please try again later.";
                _logger.LogError(ex, "Unexpected error updating user with UID '{Uid}'.", request.Uid);
                return StatusCode(500, request);
            }
        }

        [HttpDelete("remove-user")]
        public async Task<IActionResult> RemoveUserAsync([FromBody] UserRequestDTO request)
        {
            if (request == null || request.UserDTO == null)
            {
                var error = "Invalid request. User information is missing.";
                _logger.LogError("Null request received: {Error}", error);
                return BadRequest(new UserRequestDTO { Error = error });
            }

            try
            {
                var result = await _firestoreDbService.RemoveUserAsync(request);

                if (!result)
                {
                    request.Error = $"User '{request.UserDTO.Uid}' has metadata stored, but UsersMetadata object is null in request.";
                    _logger.LogError("Metadata inconsistency during user removal: {Error}", request.Error);
                    return BadRequest(request);
                }

                _logger.LogInformation("User with UID '{Uid}' was successfully soft-deleted.", request.UserDTO.Uid);
                return Ok(request);
            }
            catch (ArgumentNullException ex)
            {
                request.Error = ex.Message;
                _logger.LogError(ex, "Argument null error during user removal.");
                return BadRequest(request);
            }
            catch (KeyNotFoundException ex)
            {
                request.Error = ex.Message;
                _logger.LogError(ex, "User not found for UID '{Uid}'.", request.UserDTO.Uid);
                return NotFound(request);
            }
            catch (Exception ex)
            {
                request.Error = "An internal server error occurred. Please try again later.";
                _logger.LogError(ex, "Unexpected error during removal of user with UID '{Uid}'.", request.UserDTO.Uid);
                return StatusCode(500, request);
            }
        }


        [HttpPost("create-change-password")]
        public async Task<IActionResult> CreateChangePasswordAsync([FromBody] ChangePasswordRequestDTO request)
        {
            if (request == null ||
                request.ChangePasswordDTO == null ||
                request.DeviceInfo == null ||
                string.IsNullOrWhiteSpace(request.Uid))
            {
                request ??= new ChangePasswordRequestDTO();
                request.Error = "Empty request or missing data!";
                _logger.LogError("Received invalid ChangePassword request: {Error}", request.Error);
                return BadRequest(request);
            }

            try
            {
                var changePasswordDTO = await _firestoreDbService.CreateChangePasswordAsync(request);
                _logger.LogInformation("Password change request created successfully for UID: {Uid}", request.Uid);
                return Ok(changePasswordDTO);
            }
            catch (ArgumentNullException ex)
            {
                request.Error = ex.Message;
                _logger.LogError(ex, "Invalid input data during password change request creation.");
                return BadRequest(request);
            }
            catch (KeyNotFoundException ex)
            {
                request.Error = ex.Message;
                _logger.LogError(ex, "User not found for UID '{Uid}'.", request.Uid);
                return NotFound(request);
            }
            catch (CryptographicException ex)
            {
                request.Error = "Encryption error occurred.";
                _logger.LogError(ex, "Cryptographic error during password encryption for UID '{Uid}': {Message}", request.Uid, ex.Message);
                return StatusCode(500, request);
            }
            catch (FormatException ex)
            {
                request.Error = "Invalid format in encryption data.";
                _logger.LogError(ex, "Format error during password encryption for UID '{Uid}': {Message}", request.Uid, ex.Message);
                return BadRequest(request);
            }
            catch (InvalidOperationException ex)
            {
                request.Error = ex.Message;
                _logger.LogError(ex, "Invalid operation during password change for UID '{Uid}': {Message}", request.Uid, ex.Message);
                return StatusCode(500, request);
            }
            catch (Exception ex)
            {
                request.Error = "An internal server error occurred. Please try again later.";
                _logger.LogError(ex, "Unexpected error during password change request creation for UID '{Uid}': {Message}", request.Uid, ex.Message);
                return StatusCode(500, request);
            }
        }

        [HttpPost("create-account-login")]
        public async Task<IActionResult> CreateAccountLoginAsync([FromBody] AccountLoginRequestDTO request)
        {
            if (request == null ||
                request.AccountLoginDTO == null ||
                request.DeviceInfo == null ||
                string.IsNullOrWhiteSpace(request.Uid))
            {
                request ??= new AccountLoginRequestDTO();
                request.Error = "Empty request or missing data!";
                _logger.LogError("Received invalid AccountLogin request: {Error}", request.Error);
                return BadRequest(request);
            }

            try
            {
                var accountLoginDTO = await _firestoreDbService.CreateAccountLoginAsync(request);
                _logger.LogInformation("Account login request created successfully for UserID: {UserId}", request.Uid);
                return Ok(accountLoginDTO);
            }
            catch (ArgumentNullException ex)
            {
                request.Error = ex.Message;
                _logger.LogError(ex, "Invalid input data during account login request creation.");
                return BadRequest(request);
            }
            catch (KeyNotFoundException ex)
            {
                request.Error = ex.Message;
                _logger.LogError(ex, "User not found for UserID '{UserId}'.", request.Uid);
                return NotFound(request);
            }
            catch (Exception ex)
            {
                request.Error = "An internal server error occurred. Please try again later.";
                _logger.LogError(ex, "Unexpected error during account login request creation for UserID '{UserId}': {Message}", request.Uid, ex.Message);
                return StatusCode(500, request);
            }
        }

        [HttpPost("create-account-logout")]
        public async Task<IActionResult> CreateAccountLogoutAsync([FromBody] AccountLogoutRequestDTO request)
        {
            if (request == null ||
                request.AccountLogoutDTO == null ||
                request.DeviceInfo == null ||
                string.IsNullOrWhiteSpace(request.Uid))
            {
                request ??= new AccountLogoutRequestDTO();
                request.Error = "Empty request or missing data!";
                _logger.LogError("Received invalid AccountLogout request: {Error}", request.Error);
                return BadRequest(request);
            }

            try
            {
                var accountLogoutDTO = await _firestoreDbService.CreateAccountLogoutAsync(request);
                _logger.LogInformation("Account logout request created successfully for UserID: {UserId}", request.Uid);
                return Ok(accountLogoutDTO);
            }
            catch (ArgumentNullException ex)
            {
                request.Error = ex.Message;
                _logger.LogError(ex, "Invalid input data during account logout request creation.");
                return BadRequest(request);
            }
            catch (KeyNotFoundException ex)
            {
                request.Error = ex.Message;
                _logger.LogError(ex, "User not found for UserID '{UserId}'.", request.Uid);
                return NotFound(request);
            }
            catch (Exception ex)
            {
                request.Error = "An internal server error occurred. Please try again later.";
                _logger.LogError(ex, "Unexpected error during account logout request creation for UserID '{UserId}': {Message}", request.Uid, ex.Message);
                return StatusCode(500, request);
            }
        }

        [HttpPost("create-note")]
        public async Task<IActionResult> CreateNoteAsync([FromBody] NoteDTO request)
        {
            if (request == null)
            {
                var error = "Request body is required.";
                _logger.LogError("Null NoteDTO received: {Error}", error);
                return BadRequest(new NoteDTO { Error = error });
            }

            if (string.IsNullOrWhiteSpace(request.UserId) ||
                string.IsNullOrWhiteSpace(request.Title) ||
                string.IsNullOrWhiteSpace(request.PreviewText) ||
                string.IsNullOrWhiteSpace(request.Content) ||
                request.CreationTime == default)
            {
                request.Error = "One or more required fields are missing or invalid.";
                _logger.LogError("Invalid note data: {Error}", request.Error);
                return BadRequest(request);
            }

            try
            {
                var createdNoteDTO = await _firestoreDbService.CreateNoteAsync(request);
                _logger.LogInformation("Note created successfully for UID: {Uid}", request.UserId);
                return Ok(createdNoteDTO);
            }
            catch (ArgumentNullException ex)
            {
                request.Error = ex.Message;
                _logger.LogError(ex, "Invalid input data during note creation.");
                return BadRequest(request);
            }
            catch (Exception ex)
            {
                request.Error = "An internal server error occurred. Please try again later.";
                _logger.LogError(ex, "Unexpected error during note creation.");
                return StatusCode(500, request);
            }
        }


        [HttpPost("headers")]
        public async Task<IActionResult> GetNoteHeadersByUidAsync([FromBody] UserDTO request)
        {
            if (request == null)
            {
                var error = "Request body is required.";
                _logger.LogError("Null UserDTO received: {Error}", error);
                return BadRequest(new UserDTO { Error = error });
            }

            if (string.IsNullOrWhiteSpace(request.Uid))
            {
                request.Error = "Uid field is missing or invalid.";
                _logger.LogError("Invalid user data: {Error}", request.Error);
                return BadRequest(request);
            }

            try
            {
                var headers = await _firestoreDbService.GetNoteHeadersByUidAsync(request.Uid);
                _logger.LogInformation("Note headers were got successfully for UID: {Uid}", request.Uid);
                return Ok(headers);
            }
            catch (ArgumentNullException ex)
            {
                request.Error = ex.Message;
                _logger.LogError(ex, "Invalid input data during note header creation.");
                return BadRequest(request);
            }
            catch (KeyNotFoundException ex)
            {
                request.Error = ex.Message;
                _logger.LogError(ex, "Note headers not found for UID: {Uid}. Returning empty list.", request.Uid);
                return NotFound(new List<NoteDTO>());
            }
            catch (Exception ex)
            {
                request.Error = "An internal server error occurred. Please try again later.";
                _logger.LogError(ex, "Unexpected error during note header creation.");
                return StatusCode(500, request);
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
        public async Task<IActionResult> RemoveNoteAsync([FromBody] NoteDTO noteDTO)
        {
            if (noteDTO == null ||
                string.IsNullOrEmpty(noteDTO.UserId) ||
                string.IsNullOrEmpty(noteDTO.Title) ||
                string.IsNullOrEmpty(noteDTO.PreviewText) ||
                noteDTO.CreationTime == default)
            {
                var error = "Invalid or incomplete note data.";
                _logger.LogError("Invalid RemoveNote request: {Error}", error);
                return BadRequest(new NoteDTO
                {
                    Error = error
                });
            }

            try
            {
                await _firestoreDbService.RemoveNoteAsync(noteDTO);
                _logger.LogInformation("Note marked as deleted for UID '{Uid}' and creation time '{CreationTime}'.",
                    noteDTO.UserId, noteDTO.CreationTime);
                return Ok(noteDTO);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Null argument encountered while removing note.");
                noteDTO.Error = ex.Message;
                return BadRequest(noteDTO);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Note not found during remove operation.");
                noteDTO.Error = ex.Message;
                return NotFound(noteDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while removing note.");
                noteDTO.Error = "An internal server error occurred. Please try again later.";
                return StatusCode(500, noteDTO);
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
                _logger.LogError(ex, "Note header not found during update. UID: {Uid}, Time: {Time}", note.Header?.UserId, note.Header?.CreationTime);
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
