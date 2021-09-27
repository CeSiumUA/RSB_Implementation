using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSB_GUI.Utils
{
    public class LargeFileBytes : FileBytes
    {
        public override long Length
        {
            get
            {
                if (Bytes == null) return 0;
                long bytesCount = 0;
                foreach(var array in Bytes)
                {
                    bytesCount += array.Length;
                }
                return bytesCount;
            }
        }
        public List<byte[]> Bytes;
    }
}
