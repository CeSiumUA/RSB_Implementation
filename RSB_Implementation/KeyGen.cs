using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSB_Implementation
{
    public static class KeyGen
    {
        public static byte[] CreateRandomRoundKey()
        {
            byte[] buffer = new byte[32];
            Random random= new Random();
            random.NextBytes(buffer);
            return buffer;
        }
    }
}
