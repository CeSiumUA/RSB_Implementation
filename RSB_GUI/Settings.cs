﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSB_GUI
{
    public class Settings
    {
        public int BlockPower { get; set; }
        public int ShiftValue { get; set; }
        public string InputFilePath { get; set; } = string.Empty;
        public string OutputFilePath { get; set; } = string.Empty;
        public bool UseCommonKey { get; set; }
        public byte[] CommonKey { get; set; }
    }
}