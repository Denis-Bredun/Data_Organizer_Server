using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Firestore_Converters
{
    public class DateTimeTimestampConverter : IFirestoreConverter<DateTime>
    {
        public object ToFirestore(DateTime value)
        {
            var normalized = value.ToUniversalTime().AddTicks(-(value.Ticks % TimeSpan.TicksPerSecond));
            return Timestamp.FromDateTime(normalized);
        }

        public DateTime FromFirestore(object value)
        {
            if (value is Timestamp timestamp)
                return timestamp.ToDateTime();

            throw new ArgumentException("Invalid Firestore value for DateTime conversion.");
        }
    }
}
