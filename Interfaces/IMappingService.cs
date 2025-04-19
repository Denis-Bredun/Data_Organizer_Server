using Data_Organizer.DTOs;
using Data_Organizer_Server.DTOs;
using Data_Organizer_Server.Entities;

namespace Data_Organizer_Server.Interfaces
{
    public interface IMappingService
    {
        AccountLoginDTO MapAccountLogin(AccountLogin model);
        AccountLogoutDTO MapAccountLogout(AccountLogout model);
        ChangePasswordDTO MapChangePassword(ChangePassword model);
        UsersMetadataDTO MapMetadata(UsersMetadata metadata);
        Task<AccountLogin> MapToAccountLoginAsync(AccountLoginDTO dto);
        Task<AccountLogout> MapToAccountLogoutAsync(AccountLogoutDTO dto);
        Task<ChangePassword> MapToChangePasswordAsync(ChangePasswordDTO dto);
        Task<UsersMetadata> MapToMetadataAsync(UsersMetadataDTO dto);
        Task<User> MapToUserAsync(UserDTO dto);
        UserDTO MapUser(User user);
    }
}
