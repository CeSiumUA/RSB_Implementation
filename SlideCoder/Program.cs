using RSB.Core;
using System.Collections;
using System.Diagnostics;
using System.Security.Cryptography;
using RSB.Core.GrayScheme;

Stopwatch stopwatch = Stopwatch.StartNew();

const int KeyLength = 128;
const int BlockLength = 128;
int blockSizeBytes = BlockLength / 8;
int rounds = KeyLength / 32;
byte[] rawData = File.ReadAllBytes(@"sample.txt");

var data = Utils.FillRemnantBytes(rawData, blockSizeBytes);
byte[] keyBytes = Utils.GenerateBytes(KeyLength / 8);

for(int i = 0; i < data.Length / blockSizeBytes; i++)
{
    
    for (int r = 0; r < rounds; r++)
    {
        var bytesToProcess = new byte[blockSizeBytes];
        Array.Copy(data, i * blockSizeBytes, bytesToProcess, 0, blockSizeBytes);
        var roundKeyBytes = new byte[4];
        Array.Copy(keyBytes, r * 4, roundKeyBytes, 0, 4);
        //if ((r + 1) % 2 == 0)
        //{
            var resultBytes = LeftGrayScheme.Encrypt(bytesToProcess, roundKeyBytes);
        //}
        //else
        //{
        //    bytesToProcess = RightGrayScheme.Encrypt(bytesToProcess, roundKeyBytes);
        //}
        Array.Copy(resultBytes, 0, data, i * blockSizeBytes, blockSizeBytes);
    }
}

File.WriteAllBytes("result.txt", data);
data = File.ReadAllBytes("result.txt");

for (int i = 0; i < data.Length / blockSizeBytes; i++)
{
    for (int r = 0; r < rounds; r++)
    {
        var bytesToProcess = new byte[blockSizeBytes];
        Array.Copy(data, i * blockSizeBytes, bytesToProcess, 0, blockSizeBytes);
        var roundKeyBytes = new byte[4];
        Array.Copy(keyBytes, r * 4, roundKeyBytes, 0, 4);
        //if ((r + 1) % 2 == 0)
        //{
            var resultBytes = LeftGrayScheme.Decrypt(bytesToProcess, roundKeyBytes);
        //}
        //else
        //{
        //    bytesToProcess = RightGrayScheme.Decrypt(bytesToProcess, roundKeyBytes);
        //}
        Array.Copy(resultBytes, 0, data, i * blockSizeBytes, blockSizeBytes);
    }
}

File.WriteAllBytes("afterresult.txt", data);

stopwatch.Stop();
Console.WriteLine(stopwatch.ElapsedMilliseconds);
Console.ReadLine();