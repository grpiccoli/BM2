using System;
using System.IO;
using System.Security.Cryptography;

namespace BiblioMit.Extensions
{
    public static class Hash
    {
        public static string Get512(string file, int bufferSize = 1_000_000)
        {
            using var stream = new BufferedStream(File.OpenRead(file), bufferSize);
            using SHA512Managed sha = new();
            byte[] checksum = sha.ComputeHash(stream);
            return "sha512-" + Convert.ToBase64String(checksum);
        }
        public static string Nonce()
        {
            //Allocate a buffer
            var ByteArray = new byte[20];
            //Generate a cryptographically random set of bytes
            using var Rnd = RandomNumberGenerator.Create();
            Rnd.GetBytes(ByteArray);
            //Base64 encode and then return
            return Convert.ToBase64String(ByteArray);
        }
    }
}
