using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingBPlusTrees
{
    class RecordPointer<K>
    {
        public K Value { get; private set; }
        public int Block { get; private set; }
        public int Offset { get; private set; }

        public RecordPointer() { }

        public RecordPointer(K _value, int _block, int _offset)
        {
            this.Value = _value;
            this.Block = _block;
            this.Offset = _offset;
        }
    }
}
