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

        public int ScInterval
        {
            get
            {
                return _scInterval;
            }
            set
            {
                _scInterval = value;
            }
        }
        private int totalSteps;
        private int step;
        private int _scInterval;
        public SlideCodeEncryptor(int blockSize, int keySize, int scInterval, Action stepUpdater = null)
        {
            BlockSize = blockSize;
            KeySize = keySize;
            updateCurrentStep = stepUpdater;
            ScInterval = scInterval;
        }

        public byte[] Encrypt(byte[] data, byte[] key, CancellationToken cancellationToken = default(CancellationToken))
        {
            data = RSB.Core.Utils.FillRemnantBytes(data, BlockSizeBytes);
            int roundKeyLength = (ScInterval * 2 / 8);
            int rounds = key.Length / roundKeyLength;
            TotalSteps = data.Length / BlockSizeBytes;
            var stepDelta = TotalSteps / 10;
            var stepToUpdateOn = 0;
            for (int i = 0; i < data.Length / BlockSizeBytes; i++)
            {
                if (cancellationToken.IsCancellationRequested) break;
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
                    if (cancellationToken.IsCancellationRequested) break;
                    var roundKeyBytes = new byte[roundKeyLength];

                    Array.Copy(key, r * roundKeyLength, roundKeyBytes, 0, roundKeyLength);

                    var leftRoundKeyBytes = new byte[roundKeyLength / 2];
                    var rightRoundKeyBytes = new byte[roundKeyLength / 2];
                    
                    Array.Copy(roundKeyBytes, 0, leftRoundKeyBytes, 0, roundKeyLength / 2);
                    Array.Copy(roundKeyBytes, roundKeyLength / 2, rightRoundKeyBytes, 0, roundKeyLength / 2);
                    
                    var leftEncryptedBytes = LeftGrayScheme.Encrypt(encryptedBytes, leftRoundKeyBytes, leftRoundKeyBytes.Length);

                    byte[] encrypted = new byte[leftEncryptedBytes.Length];
                    Array.Copy(leftEncryptedBytes, encrypted, leftEncryptedBytes.Length);

                    var rightEncryptedBytes = RightGrayScheme.Encrypt(encrypted, rightRoundKeyBytes, rightRoundKeyBytes.Length);
                    
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
            int roundKeyLength = (ScInterval * 2 / 8);
            int rounds = key.Length / roundKeyLength;
            var stepDelta = TotalSteps / 10;
            var stepToUpdateOn = 0;
            for (int i = 0; i < data.Length / BlockSizeBytes; i++)
            {
                if (cancellationToken.IsCancellationRequested) break;
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
                    if (cancellationToken.IsCancellationRequested) break;
                    var roundKeyBytes = new byte[roundKeyLength];

                    Array.Copy(key, r * roundKeyLength, roundKeyBytes, 0, roundKeyLength);

                    var leftRoundKeyBytes = new byte[roundKeyLength / 2];
                    var rightRoundKeyBytes = new byte[roundKeyLength / 2];

                    Array.Copy(roundKeyBytes, 0, leftRoundKeyBytes, 0, roundKeyLength / 2);
                    Array.Copy(roundKeyBytes, roundKeyLength / 2, rightRoundKeyBytes, 0, roundKeyLength / 2);

                    var rightDecryptedBytes = RightGrayScheme.Decrypt(decryptedBytes, rightRoundKeyBytes, rightRoundKeyBytes.Length);

                    byte[] encrypted = new byte[rightDecryptedBytes.Length];
                    Array.Copy(rightDecryptedBytes, encrypted, rightDecryptedBytes.Length);

                    var leftDecryptedBytes = LeftGrayScheme.Decrypt(encrypted, leftRoundKeyBytes, leftRoundKeyBytes.Length);

                    Array.Copy(leftDecryptedBytes, decryptedBytes, leftDecryptedBytes.Length);
                }
                Array.Copy(decryptedBytes, 0, data, i * BlockSizeBytes, BlockSizeBytes);
            }

            Step = TotalSteps;
            return data;
        }
    }
}
