using Data_Organizer_Server.Entities;

namespace Data_Organizer_Server.DTOs
{
    public class AccountLogoutRequestDTO
    {
        public string UserId { get; set; }
        public AccountLogout AccountLogout { get; set; }
        public DeviceInfoModel DeviceInfo { get; set; }
    }
}
