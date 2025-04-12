using Data_Organizer_Server.Models;

namespace Data_Organizer_Server.Interfaces
{
    public interface INoteRepository
    {
        Task CreateNoteAsync(Note note);
        Task UpdateNoteAsync(Note note);
        Task<List<NoteHeader>> GetNoteHeadersByUidAsync(string uid);
        Task<NoteBody> GetNoteBodyByHeaderAsync(NoteHeader noteHeader);
        Task RemoveNoteAsync(NoteHeader noteHeader);
    }
}
