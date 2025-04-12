using Data_Organizer_Server.Interfaces;
using Data_Organizer_Server.Models;
using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Repositories
{
    public class DeviceInfoRepository : IDeviceInfoRepository
    {
        private readonly CollectionReference _devicesCollection;

        public DeviceInfoRepository(ICollectionFactory collectionFactory)
        {
            _devicesCollection = collectionFactory.GetDevicesCollection();
        }

        public async Task<DocumentReference> CreateDeviceAsync(DeviceInfoModel device)
        {
            if (device == null)
                throw new ArgumentNullException("Argument \"device\" is null while creating the device.");

            DocumentReference? existingDeviceDocRef = null;

            try
            {
                existingDeviceDocRef = await GetDeviceDocRefByCombinedInfo(device);
            }
            catch (Exception)
            {
                var newDeviceDocRef = await _devicesCollection.AddAsync(device);
                return newDeviceDocRef;
            }

            return existingDeviceDocRef;
        }

        public async Task<DocumentReference> GetDeviceDocRefByCombinedInfo(DeviceInfoModel device)
        {
            if (device == null)
                throw new ArgumentNullException("Argument \"device\" is null while getting the DeviceDocRef.");

            var query = _devicesCollection.WhereEqualTo("DeviceInfoCombined", device.DeviceInfoCombined);
            var snapshot = await query.GetSnapshotAsync();
            var docs = snapshot.Documents;

            if (docs.Count == 0)
                throw new KeyNotFoundException($"Devices with combined info '{device.DeviceInfoCombined}' were not found.");

            var docRef = docs[0].Reference;
            return docRef;
        }
    }
}
