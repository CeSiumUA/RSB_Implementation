using RSB.Core;
using System.Collections;
using System.Security.Cryptography;

const int KeyLength = 128;
const int BlockLength = 192;
int blockSizeBytes = BlockLength / 8;
int rounds = KeyLength / 32;
int graySchemeBytes = 2;
byte[] rawData = File.ReadAllBytes(@"C:\Users\mtgin\Downloads\DALF.doc");

var data = Utils.FillRemnantBytes(rawData, blockSizeBytes);

byte[] keyBytes = Utils.GenerateBytes(KeyLength / 8);

for(int i = 0; i < data.Length / blockSizeBytes; i++)
{
    var bytesToProcess = new byte[blockSizeBytes];
    Array.Copy(data, 0, bytesToProcess, i * blockSizeBytes, blockSizeBytes);
    for (int r = 0; r < rounds; r++)
    {
        var roundKeyBytes = new byte[4];
        Array.Copy(keyBytes, r * 4, roundKeyBytes, 0, 4);
        var runs = roundKeyBytes.Length / graySchemeBytes;
        for(int run = 0; run < runs; run++)
        {
            var runKeyBytes = new byte[graySchemeBytes];
            Array.Copy(roundKeyBytes, run * graySchemeBytes, runKeyBytes, 0, runKeyBytes.Length);
        }
    }
}

Console.ReadLine();