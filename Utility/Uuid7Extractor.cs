namespace ActionList.Utility {
    public class Uuid7Extractor {
        public static DateTime ExtractCreationTime(string uuid) {
            // převedení stringu na Guid
            var guid = Guid.Parse(uuid);
            // parsování UUID na pole bajtů
            var bytes = guid.ToByteArray();
            // přeskládání bajtů pro správný timestamp (GUID v Little Endian)
            long timestamp = ((long)(bytes[3] & 0xFF) << 40) |
                             ((long)(bytes[2] & 0xFF) << 32) |
                             ((long)(bytes[1] & 0xFF) << 24) |
                             ((long)(bytes[0] & 0xFF) << 16) |
                             ((long)(bytes[5] & 0xFF) << 8) |
                             (long)(bytes[4] & 0xFF);

            // převedení timestampu na UTC DateTime
            return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;          
        }
    }
}
