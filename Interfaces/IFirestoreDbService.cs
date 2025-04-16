using Data_Organizer.DTOs;
using Data_Organizer_Server.DTOs;
using Data_Organizer_Server.Entities;

namespace Data_Organizer_Server.Interfaces
{
    public interface IFirestoreDbService
    {
        Task<AccountLogin?> CreateAccountLoginAsync(AccountLoginRequestDTO request);
        Task<AccountLogout?> CreateAccountLogoutAsync(AccountLogoutRequestDTO request);
        Task<ChangePassword?> CreateChangePasswordAsync(ChangePasswordRequestDTO request);
        Task<Note> CreateNoteAsync(Note note);
        Task<UserRequestDTO> CreateUserAsync(UserRequestDTO request);
        Task<NoteBody> GetNoteBodyByHeaderAsync(NoteHeader noteHeader);
        Task<IEnumerable<NoteHeader>> GetNoteHeadersByUidAsync(string uid);
        Task<UserMetadataFlagUpdateDTO> GetUserMetadataFlagAsync(UserMetadataFlagUpdateDTO request);
        Task RemoveNoteAsync(NoteHeader noteHeader);
        Task<bool> RemoveUserAsync(UserRequestDTO request);
        Task UpdateNoteAsync(Note note);
        Task SetMetadataStoredAsync(UserMetadataFlagUpdateDTO updateDTO);
    }
}
