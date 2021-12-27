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
            var randomDataBytes = Utils.GenerateBytes(16);
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
    }
}
