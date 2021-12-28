using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace RSB_GUI
{
    [Serializable]
    public class Settings
    {
        public int BlockPower { get; set; }
        public int LogBlockLength { get; set; } = 128;
        public int ShiftValue { get; set; }
        public string InputFilePath { get; set; } = string.Empty;
        public string OutputFilePath { get; set; } = string.Empty;
        public bool UseCommonKey { get; set; }
        public byte[] CommonKey { get; set; }
        public int CommonKeyLength { get; set; } = 128;
        public bool UseEncryption { get; set; } = true;
        public int TableColumnsCount { get; set; } = 8;
        public bool UseVariant1 { get; set;} = true;
        public bool UseVariant2 { get; set;} = false;
        public bool UseVariant3 { get; set;} = false;
        public int SelectedRoundValues { get; set; } = 1;
        public const string FileName = "settings";
        public void Save(string fileName = "")
        {
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = FileName;
            }
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                binaryFormatter.Serialize(fs, this);
            }
        }
    }
}
