using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSB.Core.GrayScheme
{
    public static class LeftGrayScheme
    {
        public static byte[] Encrypt(byte[] array, byte[] key, int grayNumber = 4)
        {
            if (key.Length != grayNumber)
            {
                throw new ArgumentException("Key must be 4 bytes long!");
            }

            int operands = array.Length / grayNumber;
            var keyBits = new BitArray(key);
            BitArray[] bits = new BitArray[operands];
            for (int x = 0; x < bits.Length; x++)
            {
                byte[] operandBytes = new byte[grayNumber];
                Array.Copy(array, x * grayNumber, operandBytes, 0, grayNumber);
                var operandBits = new BitArray(operandBytes);
                bits[x] = operandBits;
            }

            for (int i = 0; i < keyBits.Length; i++)
            {
                var keyBit = keyBits[i];
                var matchingBits = new bool[operands];
                for (int j = 0; j < bits.Length; j++)
                {
                    matchingBits[j] = bits[j][i];
                }

                bool resultBit = keyBit;
                for (int j = 0; j < matchingBits.Length; j++)
                {
                    resultBit = resultBit ^ matchingBits[j];
                    matchingBits[j] = resultBit;
                }

                for (int j = 0; j < bits.Length; j++)
                {
                    bits[j][i] = matchingBits[j];
                }
            }

            byte[] resultByteArray = new byte[array.Length];
            for (int x = 0; x < bits.Length; x++)
            {
                var bitArray = bits[x];
                var bytesArray = new byte[bitArray.Length / 8];
                bitArray.CopyTo(bytesArray, 0);
                Array.Copy(bytesArray, 0, resultByteArray, x * bytesArray.Length, bytesArray.Length);
            }

            return resultByteArray;
        }

        public static byte[] Decrypt(byte[] array, byte[] key, int grayNumber = 4)
        {
            if (key.Length != grayNumber)
            {
                throw new ArgumentException("Key must be 4 bytes long!");
            }

            int operands = array.Length / grayNumber;
            var keyBits = new BitArray(key);
            BitArray[] bits = new BitArray[operands];
            for (int x = 0; x < bits.Length; x++)
            {
                byte[] operandBytes = new byte[grayNumber];
                Array.Copy(array, x * grayNumber, operandBytes, 0, grayNumber);
                var operandBits = new BitArray(operandBytes);
                bits[x] = operandBits;
            }

            for (int i = 0; i < keyBits.Length; i++)
            {
                var keyBit = keyBits[i];
                var matchingBits = new bool[operands];
                for (int j = 0; j < bits.Length; j++)
                {
                    matchingBits[j] = bits[j][i];
                }

                bool previousBit = keyBit;
                for (int j = 0; j < matchingBits.Length; j++)
                {
                    var temp = matchingBits[j];
                    matchingBits[j] = previousBit ^ matchingBits[j];
                    previousBit = temp;
                }

                for (int j = 0; j < bits.Length; j++)
                {
                    bits[j][i] = matchingBits[j];
                }
            }

            byte[] resultByteArray = new byte[array.Length];
            for (int x = 0; x < bits.Length; x++)
            {
                var bitArray = bits[x];
                var bytesArray = new byte[bitArray.Length / 8];
                bitArray.CopyTo(bytesArray, 0);
                Array.Copy(bytesArray, 0, resultByteArray, x * bytesArray.Length, bytesArray.Length);
            }

            return resultByteArray;
        }
    }
}
