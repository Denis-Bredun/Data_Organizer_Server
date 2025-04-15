using Data_Organizer_Server.Entities;

namespace Data_Organizer_Server.DTOs
{
    public class UserRequestDTO
    {
        public User User { get; set; }
        public UsersMetadata UsersMetadata { get; set; }
        public string Error { get; internal set; }
    }
}
