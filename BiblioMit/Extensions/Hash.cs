using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;

namespace BiblioMit.Extensions
{
    public static class Hash
    {
        public static string Get512(Uri url)
        {
            HttpWebRequest fileReq = WebRequest.CreateHttp(url);
            var fileResp = fileReq.GetResponse();
            if (fileReq.ContentLength > 0)
                fileResp.ContentLength = fileReq.ContentLength;

            //Get the Stream returned from the response
            using var stream = fileResp.GetResponseStream();

            //using var stream = new BufferedStream(File.OpenRead(file), bufferSize);
            using SHA512Managed sha = new();
            byte[] checksum = sha.ComputeHash(stream);
            return "sha512-" + Convert.ToBase64String(checksum);
        }
        public static string Get512Local(string file, int bufferSize = 1_000_000)
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
