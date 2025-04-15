using Data_Organizer_Server.Entities;

namespace Data_Organizer_Server.DTOs
{
    public class ChangePasswordRequestDTO
    {
        public string Uid { get; set; }
        public ChangePassword ChangePassword { get; set; }
        public DeviceInfoModel DeviceInfo { get; set; }
    }
}
