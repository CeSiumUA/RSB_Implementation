using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSB_GUI
{
    [Serializable]
    public class Settings
    {
        public int BlockPower { get; set; }
        public int LogBlockLength { get; set; } = 8;
        public int ShiftValue { get; set; }
        public string InputFilePath { get; set; } = string.Empty;
        public string OutputFilePath { get; set; } = string.Empty;
        public bool UseCommonKey { get; set; }
        public byte[] CommonKey { get; set; }
        public bool UseEncryption { get; set; } = true;
        public int TableColumnsCount { get; set; } = 8;
    }
}
