using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RSB_GUI
{
    public class SquareEncryption
    {
        private int _blockSizeBytes = 0;
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
        public SquareEncryption(int? blockSize = null, Action stepUpdater = null)
        {
            BlockSize = blockSize ?? 128;
            updateCurrentStep = stepUpdater;
        }

        public byte[] Encrypt(byte[] data, CancellationToken cancellationToken)
        {
            if (data.Length % _blockSizeBytes != 0)
            {
                data = FillRemnantBytes(data);
            }
            TotalSteps = data.Length / _blockSizeBytes;
            for (int r = 0; r < data.Length; r += _blockSizeBytes)
            {
                if (cancellationToken.IsCancellationRequested) return data;
                var linedSquare = new byte[_blockSizeBytes];
                Array.Copy(data, r, linedSquare, 0, _blockSizeBytes);
                var edgeLength = (int)Math.Sqrt(linedSquare.Length);
                byte[,] square = new byte[edgeLength, edgeLength];
                #region fillSquare
                for (int a = 0; a < edgeLength; a++)
                {
                    for (int b = 0; b < edgeLength; b++)
                    {
                        square[a, b] = linedSquare[a * edgeLength + b];
                    }
                }
                #endregion
                if (cancellationToken.IsCancellationRequested) return data;
                for (int i = 0; i < edgeLength; i++)
                {
                    #region absorption
                    byte sum = 0;
                    for (int j = 0; j < edgeLength; j++)
                    {
                        sum += square[i, j];
                    }
                    square[i, i] = sum;
                    #endregion
                }

                if (cancellationToken.IsCancellationRequested) return data;
                for (int i = 0; i < edgeLength; i++)
                {
                    #region propogation
                    for (int j = 0; j < edgeLength; j++)
                    {
                        if (j == i) continue;
                        square[j, i] = (byte)(square[j, i] + square[i, i]);
                    }
                    #endregion
                }

                if (cancellationToken.IsCancellationRequested) return data;
                for (int a = 0; a < edgeLength; a++)
                {
                    for (int b = 0; b < edgeLength; b++)
                    {
                        linedSquare[a * edgeLength + b] = square[a, b];
                    }
                }
                Array.Copy(linedSquare, 0, data, r, _blockSizeBytes);
                Step++;
            }
            return data;
        }
        public byte[] Decrypt(byte[] data, CancellationToken cancellationToken)
        {
            if (data.Length % _blockSizeBytes != 0)
            {
                data = FillRemnantBytes(data);
            }
            for (int r = 0; r < data.Length; r += _blockSizeBytes)
            {
                if (cancellationToken.IsCancellationRequested) return data;
                var linedSquare = new byte[_blockSizeBytes];
                Array.Copy(data, r, linedSquare, 0, _blockSizeBytes);
                var edgeLength = (int)Math.Sqrt(linedSquare.Length);
                byte[,] square = new byte[edgeLength, edgeLength];
                #region fillSquare
                for (int a = 0; a < edgeLength; a++)
                {
                    for (int b = 0; b < edgeLength; b++)
                    {
                        square[a, b] = linedSquare[a * edgeLength + b];
                    }
                }
                #endregion
                if (cancellationToken.IsCancellationRequested) return data;
                for (int i = 0; i < edgeLength; i++)
                {
                    #region reverse_propogation
                    for (int j = 0; j < edgeLength; j++)
                    {
                        if (j == i) continue;
                        square[j, i] = (byte)(square[j, i] - square[i, i]);
                    }
                    #endregion
                }

                if (cancellationToken.IsCancellationRequested) return data;
                for (int i = 0; i < edgeLength; i++)
                {
                    #region reverse_absorption
                    byte sum = 0;
                    for (int j = 0; j < edgeLength; j++)
                    {
                        if (j == i) continue;
                        sum += square[i, j];
                    }
                    square[i, i] = (byte)(square[i, i] - sum);
                    #endregion
                }

                if (cancellationToken.IsCancellationRequested) return data;
                for (int a = 0; a < edgeLength; a++)
                {
                    for (int b = 0; b < edgeLength; b++)
                    {
                        linedSquare[a * edgeLength + b] = square[a, b];
                    }
                }
                Array.Copy(linedSquare, 0, data, r, _blockSizeBytes);
            }
            return data;
        }
        public byte[] FillRemnantBytes(byte[] data)
        {
            var remnantBytes = data.Length % _blockSizeBytes;
            var bytesToAdd = remnantBytes == 0 ? remnantBytes : _blockSizeBytes - remnantBytes;
            byte[] filledArray = new byte[data.Length + bytesToAdd];
            Array.Copy(data, 0, filledArray, 0, data.Length);
            return filledArray;
        }

        private int totalSteps;
        private int step;
    }
}
