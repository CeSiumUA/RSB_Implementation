using RSB.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using RSB.Core.GrayScheme;
using Xunit;

namespace CryptoTests
{
    public class Gray_Scheme_Test
    {
        [Fact]
        public void LeftProcessing_Test()
        {
            var randomKeyBytes = Utils.GenerateBytes(4);
            var randomDataBytes = Utils.GenerateBytes(24);
            var greyEncryptedArray = LeftGrayScheme.Encrypt(randomDataBytes, randomKeyBytes);
            Assert.Equal(greyEncryptedArray.Length, randomDataBytes.Length);
            var grayDecryptedArray = LeftGrayScheme.Decrypt(greyEncryptedArray, randomKeyBytes);
            Assert.Equal(grayDecryptedArray.Length, greyEncryptedArray.Length);
            Assert.Equal(grayDecryptedArray.Length, randomDataBytes.Length);
            for (int x = 0; x < grayDecryptedArray.Length; x++)
            {
                Assert.Equal(grayDecryptedArray[x], randomDataBytes[x]);
            }

        }
        [Fact]
        public void RightProcessing_Test()
        {
            var randomKeyBytes = Utils.GenerateBytes(4);
            var randomDataBytes = Utils.GenerateBytes(16);
            var greyEncryptedArray = RightGrayScheme.Encrypt(randomDataBytes, randomKeyBytes);
            Assert.Equal(greyEncryptedArray.Length, randomDataBytes.Length);
            var grayDecryptedArray = RightGrayScheme.Decrypt(greyEncryptedArray, randomKeyBytes);
            Assert.Equal(grayDecryptedArray.Length, greyEncryptedArray.Length);
            Assert.Equal(grayDecryptedArray.Length, randomDataBytes.Length);
            for (int x = 0; x < grayDecryptedArray.Length; x++)
            {
                Assert.Equal(grayDecryptedArray[x], randomDataBytes[x]);
            }
        }

        [Fact]
        public void RoundsProcessing_Test()
        {
            var randomKey = Utils.GenerateBytes(16);
            var randomData = Utils.GenerateBytes(16);
            byte[] encryptedData = new byte[randomData.Length];
            byte[] decryptedData = new byte[randomData.Length];
            Array.Copy(randomData, encryptedData, encryptedData.Length);
            var rounds = randomKey.Length / 4;
            for (int x = 0; x < rounds; x++)
            {
                var roundKey = new byte[4];
                Array.Copy(randomKey, x * 4, roundKey, 0, 4);
                if (x % 2 == 1)
                {
                    var encr = RightGrayScheme.Encrypt(encryptedData, roundKey);
                    Array.Copy(encr, 0, encryptedData, 0, encryptedData.Length);
                }
                else
                {
                    var encr = LeftGrayScheme.Encrypt(encryptedData, roundKey);
                    Array.Copy(encr, 0, encryptedData, 0, encryptedData.Length);
                }

                
            }

            Array.Copy(encryptedData, decryptedData, decryptedData.Length);

            for (int x = 0; x < rounds; x++)
            {
                var roundKey = new byte[4];
                Array.Copy(randomKey, (rounds - x - 1) * 4, roundKey, 0, 4);
                if (x % 2 == 0)
                {
                    var decr = RightGrayScheme.Decrypt(decryptedData, roundKey);
                    Array.Copy(decr, 0, decryptedData, 0, decryptedData.Length);
                }
                else
                {
                    var decr = LeftGrayScheme.Decrypt(decryptedData, roundKey);
                    Array.Copy(decr, 0, decryptedData, 0, decryptedData.Length);
                }
            }
            for (int x = 0; x < decryptedData.Length; x++)
            {
                Assert.Equal(randomData[x], decryptedData[x]);
            }
        }
    }
}
