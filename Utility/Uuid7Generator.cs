using System.Security.Cryptography;

namespace ActionList.Utility {
    public static class Uuid7Generator {
        public static Guid GenerateUuid7() {
            // Získání času v milisekundách od Unix Epochy
            var unixTimeMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Převod času na bajty (Big Endian formát)
            var uuid = new byte[16];
            uuid[0] = (byte)(unixTimeMilliseconds >> 40);
            uuid[1] = (byte)(unixTimeMilliseconds >> 32);
            uuid[2] = (byte)(unixTimeMilliseconds >> 24);
            uuid[3] = (byte)(unixTimeMilliseconds >> 16);
            uuid[4] = (byte)(unixTimeMilliseconds >> 8);
            uuid[5] = (byte)(unixTimeMilliseconds);

            // Nastavení verze 7
            uuid[6] = (byte)(0x70 | ((unixTimeMilliseconds >> 12) & 0x0F));

            // Nastavení varianty 10
            uuid[8] = (byte)(0x80 | (RandomNumberGenerator.GetInt32(0, 64)));

            // Vygenerování zbývajících náhodných bajtů
            RandomNumberGenerator.Fill(uuid.AsSpan(9, 7));

            return new Guid(uuid); // Vrácení jako Guid
        }

    }
}
