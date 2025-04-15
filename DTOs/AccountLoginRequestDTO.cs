using Data_Organizer_Server.Entities;

namespace Data_Organizer_Server.DTOs
{
    public class AccountLoginRequestDTO
    {
        public string UserId { get; set; }
        public AccountLogin AccountLogin { get; set; }
        public DeviceInfoModel DeviceInfo { get; set; }
    }
}
