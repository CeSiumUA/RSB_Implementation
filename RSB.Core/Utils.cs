using System;
using System.Security.Cryptography;

namespace RSB.Core
{
    public class Utils
    {
        public static byte[] FillRemnantBytes(byte[] data, int blockSizeBytes)
        {
            var remnantBytes = data.Length % blockSizeBytes;
            var bytesToAdd = remnantBytes == 0 ? remnantBytes : blockSizeBytes - remnantBytes;
            byte[] filledArray = new byte[data.Length + bytesToAdd];
            Array.Copy(data, 0, filledArray, 0, data.Length);
            return filledArray;
        }
        public static byte[] GenerateBytes(int count)
        {
            byte[] bytes = new byte[count];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return bytes;
        }
    }
}