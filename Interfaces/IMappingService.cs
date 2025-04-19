﻿using Data_Organizer_Server.DTOs;
using Data_Organizer_Server.Entities;

namespace Data_Organizer_Server.Interfaces
{
    public interface IMappingService
    {
        UsersMetadataDTO MapMetadata(UsersMetadata metadata);
        Task<AccountLogin> MapToAccountLoginAsync(AccountLoginDTO dto);
        Task<AccountLogout> MapToAccountLogoutAsync(AccountLogoutDTO dto);
        Task<ChangePassword> MapToChangePasswordAsync(ChangePasswordDTO dto);
        Task<UsersMetadata> MapToMetadataAsync(UsersMetadataDTO dto);
        Task<User> MapToUserAsync(UserDTO dto);
    }
}
