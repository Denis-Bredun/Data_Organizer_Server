using Data_Organizer_Server.DTOs;
using Data_Organizer_Server.Entities;

namespace Data_Organizer_Server.Interfaces
{
    public interface IFirestoreDbService
    {
        Task<AccountLoginRequestDTO?> CreateAccountLoginAsync(AccountLoginRequestDTO request);
        Task<AccountLogoutRequestDTO?> CreateAccountLogoutAsync(AccountLogoutRequestDTO request);
        Task<ChangePasswordRequestDTO?> CreateChangePasswordAsync(ChangePasswordRequestDTO request);
        Task<NoteDTO> CreateNoteAsync(NoteDTO noteDTO);
        Task<UserRequestDTO> CreateUserAsync(UserRequestDTO request);
        Task<NoteBody> GetNoteBodyByHeaderAsync(NoteDTO request);
        Task<List<NoteDTO>> GetNoteHeadersByUidAsync(string uid);
        Task<UserMetadataFlagUpdateDTO> GetUserMetadataFlagAsync(UserMetadataFlagUpdateDTO request);
        Task RemoveNoteAsync(NoteDTO noteDTO);
        Task<bool> RemoveUserAsync(UserRequestDTO request);
        Task UpdateNoteAsync(NoteDTO request);
        Task SetMetadataStoredAsync(UserMetadataFlagUpdateDTO updateDTO);
        Task<UsersMetadataDTO> GetUserMetadataAsync(UsersMetadataDTO request);
    }
}
