using RSB_Implementation;
using System.Security.Cryptography;
using System.Text;

string rawText = "Hello world!";
const int blockLength = 64;
const int encryptionSteps = 2;
const int roundKeyLength = 32;
const int keyLength = 64;

int encryptionRounds = encryptionSteps * (int)Math.Pow(2, blockLength / 64);

var commonKey = GenerateCommonKey(keyLength);

var rawTextBytes = Encoding.UTF8.GetBytes(rawText).ToList();

var roundKey = KeyGen.CreateRandomRoundKey();

var byteBlocksCount = blockLength / 8;

int remnantBytesCount = byteBlocksCount - rawTextBytes.Count % byteBlocksCount;

rawTextBytes.AddRange(Enumerable.Repeat<byte>(0, remnantBytesCount));

for(int iteration = 0; iteration < encryptionSteps; iteration++)
{
    var splittedKey = commonKey.Skip(iteration * roundKeyLength / 8).Take(roundKeyLength / 8).ToArray();
}

Console.ReadLine();


byte[] GenerateCommonKey(int commonKeyLength)
{
    using (var rng = RandomNumberGenerator.Create())
    {
        byte[] key = new byte[commonKeyLength / 8];
        rng.GetBytes(key);
        return key;
    }
}