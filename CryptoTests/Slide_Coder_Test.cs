using System;
using System.Security.Cryptography;
using RSB.Core;
using RSB_GUI.Encryptors;
using Xunit;

namespace CryptoTests;

public class Slide_Coder_Test
{
    [Fact]
    public void Encryptor_Test()
    {
        var encryptorWithData = CreateEncryptor();
        var encryptor = encryptorWithData.Encryptor;
        var data = encryptorWithData.Data;
        var key = encryptorWithData.Key;

        var encryptedData = encryptor.Encrypt(data, key);
        var decryptedData = encryptor.Decrypt(encryptedData, key);

        Assert.Equal(data.Length, decryptedData.Length);
        Assert.Equal(encryptedData.Length, decryptedData.Length);

        var dataHash = BitConverter.ToString(SHA256.Create().ComputeHash(data));
        var decryptedHash = BitConverter.ToString(SHA256.Create().ComputeHash(decryptedData));

        Assert.Equal(dataHash, decryptedHash);
    }

    private EncryptorWithData CreateEncryptor()
    {
        Random random = new Random();
        int[] bitSizes = new int[] { 128, 192, 256, 512 };
        var keySize = bitSizes[random.Next(0, bitSizes.Length)];
        var blockSize = bitSizes[random.Next(0, bitSizes.Length)];
        var encryptor = new SlideCodeEncryptor(blockSize, keySize);
        var dataBytes = RandomNumberGenerator.GetBytes(random.Next((int)(Int32.MaxValue / 10e6), (int)(Int32.MaxValue / 10e4)));
        var keyBytes = RandomNumberGenerator.GetBytes(keySize);
        return new EncryptorWithData
        {
            Encryptor = encryptor,
            Data = Utils.FillRemnantBytes(dataBytes, blockSize / 8),
            Key = keyBytes
        };
    }

    private class EncryptorWithData
    {
        internal SlideCodeEncryptor Encryptor { get; set; }
        internal byte[] Data { get; set; }
        internal byte[] Key { get; set; }
    }
}
