using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSB_GUI.Utils
{
    [Serializable]
    internal class Testing
    {
        public int Variant { get; set; }
        public int Rounds { get; set; }
        public int BlockLength { get; set; }
        public double Entropy { get; set; }
        public double TimeToComplete { get; set; }
    }
}
