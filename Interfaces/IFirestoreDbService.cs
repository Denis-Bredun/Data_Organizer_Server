using Data_Organizer_Server.Models;

namespace Data_Organizer_Server.Interfaces
{
    public interface IFirestoreDbService
    {
        Task<AccountLogin?> CreateAccountLoginAsync(AccountLoginCreationRequest request);
        Task<AccountLogout?> CreateAccountLogoutAsync(AccountLogoutCreationRequest request);
        Task<ChangePassword?> CreateChangePasswordAsync(ChangePasswordCreationRequest request);
        Task<Note> CreateNoteAsync(Note note);
        Task<UserCreationRequest> CreateUserAsync(UserCreationRequest request);
        Task<NoteBody> GetNoteBodyByHeaderAsync(NoteHeader noteHeader);
        Task<IEnumerable<NoteHeader>> GetNoteHeadersByUidAsync(string uid);
        Task<User> GetUserByUidAsync(string uid);
        Task RemoveNoteAsync(NoteHeader noteHeader);
        Task<bool> RemoveUserAsync(UserCreationRequest request);
        Task UpdateNoteAsync(Note note);
        Task UpdateUserAsync(User user);
    }
}
