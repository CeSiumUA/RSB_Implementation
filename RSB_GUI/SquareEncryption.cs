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
        private bool _useMainDiagonal = true;
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
        public SquareEncryption(int? blockSize = null, Action stepUpdater = null, bool useMainDiagonal = true)
        {
            BlockSize = blockSize ?? 128;
            updateCurrentStep = stepUpdater;
            _useMainDiagonal = useMainDiagonal;
            if(Math.Sqrt(_blockSizeBytes) % 1 != 0)
            {
                _useMainDiagonal = false;
            }
            else if(BlockSize == 128)
            {
                _useMainDiagonal = true;
            }
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
                if (_useMainDiagonal)
                {
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
                }
                else
                {
                    var blocks = squareSplittingDictionary[BlockSize];
                    for(int blockNumber = 0; blockNumber < blocks.Count; blockNumber++)
                    {
                        int edgeLength = 4;
                        var bytesCount = edgeLength * edgeLength;
                        var diagonalBlocks = blocks[blockNumber];
                        var linuedBlockSquare = new byte[bytesCount];
                        Array.Copy(linedSquare, blockNumber * bytesCount, linuedBlockSquare, 0, bytesCount);
                        var square = new byte[diagonalBlocks.Count, diagonalBlocks.Count];
                        if (cancellationToken.IsCancellationRequested) return data;
                        #region fillSquare
                        for (int a = 0; a < edgeLength; a++)
                        {
                            for (int b = 0; b < edgeLength; b++)
                            {
                                square[a, b] = linuedBlockSquare[a * edgeLength + b];
                            }
                        }
                        #endregion
                        #region absorption
                        for (int i = 0; i < edgeLength; i++)
                        {
                            byte sum = 0;
                            for (int j = 0; j < edgeLength; j++)
                            {
                                sum += square[i, j];
                            }
                            var setColumn = diagonalBlocks[i];
                            square[i, setColumn] = sum;
                        }
                        #endregion

                        #region propogation
                        for (int i = 0; i < edgeLength; i++)
                        {
                            for (int j = 0; j < edgeLength; j++)
                            {
                                var setColumn = diagonalBlocks[j];
                                if (i == setColumn) continue;
                                square[j, i] = (byte)(square[j, i] + square[j, setColumn]);
                            }
                        }
                        #endregion

                        #region fromSquare
                        for (int a = 0; a < edgeLength; a++)
                        {
                            for (int b = 0; b < edgeLength; b++)
                            {
                                linuedBlockSquare[a * edgeLength + b] = square[a, b];
                            }
                        }
                        #endregion
                        Array.Copy(linuedBlockSquare, 0, linedSquare, blockNumber * bytesCount, bytesCount);
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
                if (_useMainDiagonal)
                {
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
                }
                else
                {
                    var blocks = squareSplittingDictionary[BlockSize];
                    for (int blockNumber = 0; blockNumber < blocks.Count; blockNumber++)
                    {
                        int edgeLength = 4;
                        var bytesCount = (int)Math.Pow(edgeLength, 2);
                        var diagonalBlocks = blocks[blockNumber];
                        var linuedBlockSquare = new byte[bytesCount];
                        Array.Copy(linedSquare, blockNumber * bytesCount, linuedBlockSquare, 0, bytesCount);
                        var square = new byte[diagonalBlocks.Count, diagonalBlocks.Count];
                        if (cancellationToken.IsCancellationRequested) return data;
                        #region fillSquare
                        for (int a = 0; a < edgeLength; a++)
                        {
                            for (int b = 0; b < edgeLength; b++)
                            {
                                square[a, b] = linuedBlockSquare[a * edgeLength + b];
                            }
                        }
                        #endregion

                        #region reverse_propogation
                        for (int i = 0; i < edgeLength; i++)
                        {
                            for (int j = 0; j < edgeLength; j++)
                            {
                                var setCell = diagonalBlocks[j];
                                if (i == setCell) continue;
                                square[j, i] = (byte)(square[j, i] - square[j, setCell]);
                            }
                        }
                        #endregion

                        #region reverse_absorption
                        for (int i = 0; i < edgeLength; i++)
                        {
                            var setColumn = diagonalBlocks[i];
                            byte sum = 0;
                            for (int j = 0; j < edgeLength; j++)
                            {
                                if(setColumn == j) continue;
                                sum += square[i, j];
                            }
                            square[i, setColumn] = (byte)(square[i, setColumn] - sum);
                        }
                        #endregion

                        #region fromSquare
                        for (int a = 0; a < edgeLength; a++)
                        {
                            for (int b = 0; b < edgeLength; b++)
                            {
                                linuedBlockSquare[a * edgeLength + b] = square[a, b];
                            }
                        }
                        #endregion
                        Array.Copy(linuedBlockSquare, 0, linedSquare, blockNumber * bytesCount, bytesCount);
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
        private Dictionary<int, Dictionary<int, Dictionary<int, int>>> squareSplittingDictionary = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>()
        {
            {256, new Dictionary<int, Dictionary<int, int>>()
                {
                    {0, new Dictionary<int, int>()
                        {
                            {0, 0 },
                            {1, 1 },
                            {2, 2 },
                            {3, 3 },
                        }
                    },
                    {1, new Dictionary<int, int>()
                        {
                            {0, 3 },
                            {1, 2 },
                            {2, 1 },
                            {3, 0 }
                        }
                    }
                }
            },
            {512, new Dictionary<int, Dictionary<int, int>>()
                {
                    {0, new Dictionary<int, int>()
                        {
                            {0, 0 },
                            {1, 1 },
                            {2, 2 },
                            {3, 3 },
                        }
                    },
                    {1, new Dictionary<int, int>()
                        {
                            {0, 1 },
                            {1, 2 },
                            {2, 3 },
                            {3, 0 }
                        }
                    },
                    {2, new Dictionary<int, int>()
                        {
                            {0, 2 },
                            {1, 3 },
                            {2, 0 },
                            {3, 1 }
                        }
                    },
                    {3, new Dictionary<int, int>()
                        {
                            {0, 3 },
                            {1, 2 },
                            {2, 1 },
                            {3, 0 }
                        }
                    }
                }
            }
        };
    }
}
