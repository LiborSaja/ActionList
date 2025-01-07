namespace ActionList.Utility {
    public class Uuid7Extractor {

        public static DateTime ExtractCreationTime(string uuid) {
            // Parsování UUID na pole bajtů
            var bytes = Guid.Parse(uuid).ToByteArray();

            // Extrahování timestampu z prvních 6 bajtů
            long timestamp = ((long)bytes[0] << 40) |
                             ((long)bytes[1] << 32) |
                             ((long)bytes[2] << 24) |
                             ((long)bytes[3] << 16) |
                             ((long)bytes[4] << 8) |
                             ((long)bytes[5]);

            // Převedení timestampu na DateTime
            return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;
        }

    }
}
