var fileBytes = await File.ReadAllBytesAsync(@"C:\Users\mtgin\Downloads\NIST_UI_final.rar");
const int blockSize = 128;

if (blockSize != 128) throw new NotImplementedException("Only 128 bit is supported yet :(");

var blockSizeBytes = 128 / 8;

var remnantBytes = fileBytes.Length % blockSizeBytes;
var bytesToAdd = remnantBytes == 0 ? remnantBytes : blockSizeBytes - remnantBytes;
byte[] filledArray = new byte[fileBytes.Length + bytesToAdd];
Array.Copy(fileBytes, 0, filledArray, 0, fileBytes.Length);

#region Encryption
for(int r = 0; r < filledArray.Length; r+= blockSizeBytes)
{
    var linedSquare = new byte[blockSizeBytes];
    Array.Copy(filledArray, r, linedSquare, 0, blockSizeBytes);
    var edgeLength = (int)Math.Sqrt(linedSquare.Length);
    byte[,] square = new byte[edgeLength, edgeLength];
    for(int a = 0; a < edgeLength; a++)
    {
        for (int b = 0; b < edgeLength; b++)
        {
            square[a, b] = linedSquare[a * edgeLength + b];
        }
    }

    for(int i = 0; i < edgeLength; i++)
    {
        byte sum = 0;
        for(int j = 0; j < edgeLength; j++)
        {
            sum += square[i, j];
        }
        square[i, i] = sum;
        for(int j = 0; j < edgeLength; j++)
        {
            if (j == i) continue;
            square[j, i] = (byte)((square[j, i] + square[i, i]) % 256);
        }
    }
}
#endregion

Console.ReadLine();