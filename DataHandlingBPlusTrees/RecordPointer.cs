using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingBPlusTrees
{
    class RecordPointer
    {
        public int Block { get; private set; }
        public int Offset { get; private set; }

        public RecordPointer() { }

        public RecordPointer(int _block, int _offset)
        {
            this.Block = _block;
            this.Offset = _offset;
        }
    }
}
