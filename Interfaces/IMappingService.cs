using Data_Organizer.DTOs;
using Data_Organizer_Server.Entities;

namespace Data_Organizer_Server.Interfaces
{
    public interface IMappingService
    {
        UserDTO MapUser(User user);
        UsersMetadataDTO MapMetadata(UsersMetadata metadata);

        Task<User> MapToUserAsync(UserDTO dto);
        Task<UsersMetadata> MapToMetadataAsync(UsersMetadataDTO dto);
    }
}
