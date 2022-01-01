using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RSB.Core.GrayScheme;

namespace RSB_GUI.Encryptors
{
    public class SlideCodeEncryptor
    {
        private int _blockSizeBytes = 0;
        private int _keySizeBytes = 0;
        public Action updateCurrentStep;
        public int Step
        {
            get
            {
                return step;
            }
            set
            {
                step = value;
                if (updateCurrentStep != null)
                {
                    updateCurrentStep();
                }
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
                if (updateCurrentStep != null)
                {
                    updateCurrentStep();
                }
            }
        }
        public int BlockSizeBytes
        {
            get
            {
                return _blockSizeBytes;
            }
            set
            {
                _blockSizeBytes = value;
            }
        }
        public int BlockSize
        {
            get
            {
                return _blockSizeBytes * 8;
            }
            set
            {
                _blockSizeBytes = value / 8;
            }
        }

        public int KeySize
        {
            get
            {
                return _keySizeBytes * 8;
            }
            set
            {
                _keySizeBytes = value / 8;
            }
        }

        public int KeySizeBytes
        {
            get
            {
                return _keySizeBytes;
            }
            set
            {
                _keySizeBytes = value;
            }
        }
        private int totalSteps;
        private int step;
        public SlideCodeEncryptor(int blockSize, int keySize, Action stepUpdater = null)
        {
            BlockSize = blockSize;
            KeySize = keySize;
            updateCurrentStep = stepUpdater;
        }

        public byte[] Encrypt(byte[] data, byte[] key, CancellationToken cancellationToken = default(CancellationToken))
        {
            data = RSB.Core.Utils.FillRemnantBytes(data, BlockSizeBytes);
            int rounds = key.Length / 8;
            TotalSteps = data.Length / BlockSizeBytes;
            var stepDelta = TotalSteps / 10;
            var stepToUpdateOn = 0;
            for (int i = 0; i < data.Length / BlockSizeBytes; i++)
            {
                if (stepToUpdateOn == i)
                {
                    this.Step = i;
                    stepToUpdateOn += stepDelta;
                }
                var bytesToProcess = new byte[BlockSizeBytes];
                Array.Copy(data, i * BlockSizeBytes, bytesToProcess, 0, BlockSizeBytes);
                byte[] encryptedBytes = new byte[bytesToProcess.Length];
                Array.Copy(bytesToProcess, encryptedBytes, encryptedBytes.Length);
                for (int r = 0; r < rounds; r++)
                {
                    var roundKeyBytes = new byte[8];

                    Array.Copy(key, r * 8, roundKeyBytes, 0, 8);

                    var leftRoundKeyBytes = new byte[4];
                    var rightRoundKeyBytes = new byte[4];
                    
                    Array.Copy(roundKeyBytes, 0, leftRoundKeyBytes, 0, 4);
                    Array.Copy(roundKeyBytes, 4, rightRoundKeyBytes, 0, 4);
                    
                    var leftEncryptedBytes = LeftGrayScheme.Encrypt(encryptedBytes, leftRoundKeyBytes);

                    byte[] encrypted = new byte[leftEncryptedBytes.Length];
                    Array.Copy(leftEncryptedBytes, encrypted, leftEncryptedBytes.Length);

                    var rightEncryptedBytes = RightGrayScheme.Encrypt(encrypted, rightRoundKeyBytes);
                    
                    Array.Copy(rightEncryptedBytes, encryptedBytes, rightEncryptedBytes.Length);
                }
                Array.Copy(encryptedBytes, 0, data, i * BlockSizeBytes, BlockSizeBytes);
            }

            Step = TotalSteps;
            return data;
        }

        public byte[] Decrypt(byte[] data, byte[] key, CancellationToken cancellationToken = default(CancellationToken))
        {
            data = RSB.Core.Utils.FillRemnantBytes(data, BlockSizeBytes);
            TotalSteps = data.Length / BlockSizeBytes;
            int rounds = key.Length / 8;
            var stepDelta = TotalSteps / 10;
            var stepToUpdateOn = 0;
            for (int i = 0; i < data.Length / BlockSizeBytes; i++)
            {
                if (stepToUpdateOn == i)
                {
                    this.Step = i;
                    stepToUpdateOn += stepDelta;
                }
                var bytesToProcess = new byte[BlockSizeBytes];
                Array.Copy(data, i * BlockSizeBytes, bytesToProcess, 0, BlockSizeBytes);
                byte[] decryptedBytes = new byte[bytesToProcess.Length];
                Array.Copy(bytesToProcess, decryptedBytes, decryptedBytes.Length);
                for (int r = rounds - 1; r >= 0; r--)
                {
                    var roundKeyBytes = new byte[8];

                    Array.Copy(key, r * 8, roundKeyBytes, 0, 8);

                    var leftRoundKeyBytes = new byte[4];
                    var rightRoundKeyBytes = new byte[4];

                    Array.Copy(roundKeyBytes, 0, leftRoundKeyBytes, 0, 4);
                    Array.Copy(roundKeyBytes, 4, rightRoundKeyBytes, 0, 4);

                    var rightDecryptedBytes = RightGrayScheme.Decrypt(decryptedBytes, rightRoundKeyBytes);

                    byte[] encrypted = new byte[rightDecryptedBytes.Length];
                    Array.Copy(rightDecryptedBytes, encrypted, rightDecryptedBytes.Length);

                    var leftDecryptedBytes = LeftGrayScheme.Decrypt(encrypted, leftRoundKeyBytes);

                    Array.Copy(leftDecryptedBytes, decryptedBytes, leftDecryptedBytes.Length);
                }
                Array.Copy(decryptedBytes, 0, data, i * BlockSizeBytes, BlockSizeBytes);
            }

            Step = TotalSteps;
            return data;
        }
    }
}
