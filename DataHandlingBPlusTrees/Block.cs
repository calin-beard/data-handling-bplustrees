using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingBPlusTrees
{
    public class Block
    {
        public byte[] Bytes { get; set; }

        public Block()
        {
            this.Bytes = new byte[4096];
        }

        public static int Size()
        {
            return 4096;
        }
    }
}
