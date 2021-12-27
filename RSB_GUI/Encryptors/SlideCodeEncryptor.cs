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

        public byte[] Encrypt(byte[] data, byte[] key, CancellationToken cancellationToken)
        {
            data = RSB.Core.Utils.FillRemnantBytes(data, BlockSizeBytes);
            int rounds = key.Length / 4;
            TotalSteps = data.Length / BlockSizeBytes;
            for (int i = 0; i < data.Length / BlockSizeBytes; i++)
            {
                var bytesToProcess = new byte[BlockSizeBytes];
                Array.Copy(data, i * BlockSizeBytes, bytesToProcess, 0, BlockSizeBytes);
                for (int r = 0; r < rounds; r++)
                {
                    var roundKeyBytes = new byte[4];
                    Array.Copy(key, r * 4, roundKeyBytes, 0, 4);
                    if ((r + 1) % 2 == 0)
                    {
                        bytesToProcess = LeftGrayScheme.Encrypt(bytesToProcess, roundKeyBytes);
                    }
                    else
                    {
                        bytesToProcess = RightGrayScheme.Encrypt(bytesToProcess, roundKeyBytes);
                    }
                }
                Array.Copy(bytesToProcess, 0, data, i * BlockSizeBytes, BlockSizeBytes);
            }

            Step = TotalSteps;
            return data;
        }

        public byte[] Decrypt(byte[] data, byte[] key, CancellationToken cancellationToken)
        {
            data = RSB.Core.Utils.FillRemnantBytes(data, BlockSizeBytes);
            TotalSteps = data.Length / BlockSizeBytes;
            int rounds = key.Length / 4;
            for (int i = 0; i < data.Length / BlockSizeBytes; i++)
            {
                var bytesToProcess = new byte[BlockSizeBytes];
                Array.Copy(data, i * BlockSizeBytes, bytesToProcess, 0, BlockSizeBytes);
                for (int r = 0; r < rounds; r++)
                {
                    var roundKeyBytes = new byte[4];
                    Array.Copy(key, r * 4, roundKeyBytes, 0, 4);
                    if ((r + 1) % 2 == 0)
                    {
                        bytesToProcess = LeftGrayScheme.Decrypt(bytesToProcess, roundKeyBytes);
                    }
                    else
                    {
                        bytesToProcess = RightGrayScheme.Decrypt(bytesToProcess, roundKeyBytes);
                    }
                }
                Array.Copy(bytesToProcess, 0, data, i * BlockSizeBytes, BlockSizeBytes);
            }

            Step = TotalSteps;
            return data;
        }
    }
}
