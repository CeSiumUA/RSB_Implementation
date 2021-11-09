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
        public int Step
        {
            get
            {
                return step;
            }
            set
            {
                step = value;
                updateCurrentStep();
            }
        }
        public int TotalSteps
        {
            get
            {
                return totalSteps;
            }
            set
            {
                totalSteps = value;
                updateCurrentStep();
            }
        }
        public Action updateCurrentStep;
        public RSBEcnryptor(int blockSize, int shiftValue, Action stepUpdater = null)
        {
            this.blockSize = blockSize;
            this.shiftValue = shiftValue;
            updateCurrentStep = stepUpdater;
        }
        public async Task<byte[]> EncryptBytes(byte[] bytes, CancellationToken cancellationToken = default)
        {
            object lck = new object();
            bytes = FillVoidCells(bytes);
            var bytesBlockCount = blockSize / 8;
            var encryptionSteps = bytes.Length / bytesBlockCount;
            TotalSteps = encryptionSteps;
            byte[] encryptedBytes = new byte[bytes.Length];
            List<Task> tasks = new List<Task>();
            var runs = Enumerable.Range(0, encryptionSteps);
            runs.ToList().ForEach(x =>
            {
                var shiftingTask = new Task(() =>
                {
                    if (cancellationToken.IsCancellationRequested) return;

                    byte[] blockBytes = new byte[bytesBlockCount];

                    Array.Copy(bytes, x * bytesBlockCount, blockBytes, 0, bytesBlockCount);
                    BitArray bitArray = new BitArray(blockBytes);

                    for (int i = 0; i < shiftValue % bitArray.Count; i++)
                    {
                        var temp = bitArray[bitArray.Length - 1];
                        for (int j = bitArray.Length - 1; j > 0; j--)
                        {
                            bitArray[j] = bitArray[j - 1];
                            if (cancellationToken.IsCancellationRequested) return;
                        }
                        bitArray[0] = temp;
                    }
                    bitArray.CopyTo(blockBytes, 0);
                    if (cancellationToken.IsCancellationRequested) return;
                    Array.Copy(blockBytes, 0, encryptedBytes, x * bytesBlockCount, blockBytes.Length);

                    Step++;
                });
                if (cancellationToken.IsCancellationRequested) return;
                shiftingTask.Start();
                tasks.Add(shiftingTask);
            });
            if (cancellationToken.IsCancellationRequested)
            {
                tasks.Clear();
                return encryptedBytes;
            }
            Task.WaitAll(tasks.ToArray());
            TotalSteps = Step;
            return encryptedBytes.ToArray();
        }
        public async Task<byte[]> DecryptBytes(byte[] encryptedBytes, CancellationToken cancellationToken = default)
        {
            byte[] decryptedBytes = new byte[encryptedBytes.Length];
            var bytesBlockCount = blockSize / 8;
            var encryptionSteps = encryptedBytes.Length / bytesBlockCount;
            TotalSteps = encryptionSteps;
            List<Task> tasks = new List<Task>();
            var runs = Enumerable.Range(0, encryptionSteps);
            runs.ToList().ForEach(x =>
            {
                var shiftingTask = new Task(() =>
                {
                    if (cancellationToken.IsCancellationRequested) return;

                    byte[] blockBytes = new byte[bytesBlockCount];

                    Array.Copy(encryptedBytes, x * bytesBlockCount, blockBytes, 0, bytesBlockCount);
                    BitArray bitArray = new BitArray(blockBytes);

                    for (int i = 0; i < shiftValue % bitArray.Count; i++)
                    {
                        var temp = bitArray[0];

                        for (int j = 0; j < bitArray.Length - 1; j++)
                        {
                            bitArray[j] = bitArray[j + 1];
                            if (cancellationToken.IsCancellationRequested) return;
                        }
                        bitArray[bitArray.Length - 1] = temp;
                    }
                    bitArray.CopyTo(blockBytes, 0);
                    if (cancellationToken.IsCancellationRequested) return;
                    Array.Copy(blockBytes, 0, decryptedBytes, x * bytesBlockCount, blockBytes.Length);

                    Step++;
                });
                shiftingTask.Start();
                tasks.Add(shiftingTask);
            });
            if (cancellationToken.IsCancellationRequested)
            {
                tasks.Clear();
                return encryptedBytes;
            }
            Task.WaitAll(tasks.ToArray());
            TotalSteps = Step;
            return decryptedBytes;
        }
        private byte[] FillVoidCells(byte[] bytes)
        {
            var bytesBlockCount = blockSize / 8;
            int remnantBytesCount = bytesBlockCount - bytes.Length % bytesBlockCount;
            var bytesList = bytes.ToList();
            bytesList.AddRange(Enumerable.Repeat<byte>(0, remnantBytesCount));
            return bytesList.ToArray();
        }

        private int totalSteps;
        private int step;
    }
}
