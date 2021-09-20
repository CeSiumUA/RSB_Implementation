using RSB_Implementation;
using System.Text;

string rawText = "Hello world!";
const int blockLength = 256;


var rawTextBytes = Encoding.UTF8.GetBytes(rawText).ToList();

var roundKey = KeyGen.CreateRandomRoundKey();

var byteBlocksCount = blockLength / 8;

int remnantBytesCount = byteBlocksCount - rawTextBytes.Count % byteBlocksCount;

rawTextBytes.AddRange(Enumerable.Repeat<byte>(0, remnantBytesCount));

Console.ReadLine();