using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PE.Plugins.SecureStorage
{
    public class CryptoService
    {
        public static string HexHashString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("String cannot be empty");

            var hashed = HashString(value);
            return ByteArrayToString(hashed);
        }

        public static byte[] HashString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("String cannot be empty");

            return GetHash(value);
        }

        public static bool VerifyString(string value, byte[] hash)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("String cannot be empty");

            return GetHash(value).SequenceEqual(hash);
        }

        static byte[] GetHash(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("String cannot be empty");
			
            var md5 = MD5.Create();
            return md5.ComputeHash(Encoding.ASCII.GetBytes(value));
        }

        static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "");
        }
    }
}
