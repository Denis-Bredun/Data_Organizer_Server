using Data_Organizer_Server.Interfaces;
using Data_Organizer_Server.Models;
using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Repositories
{
    public class NoteRepository : INoteRepository
    {
        private readonly CollectionReference _noteBodiesCollection;
        private readonly CollectionReference _noteHeadersCollection;

        public NoteRepository(ICollectionFactory collectionFactory)
        {
            _noteBodiesCollection = collectionFactory.GetNoteBodiesCollection();
            _noteHeadersCollection = collectionFactory.GetNoteHeadersCollection();
        }

        public async Task CreateNoteAsync(Note note)
        {
            if (note == null)
                throw new ArgumentNullException("Argument \"note\" is null while creating the note.");

            var body = note.Body;
            var header = note.Header;

            await _noteBodiesCollection.AddAsync(body);
            await _noteHeadersCollection.AddAsync(header);
        }

        public async Task<NoteBody> GetNoteBodyByHeaderAsync(NoteHeader noteHeader)
        {
            if (noteHeader == null)
                throw new ArgumentNullException("Argument \"noteHeader\" is null while getting the note.");

            var docRef = noteHeader.NoteBodyReference;

            var snapshot = await docRef.GetSnapshotAsync();

            var body = snapshot.ConvertTo<NoteBody>();

            return body;
        }

        public async Task<List<NoteHeader>> GetNoteHeadersByUidAsync(string uid)
        {
            if (string.IsNullOrWhiteSpace(uid))
                throw new ArgumentNullException("Argument \"uid\" is null or empty while getting the note headers.");

            var query = _noteHeadersCollection.WhereEqualTo("Uid", uid).WhereEqualTo("IsDeleted", false);
            var snapshot = await query.GetSnapshotAsync();
            var docs = snapshot.Documents;

            if (docs.Count == 0)
                throw new KeyNotFoundException($"Not deleted Note Headers with Uid '{uid}' were not found.");

            var noteHeaders = docs.Select(x => x.ConvertTo<NoteHeader>()).ToList();

            return noteHeaders;
        }

        public async Task RemoveNoteAsync(NoteHeader noteHeader)
        {
            noteHeader.IsDeleted = true;

            var note = new Note() { Header = noteHeader };

            await UpdateNoteAsync(note);
        }

        public async Task UpdateNoteAsync(Note note)
        {
            if (note == null)
                throw new ArgumentNullException("Argument \"note\" is null while updating the note.");

            if (note.Header == null)
                throw new ArgumentNullException("Argument \"note.Header\" is null while updating the note.");

            var header = note.Header;

            var headerDocRef = await GetHeaderDocRefByTime(header);
            await headerDocRef.SetAsync(header);

            if (note.Body != null)
            {
                var body = note.Body;
                var bodyDocRef = header.NoteBodyReference;
                await bodyDocRef.SetAsync(body);
            }
        }

        private async Task<DocumentReference> GetHeaderDocRefByTime(NoteHeader header)
        {
            header.CreationTime = header.CreationTime.ToUniversalTime();

            var query = _noteHeadersCollection.WhereEqualTo("CreationTime", header.CreationTime);
            var snapshot = await query.GetSnapshotAsync();
            var docs = snapshot.Documents;

            if (docs.Count == 0)
                throw new KeyNotFoundException($"Note Header with time '{header.CreationTime}' was not found.");

            var headerDocRef = docs[0].Reference;
            return headerDocRef;
        }
    }
}
