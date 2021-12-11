using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSB_GUI.Utils
{
    public class SmallFileBytes : FileBytes
    {
        public override long Length
        {
            get
            {
                if (Bytes == null) return 0;
                return Bytes.Length;
            }
        }
        public byte[] Bytes;
    }
}
