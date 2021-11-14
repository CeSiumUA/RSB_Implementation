using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquarePrimitive
{
    public class QuadEncryptor
    {
        private int _blockSizeBytes = 0;
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
        public QuadEncryptor(int? blockSize = null)
        {
            BlockSize = blockSize ?? 128;
        }

        public byte[] Encrypt(byte[] data)
        {
            if(data.Length % _blockSizeBytes != 0)
            {
                data = FillRemnantBytes(data);
            }
            for (int r = 0; r < data.Length; r += _blockSizeBytes)
            {
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
        public byte[] Decrypt(byte[] data)
        {
            if (data.Length % _blockSizeBytes != 0)
            {
                data = FillRemnantBytes(data);
            }
            for (int r = 0; r < data.Length; r += _blockSizeBytes)
            {
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
    }
}
