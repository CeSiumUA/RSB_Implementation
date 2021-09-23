using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RSB_GUI
{
    public class RSBEcnryptor
    {
        private int blockSize;
        private int shiftValue;
        public int Step = 0;
        public int TotalSteps = 0;
        public RSBEcnryptor(int blockSize, int shiftValue)
        {
            this.blockSize = blockSize;
            this.shiftValue = shiftValue;
        }
        public async Task<byte[]> EncryptBytes(byte[] bytes, CancellationToken cancellation = default)
        {
            bytes = FillVoidCells(bytes);
            var bytesBlockCount = blockSize / 8;
            var encryptionSteps = bytes.Length / bytesBlockCount;
            TotalSteps = encryptionSteps;
            byte[] encryptedBytes = new byte[bytes.Length];
            for(int x = 0; x < encryptionSteps; x++)
            {
                if (cancellation.IsCancellationRequested) break;

                byte[] blockBytes = new byte[bytesBlockCount];

                Array.Copy(bytes, x * bytesBlockCount, blockBytes, 0, bytesBlockCount);

                BitArray bitArray = new BitArray(blockBytes);

                for(int i = 0; i < shiftValue; i++)
                {
                    var temp = bitArray[bitArray.Length - 1];
                    for(int j = bitArray.Length - 1; j > 0; j--)
                    {
                        bitArray[j] = bitArray[j - 1];
                    }
                    bitArray[0] = temp;
                }

                bitArray.CopyTo(blockBytes, 0);

                Array.Copy(blockBytes, 0, encryptedBytes, x * bytesBlockCount, blockBytes.Length);

                Step = x;
            }
            return encryptedBytes.ToArray();
        }
        private byte[] FillVoidCells(byte[] bytes)
        {
            var bytesBlockCount = blockSize / 8;
            int remnantBytesCount = bytesBlockCount - bytes.Length % bytesBlockCount;
            var bytesList = bytes.ToList();
            bytesList.AddRange(Enumerable.Repeat<byte>(0, remnantBytesCount));
            return bytesList.ToArray();
        }
    }
}
