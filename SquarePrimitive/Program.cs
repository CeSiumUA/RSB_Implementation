using SquarePrimitive;


const int blockSize = 128;

if (blockSize != 128) throw new NotImplementedException("Only 128 bit is supported yet :(");

QuadEncryptor quadEncryptor = new QuadEncryptor(blockSize);

#region Encryption
var rawFileBytes = await File.ReadAllBytesAsync(@"C:\Users\mtgin\Downloads\NIST_UI_final.rar");
var encryptedArray = quadEncryptor.Encrypt(rawFileBytes);
await File.WriteAllBytesAsync("result.encr", encryptedArray);
Console.WriteLine("Encrypted successfully!");
#endregion

#region Decryption
var encryptedFileBytes = await File.ReadAllBytesAsync("result.encr");
var decryptedArray = quadEncryptor.Decrypt(encryptedFileBytes);
await File.WriteAllBytesAsync("result1.rar", decryptedArray);
Console.WriteLine("Decrypted successfully!");
#endregion

#region CompareArrays
for (int i = 0; i < rawFileBytes.Length; i++)
{
    if (rawFileBytes[i] != decryptedArray[i])
    {
        Console.WriteLine($"Not Equal on {i}, source: {rawFileBytes[i]}, decrypted: {decryptedArray[i]}");
    }
}
#endregion
Console.WriteLine("Check completed");
Console.ReadLine();