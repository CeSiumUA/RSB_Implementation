var fileBytes = await File.ReadAllBytesAsync(@"C:\Users\mtgin\Downloads\NIST_UI_final.rar");
const int blockSize = 128;

if (blockSize != 128) throw new NotImplementedException("Only 128 bit is supported yet :(");

var blockSizeBytes = 128 / 8;

var remnantBytes = fileBytes.Length % blockSizeBytes;
var bytesToAdd = remnantBytes == 0 ? remnantBytes : blockSizeBytes - remnantBytes;
byte[] filledArray = new byte[fileBytes.Length + bytesToAdd];
Array.Copy(fileBytes, 0, filledArray, 0, fileBytes.Length);

#region Encryption
for(int i = 0; i < filledArray.Length; i+= blockSizeBytes)
{
    var linedSquare = new byte[blockSizeBytes];
    Array.Copy(filledArray, i, linedSquare, 0, blockSizeBytes);
    var edgeLength = (int)Math.Sqrt(linedSquare.Length);
    byte[,] square = new byte[edgeLength, edgeLength];
    for(int a = 0; a < edgeLength; a++)
    {
        for (int b = 0; b < edgeLength; b++)
        {
            square[a, b] = linedSquare[a * edgeLength + b];
        }
    }
}
#endregion

Console.ReadLine();