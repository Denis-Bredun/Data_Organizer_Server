using Data_Organizer_Server.Entities;
using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Interfaces
{
    public interface IDeviceInfoRepository
    {
        Task<DocumentReference> CreateDeviceAsync(DeviceInfoModel device);
        Task<DocumentReference> GetDeviceDocRefByCombinedInfo(DeviceInfoModel device);
    }
}
